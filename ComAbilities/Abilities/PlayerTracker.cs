using ComAbilities.Localizations;
using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.ComAbilitiesEvents;
using PlayerRoles.PlayableScps.Scp079.Rewards;
using PlayerStatsSystem;
using System.Text;

namespace ComAbilities.Abilities
{
    //[Hotkey]
    public sealed class PlayerTracker : Ability, IHotkeyAbility, IReductionAbility, ICooldownAbility
    {
        private readonly static TrackerT TrackerT = Instance.Localization.Tracker;

        private static PlayerTrackerConfig _config => Instance.Config.PlayerTracker;

        private readonly Cooldown _cooldown = new();
        //public bool InterfaceActive => CompManager.DisplayManager.SelectedScreen == DisplayTypes.Tracker;

        public PlayerTracker(CompManager compManager) : base(compManager) {
            Trackers = new() {
                new ActiveTracker(_config.Length, UpdateUI, ReqLevel),
                new ActiveTracker(_config.Length, UpdateUI, _config.Slot2Level)
            };
        }
        public override string Name { get; } = TrackerT.Name;
        public override string Description { get; } = TrackerT.Description;
       // public string UsageGuide { get; } = "Once you open the Tracker menu, you can select a tracker slot. Afterwards, ping a person to begin tracking them in that slot. Once you start tracking a person, you can run .goto [slot] to instantly move your camera to the person. However, for every active tracker, your regeneration rate will decrease.";
       // public string Lore { get; } = "As part of an effort to combat the increasing number of breaches by SCP-106, SCP-173, and SCP-████, a network of sensors, light detectors, and other devices was installed within the facility to act as a support system to the Breach Scanner. This system allows for real-time monitoring and tracking of anomalies, although it has been utilized against hostile GOI forces and rogue personnel.";
        public override float AuxCost { get; } = _config.AuxCost;
        public override int ReqLevel { get; } = _config.Level;
        public override string DisplayText => string.Format(TrackerT.DisplayText, Instance.Localization.Shared.Hotkeys[HotkeyButton].ToUpper(), AuxCost);
        public string ActiveDisplayText => string.Format(TrackerT.ActiveDisplayText, Trackers.Count(x => x.Enabled));
        public override bool Enabled => _config.Enabled;

        public float AuxModifier => (float)Math.Pow(_config.AuxMultiplier,
            Math.Min(1, Trackers.Count(x => x.Enabled)));

        public AllHotkeys HotkeyButton { get; } = _config.Hotkey;

        public float CooldownLength { get; } = _config.Cooldown;
        public bool OnCooldown => _cooldown.Active;

        public bool IsActive => Trackers.Any(x => x.Player != null && x.Enabled);

        public TrackerManager Trackers { get; }

        //public bool IsGettingPlayer => _expireTrackerTask.Enabled;
        // --------------------

        public void Trigger()
        {
            if (base.Display.CurrentScreen == Screens.Tracker)
            {
                Display.CurrentScreen = Screens.Tracker;
            }
            else
            {
                Display.CurrentScreen = Screens.Tracker;
            }

            Display.Update();
        }

        public void AssignToSelectedTracker(Player player)
        {
            Trackers.StartSelected(player);

            _cooldown.Start(CooldownLength);
            CompManager.DeductAux(AuxCost);

            UpdateUI();
        }

        public void UpdateUI()
        {
            StringBuilder sb = new();
            sb.Append(Trackers.ConvertToHintString());

            if (Trackers.GetState(Trackers.SelectedTracker) == TrackerState.Selected) 
                sb.Append("\n" + TrackerT.SelectedEmpty);

            if (Trackers.GetState(Trackers.SelectedTracker) == TrackerState.SelectedFull) 
                sb.Append("\n" + TrackerT.SelectedFull);

            sb.Append("\n" + TrackerT.CloseMessage);

            Display.TrackerElement.Set(sb.ToString());
            Display.Update(Screens.Tracker);
        }

        public float GetDisplayETA() => _cooldown.GetDisplayETA();

        public Player? GetTrackerPlayer(int trackerId) => Trackers[trackerId].Player;

        public void HandleInputs(AllHotkeys hotkey)
        {
            switch (hotkey)
            {
                case AllHotkeys.HoldReload:
                    if (Trackers.SelectedTracker != -1 && Trackers[Trackers.SelectedTracker].Enabled)
                    {
                        Trackers[Trackers.SelectedTracker].CleanUp();
                    }
                    break;
                        
                case AllHotkeys.Reload:
                    if (Trackers.SelectedTracker == Trackers.Count - 1)
                    {
                        Trackers.SelectedTracker = 0;
                    } else
                    {
                        Trackers.SelectedTracker++;
                    }
                    UpdateUI();
                    break;
                case AllHotkeys.Throw:
                    CompManager.Display.CurrentScreen = Screens.Tracker;
                    CompManager.Display.Update();
                    break;
            }
        }

        public override void CleanUp()
        {
            Trackers.CleanUp();
        }
        /*public void HandleInputs(AllHotkeys? hotkey)
        {
            if (!hotkey.HasValue) return;
            switch (hotkey.Value)
            {
                case AllHotkeys.Grenade:

                    if (_trackers.SelectedTracker != default && _trackers[_trackers.SelectedTracker].Enabled)
                    {
                        _trackers[_trackers.SelectedTracker].ForceEnd();
                    }
                    break;
                case AllHotkeys.Medical:
                    CompManager.DisplayManager.SelectedScreen = DisplayTypes.Main;
                    CompManager.DisplayManager.Update();
                    break;
                default:
                    if (TrackerHotkeys.TryGetValue(hotkey.Value, out ActiveTracker tracker)) {
                        _trackers.SelectedTracker = _trackers.IndexOf(tracker);
                    }
                    break;
            }
        } */
    }
}
