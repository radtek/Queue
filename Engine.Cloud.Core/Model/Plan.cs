using System.Linq;

namespace Engine.Cloud.Core.Model
{
    public partial class Plan
    {
        public Plans PlanType
        {
            get
            {
                var elastic = new[] {9, 11, 12};
                if (elastic.Contains(PlanId))
                    return Plans.Elastico;

                if (PlanId == 18)
                    return Plans.Pontual;

                if (PlanId == 15)
                    return Plans.Anual;

                if (PlanId == 16)
                    return Plans.Bienal;

                if (PlanId == 17)
                    return Plans.Trienal;
                
                return Plans.Stank;
            }
        }
    }
}
