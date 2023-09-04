using ComAbilities.Abilities;
using ComAbilities.Types;

namespace ComAbilities.Objects
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using System.Text;

    /// <summary>
    /// Manages all plugin-related things for a certain player
    /// </summary>
    public sealed class CompManager
    {
        private readonly ComAbilities Instance = ComAbilities.Instance;
        public Player AscPlayer { get; private set; }
        public Scp079Role? Role => AscPlayer.Role as Scp079Role;

        public List<Ability> AbilityInstances { get; private set; } = new();
        public DisplayManager<DisplayTypes, Elements> DisplayManager { get; private set; }
        private HotkeyModule HotkeyModule { get; set; } = new();
            
        public List<IReductionAbility> ActiveAbilities { get; private set; } = new();

        private float RateLimit { get; } = 4;
        private Cooldown _errorCooldown { get; } = new(); // Global error ratelimit
        private UpdateTask _messageTask { get; }

        // abilities    
        public RealityScrambler RealityScrambler { get; }
        public DistressSignal DistressSignal { get; }
        public Hologram Hologram { get; }
        public PlayerTracker PlayerTracker { get; }
        public GoTo GoTo { get; }
        public RadioScanner RadioScanner { get; }
        public BroadcastMsg BroadcastMessage { get; }


        private int _timeToShowMessages { get; } = 5;

        // constructor
        public CompManager(Player player)
        {
            Log.Debug("Creating a new CompManager for " + player);
            this.DisplayManager = new(player);
            this.AscPlayer = player;

            _messageTask = new(_timeToShowMessages, () =>
            {
                DisplayManager.SetElement(Elements.Message, " ");
            });


            Log.Debug("Creating ability instances");
            this.RealityScrambler = RegisterAbility<RealityScrambler>();
            this.DistressSignal = RegisterAbility<DistressSignal>();
            this.Hologram = RegisterAbility<Hologram>();
            this.PlayerTracker = RegisterAbility<PlayerTracker>();
            this.GoTo = RegisterAbility<GoTo>();
            this.RadioScanner = RegisterAbility<RadioScanner>();
            this.BroadcastMessage = RegisterAbility<BroadcastMsg>();

            this.DisplayManager
                .CreateElement(Elements.AvailableAbilities, out Element? _, 10)
                .CreateElement(Elements.Message, out Element? _, 1)
                .CreateElement(Elements.Trackers, out Element? _, 6)
                .CreateElement(Elements.ActiveAbilities, out Element? _, 7);
                

            Log.Debug("Registering UI");
            this.DisplayManager.AddScreen(DisplayTypes.Main)
                .AddString("<align=center><voffset=-3em><b><color=#801919><size=50%>")
                .AddElement(Elements.Message, 1)
                .AddString("</size></color></b></voffset></align>")
                .AddString("<align=right><size=50%><line-height=120%><voffset=-5em><color=#adadad>")
                .AddElement(Elements.AvailableAbilities, 12)
                .AddString("</color></voffset></size></align></line-height>");

            this.DisplayManager.AddScreen(DisplayTypes.Tracker)
                .AddString("\n<align=center><voffset=-5em><b><color=#801919><size=50%>")
                .AddElement(Elements.Message, 1)
                .AddString("</size></color></b></voffset></align>")
                .AddString("<size=50%><align=center><line-height=20%><voffset=-7em>PLAYER TRACKER<br>SELECT A SLOT TO BEGIN TRACKING")
                .AddElement(Elements.Trackers, 6)
                .AddString("</color></voffset></size></align></line-height>");


            Log.Debug("All done");
        }

        private T RegisterAbility<T>() where T : Ability
        {
            T ability = (T)Activator.CreateInstance(typeof(T), this);
            if (!ability.Enabled) return ability;

            AbilityInstances.Add(ability);
            if (ability is IHotkeyAbility asHotkey)
            {
                
                HotkeyModule.Register(asHotkey);
            }
            return ability;
        }
        public void AddIfEnabled(List<Ability> list, Ability ability)
        {
            if (ability.Enabled)
            {
                list.Add(ability);  
            }
        }
        public void AddIfEnabled<T>(Dictionary<T, Ability> dict, T key, Ability ability)
        {
            if (ability.Enabled)
            {
                dict.Add(key, ability);
            }
        }

        public void HandleInput(NewHotkeys hotkey)
        {
            if (Role == null) return;

            if (Guards.SignalLost(Role)) { return; }

        }
        public void KillAll()
        {
            Log.Debug("Cleaning up");
            foreach (Ability ability in AbilityInstances)
            {
                ability.KillTasks();
            }
            this.ActiveAbilities.Clear();

            //this.DisplayManager.CleanUp();
        }


        public void DeductAux(float cost)
        {
            if (this.Role == null) throw new Exception("No role");
            this.Role.Energy -= cost;
        }
        public void TryShowErrorHint(string errorString)
        {
            if (!_errorCooldown.Active)
            {
                DisplayManager.SetElement(Elements.Message, errorString);
                DisplayManager.Update();

                _errorCooldown.Start(RateLimit);
                _messageTask.Run();
            }
        }

        public void QueueAvailableAbilityHints(int currentLevel)
        {
            StringBuilder stringBuilder = new("<align=right><size=50%><line-height=120%><voffset=10em><color=#adadad>AVAILABLE ABILITIES<br>PRESS TAB TO USE<br>HOTKEY ABILITIES OR<br>~ TO USE CONSOLE ABILITIES<br>");
            Log.Debug("Testing");
            foreach (Ability ability in AbilityInstances)
            {
                if (ability.ValidateLevel(currentLevel))
                {
                    stringBuilder.Append(ability.DisplayText + "<br>");
                }
            }
            stringBuilder.Append("</color></voffset></size></align></line-height>");
            DisplayManager.SetElement(Elements.AvailableAbilities, stringBuilder.ToString());
        }   
        public IEnumerable<Ability> GetNewAbilities(int currentLevel)
        {
            return this.AbilityInstances.Where(x => x.ReqLevel == currentLevel);
        }

        public void QueueActiveAbilityHints()
        {
            if (!this.ActiveAbilities.Any())
            {
                DisplayManager.SetElement(Elements.ActiveAbilities, "");
            }

            double regenSpeed = this.Role!.AuxManager.RegenSpeed; // make percent and round to 3 decimal places  (e.g. 0.437108 becomes 43.711%)
            StringBuilder sb = new();
            sb.Append("<color=#ad251c>");

            if (regenSpeed == 0)
            {
                sb.Append(Instance.Localization.Shared.NoAuxRegen);
            } else
            {
                float percent = (float)Math.Round((regenSpeed / Role.AuxManager._regenerationPerTier[Role.Level]) * 100, 3);
                sb.Append(string.Format(Instance.Localization.Shared.RegenSpeedFormat, percent));
            }
            sb.Append("</color>");

            DisplayManager.SetElement(Elements.ActiveAbilities, sb.ToString());
            DisplayManager.Update(DisplayTypes.Main);
        }

        public static float GetETA(Scp079Role role, float cost)
        {
            if (role == null) throw new Exception("No role");
            if (cost <= role.Energy) return 0.5f;
            float regenSpeed = role.EnergyRegenerationSpeed;
            return (float)Math.Max(0.5, (role.Energy - cost) / regenSpeed);
        }

    }

}
    