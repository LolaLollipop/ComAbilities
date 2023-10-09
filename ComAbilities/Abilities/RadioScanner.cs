using ComAbilities.Localizations;
using ComAbilities.Objects;
using ComAbilities.Types;

namespace ComAbilities.Abilities
{
    //[Hotkey]
    public sealed class RadioScanner : Ability, IHotkeyAbility, IReductionAbility, ICooldownAbility
    {
        private readonly static RadioScannerT ScannerT = Instance.Localization.RadioScanner;
        private static PlayerTrackerConfig config => Instance.Config.PlayerTracker;
        private Cooldown cooldown = new();

        private const int _minTimeToStop = 500;

        public RadioScanner(CompManager compManager) : base(compManager) { }

        public static List<PlayerRoles.PlayableScps.Scp079.Scp079Role> ActiveScanners { get; } = new();
        // --------------------
        public override string Name { get; } = ScannerT.Name;
        public override string Description { get; } = ScannerT.Description;
        public override float AuxCost { get; } = config.AuxCost;
        public override int ReqLevel { get; } = config.Level;
        public override string DisplayText => string.Format(ScannerT.DisplayText, Instance.Localization.Shared.Hotkeys[HotkeyButton].ToUpper(), AuxCost);
        public string ActiveDisplayText { get; } = ScannerT.ActiveText;
        public override bool Enabled => config.Enabled;
        public float AuxModifier { get; } = config.AuxMultiplier;

        public AllHotkeys HotkeyButton { get; } = AllHotkeys.HoldReload;

        public float CooldownLength { get; } = config.Cooldown;
        public bool OnCooldown => cooldown.Active;

        public bool IsActive { get; set; } = false;

        // --------------------

        public void Trigger()
        {
            if (IsActive && cooldown.RunningFor() > _minTimeToStop)
            {
                IsActive = false;
                ActiveScanners.Remove(CompManager.Role!.Base);
                cooldown.Start(CooldownLength);
                CompManager.ActiveAbilities.Remove(this);
                CompManager.Display.Update();
            }
            else
            {
                //  Trigger();
                IsActive = true;
                ActiveScanners.Add(CompManager.Role!.Base);
                CompManager.ActiveAbilities.Add(this);
                CompManager.Display.Update();
            }
        }

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public override void CleanUp()
        {
            if (CompManager.Role != null)
                ActiveScanners.Remove(CompManager.Role.Base);
        }
    }
}
