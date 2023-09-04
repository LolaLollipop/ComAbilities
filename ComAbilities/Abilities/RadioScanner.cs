using ComAbilities.Localizations;
using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp079;
using Respawning;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace ComAbilities.Abilities
{
    //[Hotkey]
    public sealed class RadioScanner : Ability, IHotkeyAbility, IReductionAbility, ICooldownAbility
    {

        private readonly static ComAbilities Instance = ComAbilities.Instance;
        private readonly static RadioScannerT ScannerT = Instance.Localization.RadioScanner;


        private static PlayerTrackerConfig _config => Instance.Config.PlayerTracker;

        public RadioScanner(CompManager compManager) : base(compManager) { }

        public static List<PlayerRoles.PlayableScps.Scp079.Scp079Role> ActiveScanners = new();
        // --------------------
        public override string Name { get; } = ScannerT.Name;
        public override string Description { get; } = ScannerT.Description;
        public override float AuxCost { get; } = _config.AuxCost;
        public override int ReqLevel { get; } = _config.Level;
        public override string DisplayText => string.Format(ScannerT.DisplayText, Instance.Localization.Shared.Hotkeys[HotkeyButton].ToUpper(), AuxCost);
        public string ActiveDisplayText { get; } = ScannerT.ActiveText;
        public override bool Enabled => _config.Enabled;
        public float AuxModifier { get; } = _config.AuxMultiplier;

        public AllHotkeys HotkeyButton { get; } = AllHotkeys.Grenade;

        public float CooldownLength { get; } = _config.Cooldown;
        public bool OnCooldown => _cooldown.Active;

        public bool IsActive { get; set; } = false;

        private Cooldown _cooldown { get; } = new();

        private static int _minTimeToStop { get; } = 0;

        // --------------------

        public void Trigger()
        {
            if (IsActive && _cooldown.RunningFor() > _minTimeToStop) // 3 seconds
            {
                IsActive = false;
                ActiveScanners.Remove(CompManager.Role!.Base);
                _cooldown.Start(CooldownLength);
                CompManager.ActiveAbilities.Remove(this);
                CompManager.DisplayManager.Update();

            }
            else
            {
                //  Trigger();
                IsActive = true;
                ActiveScanners.Add(CompManager.Role!.Base);
                CompManager.ActiveAbilities.Add(this);
                CompManager.DisplayManager.Update();
            }
        }

        public int GetETA()
        {   
            if (_cooldown == null) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            float? eta = _cooldown.GetETA();
            if (!eta.HasValue) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            return (int)eta;
        }

        public override void KillTasks()
        {
            if (CompManager.Role != null)
            ActiveScanners.Remove(CompManager.Role.Base);
        }
    }
}
