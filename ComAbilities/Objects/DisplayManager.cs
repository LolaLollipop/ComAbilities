using ComAbilities.Abilities;
using ComAbilities.Localizations;
using ComAbilities.Types;
using ComAbilities.Types.RueTasks;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using RueI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Utf8Json.Resolvers.Internal;


/// Handles 
namespace ComAbilities.Objects
{
    [Flags]
    public enum Screens
    {
        Main = 1,
        Tracker = 2,
    }

    public class DisplayManager
    {
        private CompManager compManager;

        private static ComAbilities Instance => ComAbilities.Instance;
        private static CALocalization Localization => Instance.Localization;

        public ScreenPlayerDisplay<Screens> PlayerDisplay { get; }

        public ScreenSetElement<Screens> TrackerElement { get; }
        public ScreenSetElement<Screens> ActiveAbilitiesElement { get; }
        public ScreenSetElement<Screens> AvailableAbilitiesElement { get; }
        public SetElement MessageElement { get; } = new(400, zIndex: 20);

        public DisplayManager(CompManager manager)
        {
            compManager = manager;
            PlayerDisplay = new(manager.AscPlayer, Screens.Main);

            TrackerElement = new(Screens.Tracker, 0, zIndex: 10, Get_TrackerMenu());
            ActiveAbilitiesElement = new(Screens.Main, 600, zIndex: 5, Get_ActiveAbilities());
            AvailableAbilitiesElement = new(Screens.Main, 300, zIndex: 4, Get_AvailableAbilities());

            PlayerDisplay.Add(MessageElement, TrackerElement, ActiveAbilitiesElement, AvailableAbilitiesElement);
        }

        public Screens CurrentScreen { get => PlayerDisplay.CurrentScreen; set => PlayerDisplay.CurrentScreen = value; }

        public void Update() => PlayerDisplay.Update();
        public void Update(Screens screen) => PlayerDisplay.Update(screen);
        /// TODO: format this to use proper text thing
        /*
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
                .AddString("<size=50%><align=center><line-height=50%><voffset=-7em>PLAYER TRACKER<br>SELECT A SLOT TO BEGIN TRACKING")
                .AddElement(Elements.Trackers, 6)
                .AddString("</color></voffset></size></align></line-height>");*/
        public string Get_ActiveAbilities()
        {
            if (!compManager.ActiveAbilities.Any() || compManager.Role == null) return string.Empty;

            float regenSpeed = compManager.Role.AuxManager.RegenSpeed;
            StringBuilder sb = new();

            sb.Append("<color=#ad251c>");

            if (regenSpeed == 0)
            {
                sb.Append(Instance.Localization.Shared.NoAuxRegen);
            }
            else
            {
                float percent = (float)Math.Round(regenSpeed / compManager.Role.AuxManager._regenerationPerTier[compManager.Role.Level] * 100, 3);
                sb.AppendFormat(Instance.Localization.Shared.RegenSpeedFormat, percent);
            }

            return sb.ToString();

            //DisplayManager.SetElement(Elements.ActiveAbilities, sb.ToString());
            //DisplayManager.Update(DisplayTypes.Main);
        }

        public string Get_AvailableAbilities()
        {
            if (compManager.Role == null) return string.Empty;

            StringBuilder sb = new("<align=right><size=50%><line-height=120%><color=#adadad>");
            sb.Append(Instance.Localization.Shared.AvailableAbilities);
            sb.Append("<br>");

            foreach (Ability ability in compManager.AbilityInstances)
            {
                if (compManager.Role.Level >= ability.ReqLevel)
                {
                    sb.AppendLine(ability.DisplayText);
                }
            }
            return sb.ToString();
        }

        public string Get_TrackerMenu()
        {
            TrackerManager trackers = compManager.PlayerTracker.Trackers;
            
            StringBuilder sb = new();
            sb.Append(trackers.ConvertToHintString());

            TrackerState currentSelected = trackers.GetState(trackers.SelectedTracker);

            if (currentSelected == TrackerState.Selected)
                sb.Append("\n" + Localization.Tracker.SelectedEmpty);

            if (currentSelected == TrackerState.SelectedFull)
                sb.Append("\n" + Localization.Tracker.SelectedFull);

            sb.Append("\n" + Localization.Tracker.CloseMessage);

            return sb.ToString();
        }

        // TODO: do this thing lol
        public string AddMessage(string message)
        {
            return string.Empty;
        }
    }
}
