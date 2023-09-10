using ComAbilities.Types;
using PlayerRoles;
using System.ComponentModel;

namespace ComAbilities.Localizations
{
    public sealed class CALocalization
    {
     //   public Version LastVersion { get; set; } = new(1, 0, 0);
        public SharedT Shared { get; set; } = new();
        public ErrorsT Errors { get; set; } = new();
        public DistressSignalT DistressSignal { get; set; } = new();
        public RealityScramblerT RealityScrambler { get; set; } = new();
        public HologramT Hologram { get; set; } = new();
        public TrackerT Tracker { get; set; } = new();
        public GoToT GoTo { get; set; } = new();
        public RadioScannerT RadioScanner { get; set; } = new(); 
        public BroadcastMessageT BroadcastMessage { get; set; } = new();
    }

    public sealed class SharedT
    {
        [Description("{0}: command description")]
        public string CommandFormat { get; set; } = "079 command: {0}";

        public string LevelUpUnlockedAbilities { get; set; } = "<color=#dedcdc>You have unlocked <color=#a83432>new abilities</color><color=#dedcdc>.</color>";
        [Description("{0}: ability name, {1} ability description")]
        public string UnlockedAbilityFormat { get; set; } = "<color=#d96e6c>{0}</color><color=#dedcdc>: {1}";
        [Description("{0}: aux regen rate")]
        public string RegenSpeedFormat { get; set; } = "TOTAL AUX REGENERATION RATE {0}%";
        public string NoAuxRegen { get; set; } = "AUX REGENERATION SUSPENDED";

        public Dictionary<AllHotkeys, string> Hotkeys { get; set; } = new()
        {
            { AllHotkeys.GunFlashlight, "Flashlight" },
            { AllHotkeys.ADS, "ADS" },
            { AllHotkeys.Reload, "Reload" },
            { AllHotkeys.HoldReload, "Hold Reload" },
            { AllHotkeys.Throw, "Throw" },
            { AllHotkeys.Noclip, "Noclip" },
        }; 

        public Dictionary<RoleTypeId, string> RoleNames { get; set; } = new()
        {
            { RoleTypeId.ChaosRepressor, "CI Repressor" },
            { RoleTypeId.ChaosMarauder,  "CI Marauder" },
            { RoleTypeId.ChaosConscript, "CI Conscript" },
            { RoleTypeId.ChaosRifleman, "CI Rifleman" },
            { RoleTypeId.ClassD, "Class-D" },
            { RoleTypeId.NtfPrivate, "MTF Private" },
            { RoleTypeId.NtfSergeant, "MTF Sergeant" },
            { RoleTypeId.NtfSpecialist, "MTF Specialist" },
            { RoleTypeId.NtfCaptain, "MTF Captain" },
            { RoleTypeId.Scientist, "Scientist" },
            { RoleTypeId.FacilityGuard, "Facility Guard" },
            { RoleTypeId.Tutorial, "Tutorial" },
            { RoleTypeId.CustomRole, "Other" },
        };
    }

    public sealed class ErrorsT 
    {
        public string DisplayAccessDenied { get; set; } = "- ACCESS DENIED - ";
        [Description("{0}: time left on cooldown (in seconds)")]
        public string DisplayOnCooldown { get; set; } = "- ON COOLDOWN. ETA: {0} SECONDS - ";
        [Description("{0}: time until enough aux (in seconds)")]
        public string DisplayNotEnoughAux { get; set; } = "- NOT ENOUGH AUX. ETA: {0} SECONDS - ";
        public string NotEnabled { get; set; } = "ERROR: Not enabled";
        public string WrongRole { get; set; } = "ERROR: You must be SCP-079 to use this ability.";
        public string SignalLost { get; set; } = "- SIGNAL LOST -";

        [Description("{0}: time until enough aux (in seconds)")]
        public string NotEnoughAux { get; set; } = "ERROR: You do not have enough Aux Power to do this. ETA: {0} seconds";

        [Description("{0}: required level")]
        public string InsufficientLevel { get; set; } = "ERROR: Your level is not high enough for this. Required level: Lv.{0}";

        [Description("{0}: time left on cooldown (in seconds)")]
        public string OnCooldown { get; set; } = "ERROR: This ability is on cooldown for {0} more seconds";
    }

    public sealed class DistressSignalT : IAbilityLocale
    {
        public string Name { get; set; } = "Distress Signal";
        public string Description { get; set; } = "Sends a distress signal, increasing the chances for a certain wave to spawn.";
        [Description("{0}: aux cost")]
        public string DisplayText { get; set; } = "[.ds] DISTRESS SIGNAL ({0} AUX)";
        public string CommandDescription { get; set; } = "079 command: Broadcasts a distress signal, increasing the chances for a certain wave to spawn.";
        public string InvalidOption { get; set; } = "Invalid team provided. Your options are either Nine-Tailed Fox (mtf) or Chaos Insurgency (ci).";

        [Description("{0}: cooldown length")]
        public string Success { get; set; } = "Successfully sent distress signal. Another distress signal can be broadcasted in {0} seconds.";
    }
    public sealed class RealityScramblerT : IAbilityLocale
    {
        public string Name { get; set; } = "Reality Scrambler";
        public string Description { get; set; } = "Disables the on-site SRAs, regenerating the Hume Shield of all SCPs.";
        [Description("{0}: hotkey, {1} aux cost")]
        public string DisplayText { get; set; } = "[{0} KEY] REALITY SCRAMBLER ({1} AUX)";
        public string ActiveText { get; set; } = "REALITY SCRAMBLER ACTIVE";
    }
    public sealed class HologramT : IAbilityLocale
    {
        public string Name { get; set; } = "Hologram";
        public string Description { get; set; } = "Projects a limited hologram, tricking other players.";

        [Description("{0}: lowest hologram aux cost, {1} highest hologram aux cost")]
        public string DisplayText { get; set; } = "[.hg] HOLOGRAM ({0}-{1} AUX)";

        [Description("{0}: name of role colored, {1} time left (in seconds), {2} cancel message")]
        public string ActiveText { get; set; } = "<size=30>You currently appear to others as a {0}.<br>Time left: {1} seconds<br>{2}</size>";
        public string CancelMessageBefore { get; set; } = "Press X twice to cancel.";
        public string CancelMessageAfter { get; set; } = "Press X again to cancel.";
        public string AvailableHologramRoles { get; set; } = "List of available roles:";

        [Description("{0}: index, {1} required level, {2} role name, {3} aux cost")]
        public string HologramRoleFormat { get; set; } = "({0}) Lv.{1} | {2} [{3} Aux]";
        [Description("{0}: cooldown (in seconds)")]
        public string Success { get; set; } = "You are projecting a hologram. This can be done again in {0} seconds.";
    }

    public sealed class TrackerT : IAbilityLocale
    {
        public string Name { get; set; } = "Player Tracker";
        public string Description { get; set; } = "Allows for players to be tagged and then teleported to later.";
        [Description("{0}: hotkey, {1} aux cost")]
        public string DisplayText { get; set; } = "[{0} KEY] PLAYER TRACKER ({1} AUX)";
        [Description("{0}: number of active trackers")]
        public string ActiveDisplayText { get; set; } = "{0} TRACKERS ACTIVE";

        [Description("{0}: index, {1} role color, {2} role name, {3} hotkey (uppercase)")]
        public string TrackerFormat { get; set; } = "[ ({0}) <color={1}>{2}</color> : {3} KEY]";

        [Description("{0}: selected tracker")]
        public string SelectedTrackerFormat { get; set; } = "> <b>{0}</b> <";

        public string SelectedEmpty { get; set; } = "PING PLAYER TO ASSIGN THEM TO THIS SLOT";
        public string SelectedFull { get; set; } = "HOLD [ RELOAD KEY ] TO DELETE";
        [Description("{0}: hotkey")]
        public string CloseMessage { get; set; } = "PRESS [ {0} KEY ] TO CLOSE MENU";
    }

    public sealed class GoToT : IAbilityLocale
    {
        public string Name { get; set; } = "Go To";
        public string Description { get; set; } = "Move your camera to an SCP or tracked player.";

        [Description("{0}: aux cost of going to a tracked player, {1}: aux cost of going to an SCP")]
        public string DisplayText { get; set; } = "[.gt] GO TO ({0}/{1} AUX)";
        public string InvalidSearch { get; set; } = "Invalid search provided.";
        public string NoSCPFound { get; set; } = "No SCP of that type found.";
        public string Success { get; set; } = "Teleporting you...";
    }
    public sealed class RadioScannerT : IAbilityLocale
    {
        public string Name { get; set; } = "Radio Scanner";
        public string Description { get; set; } = "Eavesdrop on messages sent through the radio.";

        [Description("{0}: hotkey, {1} aux cost")]
        public string DisplayText { get; set; } = "[{0} KEY] RADIO SCANNER ({1} AUX)";
        public string ActiveText { get; set; } = "REALITY SCRAMBLER ACTIVE";
    }
    public sealed class BroadcastMessageT : IAbilityLocale
    {
        public string Name { get; set; } = "Broadcast Message";
        public string Description { get; set; } = "Broadcast a message to all SCPs.";

        [Description("{0}: aux cost")]
        public string DisplayText { get; set; } = "[.bc] BROADCAST ({0} AUX)";
        [Description("{0}: player name, {1} message")]
        public string BroadcastFormat { get; set; } = "<size=55%><color=#c21c19>SCP-079 ({0}): <color=#c2c2c2>{message}</color></size>";
        public string Success { get; set; } = "Successfully sent broadcast! Another broadcast can be sent in {0} seconds";
        public string MessageTooLong { get; set; } = "Cannot send broadcast: the broadcast is too long.";
        public string NoMessageProvided { get; set; } = "No message provided.";
    }

    public interface IAbilityLocale
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DisplayText { get; set; }
    }
}   