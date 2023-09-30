using ComAbilities.Localizations;
using ComAbilities.Objects;
using ComAbilities.Types;
using ComAbilities.Types.RueTasks;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using PlayerRoles.PlayableScps.HumeShield;

namespace ComAbilities.Abilities
{
    //[Hotkey]
    public sealed class RealityScrambler : Ability, IHotkeyAbility, IReductionAbility, ICooldownAbility
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;
        private static RealityScramblerT RealityScramblerT = Instance.Localization.RealityScrambler;
        private static RealityScramblerConfig _config { get; } = Instance.Config.RealityScrambler;
        public RealityScrambler(CompManager compManager) : base(compManager)
        {
        }


        public override string Name { get; } = RealityScramblerT.Name;
        public override string Description { get; } = RealityScramblerT.Description;
        //  public string UsageGuide { get; } = "When activated, the Reality Scrambler allows you to regenerate the Hume Shield of all SCPs, regardless of whether or not they are taking damage. However, this will significantly reduce the rate at which you regenerate Aux Power while active.";
        //  public string Lore { get; } = "Site-02 features █ Scranton Reality Anchors, powerful devices that can nullify the abilities of reality-benders. However, in order to facilitate testing, these can be remotely disabled. Doing so greatly increases the reality-bending powers of the various anomalies with the site, so authorization from the Facility Manager is required.";
        public override float AuxCost { get; } = _config.AuxCost;
        public override int ReqLevel { get; } = _config.Level;
        public override string DisplayText => string.Format(RealityScramblerT.DisplayText, HotkeyButton.ToString().ToUpper(), AuxCost);
        public string ActiveDisplayText { get; } = RealityScramblerT.ActiveText;
        public override bool Enabled => _config.Enabled;

        public AllHotkeys HotkeyButton { get; } = _config.Hotkey;

        public bool OnCooldown => _cooldown.Active;

        public float AuxModifier { get; } = _config.AuxRegenMultiplier;
        public bool IsActive => _regenHumeTask.Enabled;

        public float CooldownLength { get; } = _config.Cooldown;
        private string BroadcastText { get; } = _config.BroadcastText;

        private Cooldown _cooldown { get; } = new();
        private PeriodicTask _regenHumeTask => new(_config.Length, _config.TickTime, HumeRegen, OnFinished);

        public void Toggle()
        {
            // 3 second delay before it can be turned off 
            if (_regenHumeTask.Enabled && _regenHumeTask.RunningFor() > 3000) // 3 seconds
            {
                _regenHumeTask.Interrupt();
            } else if (!_regenHumeTask.Enabled)
            {
              //  Trigger();

                _regenHumeTask.Run();
                CompManager.ActiveAbilities.Add(this);
            }
        }
        public void Trigger()
        {
            Toggle();
        }

        public void HumeRegen()
        {   
            if (this.CompManager.Role is null || this.CompManager.Role.Energy < _config.AuxCostTick)
            {
                return;
            }
            CompManager.Role.Energy -= _config.AuxCostTick; 

            Player[] scps = Helper.GetSCPs();
            foreach (var scp in scps)
            {
                Role role = scp.Role;
                IHumeShieldRole? hume = role as IHumeShieldRole;

                if (hume is not null)
                {
                    Exiled.API.Features.Broadcast bc = new Exiled.API.Features.Broadcast(BroadcastText, (ushort)Math.Ceiling(CooldownLength * 1.5), true, Broadcast.BroadcastFlags.Normal);
                    scp.Broadcast(bc); 
                    HumeShieldModuleBase humeModule = hume.HumeShieldModule;
                    if (humeModule.HsCurrent == humeModule.HsMax) return;
                    humeModule.HsCurrent += Math.Min(humeModule.HsCurrent * (_config.RegeneratePercentTick * 0.01f) , humeModule.HsMax - humeModule.HsCurrent);
                }
            }
        }
        public float GetDisplayETA() => _cooldown.GetDisplayETA();

        public void OnFinished()
        {
            _cooldown.Start(CooldownLength);
            CompManager.ActiveAbilities.Remove(this);
        }

        public override void CleanUp()
        {
            _regenHumeTask.CleanUp();
        }
    }
}
