using System;
using System.ComponentModel;

namespace Engine.Cloud.Core.Model
{
    public enum TypeHypervisor
    {
        [Description("SP1-C1")]
        XEN_SP1 = 1,
        [Description("RJ1-C1")]
        XEN_RJ1 = 2,
        [Description("SP1-C2")]
        KVM_SP1 = 3,
        [Description("SP1-C3")]
        HYPERV_SP1 = 4,
        [Description("RJ1-C2")]
        KVM_RJ1 = 5
    }

    public enum BackupRegion
    {
        [Description("SP1")]
        SP1 = 1,
        [Description("RJ1")]
        RJ1 = 2
    }

    public enum TypeFirewallInstall
    {
        Standalone = 1,
        Failover = 2
    }

    public enum Region
    {
        [Description("SP1")]
        SP1 = 1,
        [Description("RJ1")]
        RJ1 = 2
    }

    public enum TypeBackup
    {
        [Description("Cloud Backup")]
        CloudBackup = 1,
        [Description("Backup Online")]
        BackupOnline = 2
    }

    public enum TypeDistributionVlan
    {
        [Description("Estático")]
        Static = 1,
        [Description("DHCP")]
        DHCP = 2
    }
    
    public enum LoadBalanceIpStatus
    {
        [Description("active")]
        Active = 1,
        [Description("backup")]
        Backup = 2,
        [Description("maintenance")]
        Maintenance = 3,
        [Description("down")]
        Down = 4
    }

    public enum BookServiceGraph
    {
        None = -1,
        Memory = 0,
        Cpu = 1,
        Disc = 2,
        NetworkIn = 3,
        NetworkOut = 4,
        Backup = 5 
    }

    public enum StatusBlock
    {
        [Description("")]
        Nothing = 0,
        [Description("Em manutenção")]
        Maintenance = 1,
        [Description("Em alteração")]
        Busy = 2,
        [Description("Suspenso")]
        Suspend = 3,
        [Description("Falha ao buscar informação")]
        Unknown = 4
    }


    public enum HostStatus
    {
        [Description("Ativo")]
        RUNNING = 0,
        [Description("Iniciando")]
        STARTING = 1,
        [Description("Sobrecarregado")]
        HEAVY_LOADED = 2,
        [Description("Falha")]
        FAILURE = 3,
        [Description("Alerta")]
        WARNING = 4,
        [Description("Desativado")]
        DEPRECATED = 5,
        [Description("Manutenção")]
        MAINTENANCE = 6,
        [Description("Indisponível")]
        UNAVAILABLE = 7,
        [Description("Desconhecido")]
        UNKNOWN = 8
    }

    public enum Status
    {
        [Description("Indefinido")]
        Undefined = 0,
        [Description("Ativo")]
        Active = 1,
        [Description("Suspenso")]
        Suspended = 2,
        [Description("Cancelado")]
        Deleted = 3
    }

    public enum TypeInstallOrigin
    {
        [Description("Admin")]
        Admin = 1,
        [Description("Panel")]
        Panel = 2,
        [Description("Clone")]
        Clone = 3,
        [Description("Gestor")]
        Gestor = 4,
        [Description("Firewall")]
        Firewall = 5
    }

    public enum TypeImage
    {
        [Description("Public")]
        Public = 0,
        [Description("Private")]
        Private = 1
    }

    public enum RemoteStatus
    {
        [Description("Deletado")]
        DontExist = -1,
        [Description("Ligado")]
        Online = 1,
        [Description("Desligado")]
        Offline = 2,
        [Description("Pausado")]
        Suspended = 3
    }



    public enum TypeDisk
    {
        [Description("SO")]
        SO = 1,
        [Description("Dados")]
        Data = 2
    }

    public enum TypeIp
    {
        [Description("Principal")]
        Principal = 1,
        [Description("Adicional")]
        Additional = 2
    }

    public enum TypeSnapshot
    {
        [Description("Disco")]
        Disk = 1,
        [Description("Servidor")]
        Server = 2
    }

    public enum TypeFirewall
    {
        [Description("Usuário")]
        User = 1,
        [Description("Servidor")]
        Server = 2,
        [Description("Geral")]
        General = 3
    }

    public enum TypeManagement
    {
        [Description("Não gerenciado")]
        Client = 0,
        [Description("Compartilhado")]
        Shared = 2
    }
   
    public enum BookServiceStatus
    {
        [Description("Aguardando")]
        Waiting = 0,
        [Description("Processando")]
        Running = 1,
        [Description("Concluído")]
        Completed = 2,
        [Description("Falha")]
        Failed = 3,
        [Description("Incompleto")]
        Warning = 4
    }
    public enum TypeMapping
    {
        [Description("None")]
        None = 0,
        [Description("HSphere")]
        HSphere = 1,
        [Description("Cloud")]
        Cloud = 2,
        [Description("Adittional Ip")]
        AditionalIp = 4,
        [Description("Load Balance")]
        LoadBalance = 5,
        [Description("Backup")]
        Backup = 6,
        [Description("Private Image")]
        PrivateImage = 7
    }

    public enum StatusQueueAction
    {
        [Description("ON_QUEUE")]
        ON_QUEUE = 0,
        [Description("PENDING")]
        PENDING = 1,
        [Description("FAILED")]
        FAILED = 2,
        [Description("COMPLETED")]
        COMPLETED = 3,
        [Description("ROLLED_BACK")]
        ROLLED_BACK = 4,
        [Description("CANCELED")]
        CANCELED = 5,
    }

    public enum StatusService
    {
        SUCCESS = 1,
        FAILED = 2
    }

    public enum Permission
    {
        [Description("Sem acesso")]
        NONE = 0,
        [Description("Somente Leitura")]
        READONLY = 1,
        [Description("Administrador")]
        ADMIN = 2
    }

    public enum TypeQueueActionReference
    {
        [Description("Todos")]
        ALL = 0,
        [Description("Cliente")]
        CLIENT = 1,
        [Description("Servidores")]
        VM_SERVER = 2,
        [Description("Backup")]
        BACKUP = 3,
        [Description("Load Balance")]
        LOADBALANCE = 4,
        [Description("Imagem Privada")]
        PRIVATE_IMAGE = 5,
        [Description("Mail Manager")]
        MAILMANAGER = 6,
        [Description("Private Vlan")]
        PRIVATEVLANMANAGER = 7
    }
    
    public enum Service
    {
        [Description("CD")]
        CloudServer,
        [Description("BK")]
        Backup,
        [Description("365")]
        Office365,
        [Description("MAIL")]
        Email,
        [Description("MSS")]
        SimpleStorage,
        [Description("EM")]
        MailMarketing,
        [Description("HS")]
        Hospedagem,
        [Description("SB")]
        SiteBuilder,
        [Description("CGER")]
        CloudManager,
        [Description("DT")]
        CloudDatabase,
        [Description("BK2")]
        BackupOnline,
        [Description("LB")]
        LoadBalance,
        [Description("DBS")]
        CloudDataBase
    }

    public enum ProductType
    {
        [Description("Servidores")]
        VM_SERVER = 1,
        [Description("Cloud Backup")]
        CLOUD_BACKUP = 2,
        [Description("Backup Online")]
        BACKUP_ONLINE = 3,
        [Description("Load Balance")]
        LOADBALANCE = 4,
        [Description("Financeiro")]
        FINANCIAL = 5,
        [Description("Suporte")]
        SUPPORT = 6,
        [Description("Dns")]
        DNS = 7,
        [Description("Email Corporativo")]
        MAIL_MANAGER = 8,
        [Description("Hospedagem")]
        HOSTING = 9,
        [Description("Imagem Privada")]
        PRIVATE_IMAGE = 10,
        [Description("Rede Privada")]
        PRIVATE_VLAN = 11,
        [Description("Eventos zabbix")]
        EVENT_ZABBIX = 12
    }

    public enum TypeResource
    {
        [Description("Vcpu")]
        vcpu = 1,
        [Description("Frequencia")]
        frequency = 2,
        [Description("Memória")]
        memory = 3,
        [Description("Transferencia")]
        transfer = 4,
        [Description("Banda")]
        bandwidth = 5,
        [Description("Disco")]
        disk = 6,
        [Description("Snapshot")]
        snapshot = 11
    }

    public enum BillingGroupTicketType
    {
        [Description("Servidor Cloud")]
        Server = 1,
        [Description("Customer")]
        Customer = 2,
        [Description("Load Balance")]
        LoadBalance = 3
    }

    public enum PlanModality
    {
        [Description("Pré-pago")]
        PrePago = 1,
        [Description("Pós-pago")]
        PosPago = 2
    }

    public enum PeriodMmm
    {
        Mensal = 1,
        Trimestral = 2,
        Semestral = 3,
        Anual = 4
    }

    public enum Plans
    {
        [Description("Plano Pontual")]
        Pontual,
        [Description("Elástico")]
        Elastico,
        [Description("Estanque")]
        Stank,
        [Description("12 meses")]
        Anual,
        [Description("24 meses")]
        Bienal,
        [Description("36 meses")]
        Trienal
    }

    public enum PlanContractType
    {
        [Description("Plano Pontual")]
        Punctual = 18,
        [Description("12 meses")]
        Annual = 15,
        [Description("24 meses")]
        Biennial = 16,
        [Description("36 meses")]
        Triennial = 17
    }

    public enum CardFlags
    {
        Visa = 1,
        Master = 2,
        Mastercard = 3, 
        Amex = 4,
        Diners = 5
    }

    public enum SO
    {
        [Description("linux")]
        Linux = 0,
        [Description("windows")]
        Windows = 1,
        [Description("unix")]
        Unix = 2
    }

    public enum StatusTicket
    {
        [Description("Aberto ou em andamento")]
        Open = 1,
        [Description("Encerrado")]
        Closed = 2
    }

    public enum TicketNote
    {
        [Description("Não Avaliado")]
        Undefined = 0,
        [Description("Péssimo")]
        Terrible = 1,
        [Description("Ruim")]
        Bad = 2,
        [Description("Bom")]
        Good = 4,
        [Description("Ótimo")]
        Great = 5,
    }

    public enum TypeAction
    {
        [Description("Alteração - VirtualMachine")]
        Server_Management = 1,
        [Description("Alteração - Backup")]
        Backup_Management = 5,
        [Description("Ação não bloqueante")]
        NonBlockingChange = 6,
        [Description("Auditoria painel")]
        AuditActionPanel = 9,
        [Description("Alteração - Load balance")]
        LoadBalance_Management = 10,
        [Description("Alteração - Private Image")]
        PrivateImage_Management = 11,
        [Description("Alteração - Mail Manager")]
        MailManager_Management = 12,
        [Description("Alteração - Vlan Privada")]
        PrivateVlan_Management = 13
    }

    public enum UserType
    {
        [Description("Usuario Principal")]
        Master = 1,
        [Description("Usuario Adicional")]
        AdditionalUser = 2,
        [Description("Usuario do Admin")]
        InternalUser = 3,
        [Description("Usuario com email MMM")]
        EmailMMM = 5
    }

    public enum EventSeverity
    {
        [Description("Não classificado")]
        Notclassified = 0,
        [Description("Info")]
        Information = 1,
        [Description("Alerta")]
        Warning = 2,
        [Description("Médio")]
        Average = 3,
        [Description("Alto")]
        High = 4,
        [Description("Desastre")]
        Disaster = 5
    }

    public enum TypeActionStep
    {
        [Description("Instalar servidor")]
        Server_InstallServer = 1,
        [Description("Remover servidor")]
        Server_UninstallServer = 2,
        [Description("Desligar (forçado) servidor")]
        Server_PowerOffServer = 3,
        [Description("Ligar servidor")]
        Server_PowerOnServer = 4,
        [Description("Atualizar servidor")]
        Server_Refresh = 52,
        [Description("Reiniciar servidor")]
        Server_RebootServer = 5,
        [Description("Pausar servidor")]
        Server_PauseServer = 6,
        [Description("Resume servidor")]
        Server_ResumeServer = 7,
        [Description("Reinstalar servidor")]
        Server_ReinstallServer = 8,
        [Description("Reset servidor")]
        Server_ResetServer = 9,
        [Description("Desligar servidor")]
        Server_ShutdownServer = 10,
        [Description("Criar disco")]
        Server_CreateDisk = 11,
        [Description("Expandir de disco")]
        Server_UpgradeDisk = 12,
        [Description("Remover de disco")]
        Server_DeleteDisk = 13,
        [Description("Unificar de disco")]
        Server_UnifyDisk = 14,
        [Description("Criar snapshot")]
        Server_CreateSnapshot = 15,
        [Description("Remover de snapshot")]
        Server_DeleteSnapshot = 16,
        [Description("Restaurar snapshot")]
        Server_RestoreSnapshot = 17,
        [Description("Alterar memória/vcpu")]
        Server_ChangeMemoryVcpu = 18,
        [Description("Alterar largura de banda")]
        Server_ChangeBandwidth = 19,
        [Description("Enviar de instruções")]
        Server_SendInstalServerInstructions = 20,
        [Description("Registrar bilhetagem")]
        Server_BillingServer = 21,
        [Description("Alterar ip adicional")]
        Server_AdditionalIp = 22,
        [Description("Alterar regra de firewall")]
        Server_FirewallRule = 23,
        [Description("Suspender do servidor")]
        Server_SupendServer = 24,
        [Description("Suspender do servidor do painel")]
        Server_SuspendPanelServer = 42,
        [Description("Reativar servidor")]
        Server_ReactivateServer = 25,
        [Description("Envio de notificações de ação")]
        Server_SendChangeConfigurationInstructions = 26,
        [Description("Criar pedido de ip adicional")]
        Server_CreateIpOrder = 29,
        [Description("Cancelar pedido de ip adicional")]
        Server_CancelIpOrder = 30,
        [Description("Criar Clone")]
        Server_CreateClone = 31,
        [Description("Criar pedido de licença do servidor")]
        Server_CreateLicenseOrder = 32,
        [Description("Configurar interface de rede na API")]
        Server_ConfigureNetworkInterface = 33,
        //[Description("Liberar interface/banda de rede para VM")] TODO : reaproveitar esse ID:51
        //Server_UnRegisterNetworkInterface = 51,
        [Description("Criar pedido servidor")]
        Server_CreateOrder = 53,
        [Description("Cancelar pedido servidor")]
        Server_CancelOrder = 54,
        [Description("Criar interface/banda de rede na API")]
        Server_CreateNetworkInterface = 76,
        [Description("Excluir interface/banda de rede na API")]
        Server_DeleteNetworkInterface = 77,

        [Description("Migrar Host")]
        Server_HostMigrate = 41,
        [Description("Migrar VM")]
        Server_VmMigrate = 43,

        /// <summary>
        /// Backup
        /// </summary>
        [Description("Enviar de e-mail de instruções para backup")]
        Backup_SendCreateBackupInstructions = 34,
        [Description("Enviar de e-mail de notificação de mudança no backup")]
        Backup_SendChangeConfigurationInstructions = 35,
        [Description("Registrar bilhetagem de backup")]
        Backup_BillingBackup = 36,
        [Description("Criar pedido backup")]
        Backup_CreateOrder = 37,
        [Description("Enviar de e-mail com nova senha do agente de backup")]
        Backup_SendChangePasswordInstructions = 38,
        [Description("Alterar status de produto backup")]
        Backup_ChangeStatus = 39,
        [Description("Instalar backup")]
        Backup_InstallBackup = 40,
        [Description("Cancelar pedido backup")]
        Backup_CancelOrder = 55,

        /// <summary>
        /// Load Balance
        /// </summary>
        [Description("Instalar Load Balance")]
        LoadBalance_Install = 45,
        [Description("Enviar de e-mail para load balance")]
        LoadBalance_SendEmailInstructions = 46,
        [Description("Registrar pedido no admin de load balance")]
        LoadBalance_Admin = 47,
        [Description("Registrar bilhetagem de load balance")]
        LoadBalance_Billing = 48,
        [Description("Alterar status de load balance")]
        Loadbalance_ChangeStatus = 49,
        [Description("Alterar banda de load balance")]
        Loadbalance_ChangeBandWidth = 50,

        /// <summary>
        /// Private Image
        /// </summary>
        [Description("Criar Imagem Privada")]
        PrivateImage_Create = 56,
        [Description("Enviar e-mail de Imagem Privada")]
        PrivateImage_SendInstructions = 57,
        [Description("Manipular tickets de Imagem Privada no Billing")]
        PrivateImage_Billing = 58,
        [Description("Manipular pedido de Imagem Privada no Admin")]
        PrivateImage_AdminOrder = 59,
        [Description("Alterar status de produto Imagem Privada")]
        PrivateImage_ChangeStatus = 60,

        [Description("Criar Master - MMM")]
        MailManager_CreateMaster = 61,
        [Description("Criar Plano de Email - MMM")]
        MailManager_CreateProductAssign = 62,
        [Description("Criar Assinatura de domínio - MMM")]
        MailManager_CreateDomainAssign = 63,
        [Description("Criar Domínio - MMM")]
        MailManager_CreateDomain = 64,
        [Description("Criar conta de email - MMM")]
        MailManager_CreateAccount = 65,
        [Description("Configurar perfil da conta - MMM")]
        MailManager_SetProfile = 66,
        [Description("Criar pedido no admin - MMM")]
        MailManager_CreateOrderAdmin = 67,
        [Description("Envio de email - MMM")]
        MailManager_SendAccountInstructions = 68,

        [Description("Criar Rede Privada")]
        PrivateVlan_Create = 69,
        [Description("Alterar status de Rede Privada")]
        PrivateVlan_ChangeStatus = 70,
        [Description("Enviar e-mail de Rede Privada")]
        PrivateVlan_SendInstructions = 71,
        [Description("Incluir Network Interface de Rede Privada")]
        PrivateVlan_IncludeNetworkInterface = 72,
        [Description("Excluir Network Interface de Rede Privada")]
        PrivateVlan_ExcludeNetworkInterface = 73,
        [Description("Configurar Firewall Dedicado")]
        Server_ConfigureDedicatedFirewall = 75,
        [Description("Habilitar plano contratado - MMM")]
        MailManager_EnablePlan = 74,

    }
}