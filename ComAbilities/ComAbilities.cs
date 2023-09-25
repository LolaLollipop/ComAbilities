namespace ComAbilities
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.ComAbilitiesEvents;
    using global::ComAbilities.Localizations;
    using global::ComAbilities.Objects;
    using global::Localizer;

    using HarmonyLib;

#pragma warning disable CS8618
#pragma warning disable CS8625
    public sealed class ComAbilities : Plugin<Config>
    {
        private ComAbilities() { }

        private PlayerHandler playerHandler;
        private ServerHandler serverHandler;
        private Scp079Handler scp079Handler;

        public override string Author { get; } = "Rue";
        public override string Name { get; } = "ComAbilities";
        public override string Prefix { get; } = "ComAbilities";
        public override Version Version { get; } = new(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new(8, 0, 0);
        public override PluginPriority Priority { get; } = PluginPriority.Last;

        private string LocalizationsURL { get; } = "https://github.com/Ruemena/ComAbilities/raw/main/Localizations.zip";
        private string LocalizationPath => $"{Paths.Configs}/{Server.Port}-ComAbilities_Localizations";
        private Version LocalizationVersion { get; } = new(1, 0, 0);


        private static readonly ComAbilities Singleton = new();
        public static ComAbilities Instance => Singleton;


        public CompDict CompDict { get; } = new();

        private Updater Updater;
        private Localizer<CALocalization> Localizer;
        public CALocalization Localization => Localizer.CurrentLocalization;



        private Harmony _harmony;
        private string _harmonyId { get; } = $"079Abilities_{DateTime.Now.Ticks}:3";

        public override void OnEnabled()
        {
            _harmony = new Harmony(_harmonyId);
            _harmony.PatchAll();


            HttpClient client = new();
            Localizer = new(client);
            Localizer.Start(LocalizationsURL, LocalizationPath, Config.Localization, LocalizationVersion);

            Updater = new(Version, "ComAbilities.dll", "https://api.github.com/repos/Ruemena/ComAbilities/releases/latest", client);
            Updater.Start(this);


            playerHandler = new PlayerHandler();
            serverHandler = new ServerHandler();
            scp079Handler = new Scp079Handler();

            Exiled.Events.Handlers.Server.RoundEnded += this.serverHandler.OnRoundEnded;

            Exiled.Events.Handlers.Player.InteractingDoor += this.playerHandler.OnInteractingDoor;
            Exiled.Events.Handlers.Player.ChangingRole += playerHandler.OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem += playerHandler.OnChangingItem;
            Exiled.Events.Handlers.Player.Left += playerHandler.OnLeft;
            Exiled.Events.Handlers.Player.Spawning += playerHandler.OnSpawning;

            Exiled.Events.Handlers.Player.DroppingItem += playerHandler.OnDroppingItem;
            Exiled.Events.Handlers.Player.TogglingWeaponFlashlight += playerHandler.OnTogglingWeaponFlashlight;
            Exiled.Events.Handlers.Player.UnloadingWeapon += playerHandler.OnUnloadingWeapon;
            Exiled.Events.Handlers.Player.ReloadingWeapon += playerHandler.OnReloadingWeapon;

            Exiled.Events.Handlers.Player.ActivatingWarheadPanel += this.playerHandler.DenyHologram;
            Exiled.Events.Handlers.Player.StoppingGenerator += playerHandler.DenyHologram;
            Exiled.Events.Handlers.Player.ClosingGenerator += playerHandler.DenyHologram;
            Exiled.Events.Handlers.Scp106.Stalking += playerHandler.DenyHologram;
            Exiled.Events.Handlers.Scp106.Teleporting += playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.ChangingLeverStatus += playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.ChangingLeverStatus += playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.Stopping += playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.Starting += playerHandler.DenyHologram;

            Exiled.Events.Handlers.Scp079.Pinging += this.scp079Handler.OnPinging;
            Exiled.Events.Handlers.Scp079.GainingLevel += scp079Handler.OnGainingLevel;

            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            _harmony.UnpatchAll(_harmonyId);


            Exiled.Events.Handlers.Server.RoundEnded -= serverHandler.OnRoundEnded;

            Exiled.Events.Handlers.Player.InteractingDoor -= playerHandler.OnInteractingDoor;
            Exiled.Events.Handlers.Player.ChangingRole -= playerHandler.OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem -= playerHandler.OnChangingItem;
            Exiled.Events.Handlers.Player.DroppingItem += playerHandler.OnDroppingItem;
            Exiled.Events.Handlers.Player.Left -= playerHandler.OnLeft;
            Exiled.Events.Handlers.Player.Spawning -= playerHandler.OnSpawning;

            Exiled.Events.Handlers.Player.ActivatingWarheadPanel -= playerHandler.DenyHologram;
            Exiled.Events.Handlers.Player.StoppingGenerator -= playerHandler.DenyHologram;
            Exiled.Events.Handlers.Player.ClosingGenerator -= playerHandler.DenyHologram;
            Exiled.Events.Handlers.Scp106.Stalking -= playerHandler.DenyHologram;   
            Exiled.Events.Handlers.Scp106.Teleporting -= playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.ChangingLeverStatus -= playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.ChangingLeverStatus -= playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.Stopping -= playerHandler.DenyHologram;
            Exiled.Events.Handlers.Warhead.Starting -= playerHandler.DenyHologram;

          //  Exiled.Events.Handlers.Player.Hurting -= playerHandler.OnHurting;

            Exiled.Events.Handlers.Scp079.Pinging -= scp079Handler.OnPinging;
            Exiled.Events.Handlers.Scp079.GainingLevel -= scp079Handler.OnGainingLevel;

            playerHandler = null;
            serverHandler = null;
            scp079Handler = null;

            base.OnDisabled();
        }
    }
}
#pragma warning restore