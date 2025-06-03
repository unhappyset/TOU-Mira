using Reactor.Localization;
using Reactor.Localization.Utilities;

namespace TownOfUs.Modules.Localization
{
    public sealed class TaskProvider : LocalizationProvider
    {
        private static StringNames DeathValley = CustomStringName.CreateAndRegister("Death Valley");

        public const SystemTypes DeathValleySystemType = (SystemTypes)250;

        public override bool TryGetStringName(SystemTypes systemType, out StringNames? result)
        {
            if (systemType == DeathValleySystemType)
            {
                result = DeathValley;
                return true;
            }

            return base.TryGetStringName(systemType, out result);
        }
    }
}