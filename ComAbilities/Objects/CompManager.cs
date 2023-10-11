using ComAbilities.Abilities;
using ComAbilities.Types;

namespace ComAbilities.Objects
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using System.Data;
    using System.Globalization;
    using System.Text;
    using System.Diagnostics.CodeAnalysis;
    using global::ComAbilities.Types.RueTasks;
    using RueI;

    /// <summary>
    /// Manages all plugin-related things for a certain player
    /// </summary>
    public sealed class CompManager : IHotkeyHandler, IKillable
    {
        private static ComAbilities Instance => ComAbilities.Instance;

        private const int TimeToShowMessages = 5;
        private const int MESSAGE_RATE_LIMIT = 6;

        private readonly Dictionary<AllHotkeys, IHotkeyAbility> _hotkeysDict = new();
        private readonly Cooldown _errorCooldown = new();
        private readonly UpdateTask _messageTask;

        public Player AscPlayer { get; }
        public Scp079Role? Role => AscPlayer.Role as Scp079Role;

        public List<Ability> AbilityInstances { get; } = new();
        public DisplayManager Display { get; }
        
        public List<IReductionAbility> ActiveAbilities { get; } = new();

        public KeycardPermissions CachedKeycardPermissions { get; private set; } = new();

        // abilities    
        public RealityScrambler RealityScrambler { get; }
        public DistressSignal DistressSignal { get; }
        public Hologram Hologram { get; }
        public PlayerTracker PlayerTracker { get; }
        public GoTo GoTo { get; }
        public RadioScanner RadioScanner { get; }
        public BroadcastMsg BroadcastMessage { get; }

        // constructor
        public CompManager(Player player)
        {
            Log.Debug("Creating a new CompManager for " + player);
            this.Display = new(this);
            this.AscPlayer = player;

            _messageTask = new(TimeToShowMessages, () =>
            {
                Display.MessageElement.Set(string.Empty);
                Display.Update();
            });


            Log.Debug("Creating ability instances");
            this.RealityScrambler = RegisterAbility<RealityScrambler>();
            this.DistressSignal = RegisterAbility<DistressSignal>();
            this.Hologram = RegisterAbility<Hologram>();
            this.PlayerTracker = RegisterAbility<PlayerTracker>();
            this.GoTo = RegisterAbility<GoTo>();
            this.RadioScanner = RegisterAbility<RadioScanner>();
            this.BroadcastMessage = RegisterAbility<BroadcastMsg>();

           /* this.DisplayManager
                .CreateElement(Elements.AvailableAbilities, out Element? _, 10)
                .CreateElement(Elements.Message, out Element? _, lines: 1)
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
                .AddString("</color></voffset></size></align></line-height>"); */


            Log.Debug("All done");
        }

        private T RegisterAbility<T>() where T : Ability
        {
            T ability = (T)Activator.CreateInstance(typeof(T), this);
            if (!ability.Enabled) return ability;

            AbilityInstances.Add(ability);
            if (ability is IHotkeyAbility asHotkey)
            {

                _hotkeysDict.Add(asHotkey.HotkeyButton, asHotkey);
            }
            return ability;
        }

        public void HandleInput(AllHotkeys hotkey)
        {
            if (Role == null) return;
            if (Guards.SignalLost(Role)) return;

            if (Display.CurrentScreen == Screens.Tracker)
            {
                this.PlayerTracker.HandleInputs(hotkey);
                return;
            }

            if (this._hotkeysDict.TryGetValue(hotkey, out IHotkeyAbility ability))
            {
                if (ability is ICooldownAbility rateLimitedAbility)
                {
                    if (Guards.OnCooldown(rateLimitedAbility, out string errorCooldown))
                    {
                        ShowErrorHint(errorCooldown);
                        return;
                    }
                }

                if (Guards.NotEnoughAuxDisplay(Role, ability.AuxCost, out string response))
                {
                    ShowErrorHint(response);
                    return;
                }

                ability.Trigger();
            }
        }

        public void ShowErrorHint(string errorString)
        {
            if (!_errorCooldown.Active)
            {
                Display.MessageElement.Set(errorString);
                Display.Update();

                _errorCooldown.Start(TimeToShowMessages);
                _messageTask.Run();
            }
        }

        public void UpdatePermissions() {
            if (this.Role != null) CachedKeycardPermissions = CalculatePermissions(Role.Level);
        }

        public static KeycardPermissions CalculatePermissions(int curLevel)
        {
            Dictionary<KeycardPermissions, int> reqLevelsDict = Instance.Config.DoorPermissions;

            KeycardPermissions newPerms = new();
            IEnumerable<KeycardPermissions> permsList = Enum.GetValues(typeof(KeycardPermissions)).Cast<KeycardPermissions>();

            foreach (KeycardPermissions perm in permsList)
            {
                if (reqLevelsDict.TryGetValue(perm, out int level))
                {
                    if (level <= curLevel)
                    {
                        newPerms |= perm;
                    }
                }
                else
                {
                    newPerms |= perm;
                }
            }
            return newPerms;
        }   

        /// <summary>
        /// Generates an aux ETA for an <see cref="Scp079Role"/>.
        /// </summary>
        /// <param name="role">The role to generate the ETA for </param>
        /// <param name="cost">The aux cost</param>
        /// <returns></returns>
        public static float GetDisplayETA(Scp079Role role, float cost)
        {
            if (cost <= role.Energy) return 0.5f;
            float regenSpeed = role.EnergyRegenerationSpeed;
            return (float)Math.Max(0.5, (role.Energy - cost) / regenSpeed);
        }

        public void CleanUp()
        {
            Log.Debug("Cleaning up");
            foreach (Ability ability in AbilityInstances)
            {
                ability.CleanUp();
            }
            this.ActiveAbilities.Clear();
        }
        /*
/// <summary>
/// Gets a CompManager from aplayer
/// </summary>
/// <param name="player">The player to get the CompManager for</param>
/// <returns></returns>
public static CompManager Get(Player player) => CompManager.playerComputers[player];

/// <summary>
/// Attempts to get a CompManager from a player
/// </summary>
/// <param name="player">The player to get the CompManager for</param>
/// <param name="compManager">The returned CompManager</param>
/// <returns>A bool indicating whether or not the player exists in the dictionary</returns>
public static bool TryGet(Player player, out CompManager compManager) => CompManager.playerComputers.TryGetValue(player, out compManager);

/// <summary>
/// Removes a CompManager from a player
/// </summary>
/// <param name="player">The player to remove the CompManager from</param>
public static void Remove(Player player)
{
   CompManager.playerComputers[player].KillAll();
   CompManager.playerComputers.Remove(player);
}

/// <summary>
/// Creates a new CompManager and adds it to the dictionary
/// </summary>
/// <param name="player"></param>
public static void Add(Player player) => CompManager.playerComputers.Add(player, new CompManager(player));

/// <summary>
/// Attempts to remove a CompManager from a player
/// </summary>
/// <param name="player">The player to remove the CompManager from</param>
public static void TryRemove(Player player)
{
   if (CompManager.playerComputers.ContainsKey(player)) Remove(player);
} */
    }
}
    