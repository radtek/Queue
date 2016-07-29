using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Engine.Cloud.Core.Domain;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.DataContext;
using Engine.Cloud.Core.Utils.Extensions;
using Engine.Cloud.Core.Utils.Logging;
using Utils;

namespace Engine.Cloud.QueueAction
{
    public class QueueActionMonitor
    {
        private readonly EngineCloudDataContext _context;

        private readonly ILogger _logger;

        private const int INTERVAL_INNER_ACTION = 10; // segundos

        public QueueActionMonitor(EngineCloudDataContext context)
        {
            _context = context;
            _logger = LogFactory.GetInstance();
        }

        public long Run()
        {
            var total = 0;

            if (Environment.UserInteractive)
            {
                var queueActions = _context.QueueAction.Where(x => x.QueueActionId == 52518).ToList();

                if (queueActions.Any())
                    ProcessItem_QueueAction(queueActions.FirstOrDefault().QueueActionId, new EngineCloudDataContext());
            }
            else
            {
                var list =
                    _context.QueueAction.Where(
                        x =>
                            ((x.EndDate == null &&
                              DbFunctions.DiffSeconds(x.LastUpdate, DateTime.Now) > INTERVAL_INNER_ACTION) ||
                             (x.StatusActionId == (int)StatusQueueAction.ON_QUEUE)) &&
                            x.StatusActionId != (int)StatusQueueAction.CANCELED &&
                            x.TypeActionId != (int)TypeAction.AuditActionPanel
                        ).ToList();

                Parallel.ForEach(list, item =>
                {
                    using (var context = new EngineCloudDataContext())
                    {
                        if (ThisQueueActionEnabled(item, context))
                        {
                            ProcessItem_QueueAction(item.QueueActionId, context);
                            total++;
                        }
                    }
                });
            }
            return total;
        }

        private void ProcessItem_QueueAction(long queueActionId, EngineCloudDataContext context)
        {
            var _queueActionDomain = new QueueActionDomain(context);

            var queueAction = new QueueActionDomain(context).Get(x => x.QueueActionId == queueActionId);

            try
            {
                // em caso de falha em algum step, invalida a action
                if (queueAction.QueueActionStep.Any(x => x.StatusActionId == (int)StatusQueueAction.FAILED))
                {
                    var step = queueAction.QueueActionStep.Last(x => x.StatusActionId == (int)StatusQueueAction.FAILED);

                    queueAction.Result = step.ResultBody;
                    queueAction.StatusActionId = (int)StatusQueueAction.FAILED;
                    queueAction.EndDate = DateTime.Now;

                    _queueActionDomain.UpdateAction(queueAction);

                    _logger.Log(string.Format(
                        "Engine.Cloud.QueueActionStep:  QueueActionId:{1} /n/nRequestBody:{0} : ({6}), QueueActionStepId:{2}, ResultBody:{3}, ServiceCode:{4}, Attempts:{5}",
                        step.RequestBody, step.QueueActionId, step.QueueActionStepId, step.ResultBody, step.ServiceCode,
                        step.Attempts, step.QueueAction.Name));
                }
                else
                {
                    //STEPS
                    //  busca o primeiro passo ainda não finalizado para processa-lo
                    var step = queueAction.QueueActionStep.FirstOrDefault(x => x.EndDate == null);

                    // #REQUISITO_VISUAL : :(
                    UpdateLastActions(queueAction, _queueActionDomain);

                    if (step != null)
                        ProcessItem_Step(step, context); // Processa o STEP

                    // atualiza a porcentagem
                    var processed = queueAction.QueueActionStep.Count(x => x.StatusActionId == (int)StatusQueueAction.COMPLETED);

                    var total_itens = queueAction.QueueActionStep.Count();
                    queueAction.TotalCompleted = processed.PercentOf(total_itens);
                    _queueActionDomain.UpdateAction(queueAction);


                    //ACTION
                    //  caso não tenha mais steps, finaliza a action
                    if (queueAction.TotalCompleted >= 100)
                    {
                        // pega-se o ultimo passo finalizado (falho ou não)
                        var lastStep = queueAction.QueueActionStep.Last(x => x.EndDate != null);
                        queueAction.StatusActionId = lastStep.StatusActionId;

                        StatusQueueAction statusQueueAction;
                        Enum.TryParse(lastStep.StatusActionId.ToString(), out statusQueueAction);

                        queueAction.Result = statusQueueAction.GetDescription();
                        queueAction.EndDate = DateTime.Now;

                        _queueActionDomain.UpdateAction(queueAction);
                    }

                    if (queueAction.TotalCompleted >= 100 && queueAction.TypeActionId != (int)TypeAction.NonBlockingChange)
                    {
                        // #REQUISITO_VISUAL : Desbloqueia se for um VM_SERVER
                        if (queueAction.QueueActionReference.TypeQueueActionReferenceId == (int)TypeQueueActionReference.VM_SERVER)
                            UnBlockServer(queueAction, context);
                    }

                    if (!string.IsNullOrEmpty(queueAction.Result))
                        _logger.Log(
                            string.Format(
                                "Engine.Cloud.QueueAction: QueueActionId:{0} : TypeActionId:{1} : Result:{2}",
                                queueAction.QueueActionId, queueAction.TypeActionId, queueAction.Result));
                }
            }
            catch (Exception ex)
            {
                _logger.Log(
                    string.Format(
                        "Engine.Cloud.QueueAction : erro ao processar item de fila. QueueActionId {0} : TypeActionId:{1}",
                        queueAction.QueueActionId, queueAction.TypeActionId), ex);
            }
        }


        private static bool ThisQueueActionEnabled(Core.Model.QueueAction action, EngineCloudDataContext context)
        {
            var _queueActionDomain = new QueueActionDomain(context);

            if (action.AfterQueueActionId != null)
            {
                var enabled =
                    _queueActionDomain.Get(x => x.QueueActionId == action.AfterQueueActionId).StatusActionId ==
                    (int)StatusQueueAction.COMPLETED;

                return enabled;
            }

            return true;
        }

        private void ProcessItem_Step(QueueActionStep step, EngineCloudDataContext context)
        {
            try
            {
                var queueActionService = new Core.Domain.Services.QueueAction.QueueActionService(context);
                var _queueActionDomain = new QueueActionDomain(context);
                var _queueActionStepDomain = new QueueActionStepDomain(context);

                // #REQUISITO_VISUAL : informa que esta em alteração
                if (step.QueueAction.QueueActionReference.TypeQueueActionReferenceId == (int)TypeQueueActionReference.VM_SERVER)
                    UpdateStatusServer(step, context);

                var action = step.QueueAction;

                UpdateAttempts(step, _queueActionDomain);


                // caso ja tenha sido inserido, checa o andamento da requisição
                if (step.StatusActionId == (int)StatusQueueAction.PENDING)
                {
                    queueActionService.CheckStepPending(step);
                }

                //  caso ainda não tenha sido processado ...
                if (step.StatusActionId == (int)StatusQueueAction.ON_QUEUE)
                {
                    var stepResult = queueActionService.ExecuteStep(step);

                    // atualiza action
                    action.LastUpdate = DateTime.Now;

                    if (stepResult.Status == StatusQueueAction.FAILED)
                    {
                        action.StatusActionId = (int)StatusQueueAction.FAILED;
                        action.Result = stepResult.Result;
                    }
                    else if (stepResult.Status == StatusQueueAction.COMPLETED)
                        step.StatusActionId = (int)StatusQueueAction.COMPLETED;
                    else
                        action.StatusActionId = (int)StatusQueueAction.PENDING;

                    _queueActionDomain.UpdateAction(action);
                }

                if (step.StatusActionId == (int)StatusQueueAction.COMPLETED ||
                    step.StatusActionId == (int)StatusQueueAction.FAILED ||
                    step.StatusActionId == (int)StatusQueueAction.ROLLED_BACK)
                {
                    step.EndDate = DateTime.Now;
                }

                _queueActionStepDomain.AddUpdateQueueActionStep(step);
            }
            catch (Exception ex)
            {
                _logger.Log(
                    string.Format(
                        "{4} : erro interno ao processar step. QueueActionId {0} : QueueActionStepId:{1}, QueueActionStepId {2}, TypeActionStepId {3}",
                        step.QueueActionId, step.QueueAction.TypeActionId, step.QueueActionStepId, step.TypeActionStepId, LogUtils.GetCurrentMethod(this)), ex);
            }
        }

        private static void UpdateAttempts(QueueActionStep step, QueueActionDomain _queueActionDomain)
        {
            if (step.StatusActionId == (int)StatusQueueAction.PENDING)
                return;

            step.Attempts = step.Attempts + 1;
            _queueActionDomain.UpdateAction(step.QueueAction);
        }

        private static void UpdateStatusServer(QueueActionStep step, EngineCloudDataContext context)
        {
            var arrActions = new int[] { (int)TypeActionStep.Server_SuspendPanelServer };
            if (arrActions.Contains(step.TypeActionStepId))
                return;

            var serverDomain = new ServerDomain(context);
            var server = serverDomain.Get(x => x.ServerId == step.QueueAction.QueueActionReference.ValueId);

            if (step.QueueAction.TypeActionId != (int)TypeAction.NonBlockingChange)
            {
                // marca-se que o servidor esta em alteração/instalação
                server.MessageStatus = step.TypeActionStepId == (int)TypeActionStep.Server_InstallServer
                    ? "Em instalação"
                    : "Em alteração";

                server.StatusBlockId = (int)StatusBlock.Busy;

                serverDomain.AddUpdateServer(server);
            }
        }

        private void UnBlockServer(Core.Model.QueueAction queueAction, EngineCloudDataContext context)
        {
            var arrActions = new int[] { (int)TypeActionStep.Server_SuspendPanelServer };
            if (queueAction.QueueActionStep.Any(s => arrActions.Contains(s.TypeActionStepId)))
                return;

            var serverDomain = new ServerDomain(context);
            var server =
                serverDomain.Get(x => (x.ServerId == queueAction.QueueActionReference.ValueId &&
                                       queueAction.QueueActionReference.TypeQueueActionReferenceId == (int)TypeQueueActionReference.VM_SERVER));

            if (server != null)
            {
                server.MessageStatus = string.Empty;
                server.StatusBlockId = (int)StatusBlock.Nothing;

                serverDomain.AddUpdateServer(server);
                _logger.Log(string.Format("Servidor {0} desbloqueado.", server.Name));
            }

        }

        private static void UpdateLastActions(Core.Model.QueueAction queueAction, QueueActionDomain _queueActionDomain)
        {
            if (queueAction.TotalCompleted == 0)
                queueAction.TotalCompleted = 1;

            _queueActionDomain.UpdateAction(queueAction);
        }
    }
}
