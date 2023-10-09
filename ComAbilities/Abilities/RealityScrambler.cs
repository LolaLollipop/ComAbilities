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
        private static readonly RealityScramblerT RealityScramblerT = Instance.Localization.RealityScrambler;
        private static RealityScramblerConfig config { get; } = Instance.Config.RealityScrambler;

        private string BroadcastText = config.BroadcastText;
        private readonly Cooldown cooldown = new();
        private readonly PeriodicTask regenHumeTask;

        private const int minTimeToCancel = 3000;

        public RealityScrambler(CompManager compManager) : base(compManager)
        {
            regenHumeTask = new(config.Length, config.TickTime, HumeRegen, OnFinished);
        }


        public override string Name { get; } = RealityScramblerT.Name;
        public override string Description => RealityScramblerT.Description;
        //  public string UsageGuide { get; } = "When activated, the Reality Scrambler allows you to regenerate the Hume Shield of all SCPs, regardless of whether or not they are taking damage. However, this will significantly reduce the rate at which you regenerate Aux Power while active.";
        //  public string Lore { get; } = "Site-02 features █ Scranton Reality Anchors, powerful devices that can nullify the abilities of reality-benders. However, in order to facilitate testing, these can be remotely disabled. Doing so greatly increases the reality-bending powers of the various anomalies with the site, so authorization from the Facility Manager is required.";
        public override float AuxCost => config.AuxCost;
        public override int ReqLevel => config.Level;
        public override string DisplayText => string.Format(RealityScramblerT.DisplayText, HotkeyButton.ToString().ToUpper(), AuxCost);
        public string ActiveDisplayText => RealityScramblerT.ActiveText;
        public override bool Enabled => config.Enabled;

        public AllHotkeys HotkeyButton => config.Hotkey;

        public bool OnCooldown => cooldown.Active;

        public float AuxModifier { get; } = config.AuxRegenMultiplier;
        public bool IsActive => regenHumeTask.Enabled;

        public float CooldownLength { get; } = config.Cooldown;

        public void Toggle()
        {
            // 3 second delay before it can be turned off 
            if (regenHumeTask.Enabled && regenHumeTask.RunningFor() > minTimeToCancel)
            {
                regenHumeTask.End();
            } else if (!regenHumeTask.Enabled)
            {
              //  Trigger();

                regenHumeTask.Run();
                CompManager.ActiveAbilities.Add(this);
            }
        }
        public void Trigger()
        {
            Toggle();
        }

        public void HumeRegen()
        {   
            if (this.CompManager.Role is null || this.CompManager.Role.Energy < config.AuxCostTick)
            {
                return;
            }
            CompManager.Role.Energy -= config.AuxCostTick; 

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
                    humeModule.HsCurrent += Math.Min(humeModule.HsCurrent * (config.RegeneratePercentTick * 0.01f) , humeModule.HsMax - humeModule.HsCurrent);
                }
            }
        }
        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public void OnFinished()
        {
            cooldown.Start(CooldownLength);
            CompManager.ActiveAbilities.Remove(this);
        }

        public override void CleanUp()
        {
            regenHumeTask.CleanUp();
        }
    }
}
