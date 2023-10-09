namespace ComAbilities
{
    using System.ComponentModel;
    using Exiled.API.Enums;
    using Exiled.API.Features.Attributes;
    // using Exiled.API.Enums;
    using Exiled.API.Interfaces;
    using global::ComAbilities.Types;
    using MapGeneration;
    using PlayerRoles;
    using UnityEngine;
    using UnityEngine.Windows;
    using KeycardPermissions = Exiled.API.Enums.KeycardPermissions;

    public sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
#if DEBUG
        public bool Debug { get; set; } = true;
#else
        public bool Debug { get; set; } = false;
#endif
        public string Localization { get; set; } = "English";

        public DistressSignalConfig DistressSignal { get; set; } = new();
        public RealityScramblerConfig RealityScrambler { get; set; } = new();
        public HologramConfig Hologram { get; set; } = new();
        public PlayerTrackerConfig PlayerTracker { get; set; } = new();
        public GoToScpConfig GoToScp { get; set; } = new();
        public BroadcastMessageConfig BroadcastMessage { get; set; } = new();
        public int AdditionalGenerators { get; set; } = 3;

        public BalanceConfigs BalanceConfigs { get; set; } = new();
        public GeneratorEffectsConfigs GeneratorEffectsConfigs { get; set; } = new();
        [Description("Whether or not to lock all doors open during the LCZ decontamination countdown, preventing 079 from locking doors")]
        public bool LCZCountdownLock { get; set; } = true;
        public bool DoComputerPerms { get; set; } = true;
        [Description("The level that 079 gets each keycard permission")]

        public Dictionary<KeycardPermissions, int> DoorPermissions { get; set; } = new()
        {
            { KeycardPermissions.ContainmentLevelOne, 1 },
            { KeycardPermissions.ContainmentLevelTwo, 2 },
            { KeycardPermissions.ContainmentLevelThree, 3 }, // unused
            { KeycardPermissions.ArmoryLevelOne, 2 },
            { KeycardPermissions.ArmoryLevelTwo, 3 },
            { KeycardPermissions.ArmoryLevelThree, 3 },
            { KeycardPermissions.Checkpoints, 1 },
            { KeycardPermissions.ExitGates, 3 },
            { KeycardPermissions.Intercom, 4 },
            { KeycardPermissions.AlphaWarhead, 6 },
        };

        //public CATranslations Translations { get/; set; } = new();
        
    }
    public sealed class GeneratorEffectsConfigs
    {
        public bool DoDoorExploding { get; set; } = true;
        public bool AllowKeycardDoors { get; set; } = false;
        public bool FilterAlreadyDestroyed { get; set; } = true;
       // public bool FilterCannotDestroy = true;
        public Dictionary<int, Range> DoorExplodeInterval { get; set; } = new()
        {
            { 1, new Range(40, 60) },
            { 2, new Range(25, 40) },
            { 3, new Range(15, 25) },
        };
        public List<DoorType> BlacklistedDoors { get; set; } = new()
        {
            DoorType.PrisonDoor
        };
    }
    public sealed class BalanceConfigs
    {
        public Dictionary<RoomName, float> BlackoutCooldowns { get; set; } = new()
        {
            { RoomName.Outside, 10 },
            { RoomName.Hcz106, 2 },
        };
        public Dictionary<int, float> RegenMultipliers { get; set; } = new()
        {
            { 1, 1.2f },
            { 2, 1.1f },
            { 3, 1f },
            { 4, 1f },
            { 5, 0.9f },
        };
    }
    public sealed class PlayerTrackerConfig : IAbilityConfig
    {
        [Description("Enable holograms, allowing for 079 to temporarily become a fake projection of a class.")]
        public bool Enabled { get; set; } = true;
        public int Level { get; set; } = 3;
        public int Slot2Level { get; set; } = 4;
        public float AuxCost { get; set; } = 50;
        public float Cooldown { get; set; } = 120;
        public float Length { get; set; } = 180f;
        public float AuxMultiplier { get; set; } = 0.75f;
        public AllHotkeys Hotkey { get; set; } = AllHotkeys.Throw;
        public bool TrackTutorials { get; set; } = true;
        public float GoToCost { get; set; } = 40f;
        public float GoToCooldown { get; set; } = 40f;
    }
    public sealed class HologramConfig : IAbilityConfig
    {
        [Description("Enable holograms, allowing for 079 to temporarily become a fake projection of a class.")]
        public bool Enabled { get; set; } = true;
        [Description("The level that 079 unlocks holograms")]
        public int Level { get; set; } = 4;
        [Description("The initial aux power cost of making a hologram")]
        public float AuxCost { get; set; } = 50;
        [Description("The cooldown for making a hologram, in seconds")]
        public float Cooldown { get; set; } = 120;
        [Description("How long the hologram lasts before it is finished")]
        public float Length { get; set; } = 20f;
        [Description("A list of all selectable roles, their unlocked level, and their aux cost")]
        public List<HologramRoleConfig> RoleLevels { get; } = new()
        {
            new HologramRoleConfig(RoleTypeId.ClassD, 4, 40)
        };
    }
    public sealed class RealityScramblerConfig : IAbilityConfig
    {
        [Description("Enable reality scrambling, allowing for 079 to regenerate the Hume Shield of other SCPs")]
        public bool Enabled { get; set; } = false;
        [Description("Default key for reality scrambling")]
        public AllHotkeys Hotkey { get; set; } = AllHotkeys.ADS;
        [Description("The level that 079 unlocks reality scrambling")]
        public int Level { get; set; } = 5;
        [Description("The initial aux power cost of reality scrambling")]
        public float AuxCost { get; set; } = 35f;
        [Description("The cooldown for reality scrambling, in seconds")]
        public float Cooldown { get; set; } = 10f;
        [Description("How long reality scrambling lasts before it is finished")]
        public float Length { get; set; } = 30f;
        [Description("How often to tick while reality scrambling (deducting aux and regenerating hume)")]
        public float TickTime { get; set; } = 2.5f;
        [Description("What percent of max Hume to regenerate every tick")]
        public float RegeneratePercentTick { get; set; } = 2.5f;
        [Description("How much aux power it costs every tick")]
        public float AuxCostTick { get; set; } = 8f;
        [Description("The multiplier to Aux Power while active")]
        public float AuxRegenMultiplier { get; set; } = 0;
        public string BroadcastText { get; set; } = "<size=20>Your <color=#8079ff>Hume Shield</color> is being regenerated by <color=#C50000>SCP-079</color>.</size>";
    }
    public sealed class DistressSignalConfig : IAbilityConfig
    {
        [Description("Enable distress signalling, allowing 079 to increase the chances for a certain wave to spawn")]
        public bool Enabled { get; set; } = false;
        [Description("The level that 079 unlocks signalling")]
        public int Level { get; set; } = 5;
        [Description("The aux power cost of signalling")]
        public float AuxCost { get; set; } = 200f;
        [Description("The cooldown for signalling, in seconds")]
        public float Cooldown { get; set; } = 240f; // 4 minutes
        [Description("The number of tickets that a successful distress signal will grant to the chosen team")]
        public float Tickets { get; set; } = 4;
    }
    public sealed class GoToScpConfig : IAbilityConfig
    {
        [Description("Enable distress signalling, allowing 079 to increase the chances for a certain wave to spawn")]
        public bool Enabled { get; set; } = true;
        [Description("The level that 079 unlocks signalling")]
        public int Level { get; set; } = 5;
        [Description("The aux power cost of signalling")]
        public float AuxCost { get; set; } = 200f;
        [Description("The cooldown for signalling, in seconds")]
        public float Cooldown { get; set; } = 240f; // 4 minutes
    }
    public sealed class RadioScannerConfig : IAbilityConfig
    {
        public bool Enabled { get; set; } = true;
        public int Level { get; set; } = 3;
        public float AuxCost { get; set; } = 0;
        public float Cooldown { get; set; } = 5f;
        public float AuxRegenMultiplier { get; set; } = 0;

    }
    public sealed class BroadcastMessageConfig : IAbilityConfig
    {
        [Description("Enable distress signalling, allowing 079 to increase the chances for a certain wave to spawn")]
        public bool Enabled { get; set; } = true;
        public int Level { get; set; } = 0; 
        public float AuxCost { get; set; } = 10;
        public float Cooldown { get; set; } = 10f;
        public int MaxLength { get; set; } = 200;
        public float MessageDuration { get; set; } = 10f;
        public int MaxPlayerNameLength { get; set; } = 10;
    }

    public record HologramRoleConfig(RoleTypeId Role, int Level, float Cost);

    public interface IAbilityConfig 
    {
        public bool Enabled { get; set; }
        public int Level { get; set; }
        public float AuxCost { get; set; }
        public float Cooldown { get; set; }
    }
}