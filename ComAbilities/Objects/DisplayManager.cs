using ComAbilities.Abilities;
using ComAbilities.Localizations;
using ComAbilities.Types;
using CommandSystem;
using Exiled.API.Features;
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
    /// <summary>
    /// Used to display multiple things at a time
    /// </summary>
    public class Element
    {
        private string? _content;
        public int AllocatedLines { get; private set; }
        public Element(int lines)
        {
            AllocatedLines = lines;
        }
        public Element Set(string content)
        {
            _content = content;
            return this; // method chaining
        }
        public Element AllocateLines(int lines)
        {
            AllocatedLines = lines;
            return this; // method chaining
        }
        public string Get()
        {
            return _content ?? " ";
        }
    }
    internal class ElementOrString<TElementName>
    {
        public string? String { get; set; }
        public TElementName? ElementName { get; set; }
        public int? AllocatedLines { get; set; }
        public ElementOrString(string content)
        {
            String = content;
        }
        public ElementOrString(TElementName name, int allocatedLines)
        {
            ElementName = name;
            AllocatedLines = allocatedLines;
        }
    }
    public class Screen<TElementName>
    {
        public Screen() { }
       // private Dictionary<TElementName, Element> _dynamicElements = new();
        private List<ElementOrString<TElementName>> _display = new();
        private static string _placeholder => " ";

        public Screen<TElementName> AddString(string add)
        {
            ElementOrString<TElementName> element = new ElementOrString<TElementName>(add);
            _display.Add(element);
            return this; // method chaining
        }
        public Screen<TElementName> AddElement(TElementName elementName, int allocatedLines = 0)
        {
            ElementOrString<TElementName> element = new(elementName, allocatedLines);
            element.AllocatedLines = allocatedLines;
            _display.Add(element);
            return this; // method chaining
        }
        public string ConvertToString(Dictionary<TElementName, Element> elements)
        {
            StringBuilder sb = new();
            foreach (var elementOrString in _display)
            {
                if (elementOrString.String != null)
                {
                    sb.Append(elementOrString.String);
                } else if (elementOrString.ElementName != null)
                {
                    Element currentElement = elements[elementOrString.ElementName];
                    sb.Append(currentElement.Get());

                    int newLines = (elementOrString.AllocatedLines ?? 0) - Regex.Matches(currentElement.Get(), @"(\n)|<br>").Count;
                    for (int i = 0; i < Math.Min(newLines, 0); i++)
                    {
                        sb.Append("\n ");
                    }

                } else throw new Exception("Invalid ElementOrString provided - this is a developer error");
            }
            return sb.ToString();
        }
    }
    public class DisplayManager<TScreenName, TElementName>
    {
        public TScreenName? SelectedScreen;

        private Dictionary<TScreenName, Screen<TElementName>> _screens = new();
        private Dictionary<TElementName, Element> _elements = new();
        private Player _player;
        private string cache = string.Empty;

        private bool _shouldUpdate { get; set; } = false;
        private UpdateTask _rateLimitTask => new(0.6f, OnRateLimitFinished);


        public DisplayManager(Player player) {
            _player = player;
        }
        public DisplayManager<TScreenName, TElementName> CreateElement(TElementName name, out Element? element, int lines = 0)
        {
            element = null;
            _elements[name] = new(lines);
            return this;
        }
        public Element SetElement(TElementName name, string content)
        {
            _elements[name].Set(content);
            return _elements[name];
        }
        public Screen<TElementName> AddScreen(TScreenName screenName)
        {
            Screen<TElementName> screen = new();
            _screens.Add(screenName, screen);

            return screen;
        }
        public void SetScreen(TScreenName screenName)
        {
            if (!_screens.ContainsKey(screenName)) throw new Exception("Invalid screen provided");
            SelectedScreen = screenName;
        }
        public void Update(TScreenName screenName)
        {
            if (_rateLimitTask.IsRunning)
            {
                _shouldUpdate = true;
                return;
            }

            if (SelectedScreen != null) { 
                if (screenName != null && screenName.Equals(SelectedScreen))
                {
                    ShowNewHints();
                }
            }
        }
        public void Update()
        {
            if (_rateLimitTask.IsRunning)
            {
                _shouldUpdate = true;
                return;
            }

            ShowNewHints();
        }
        private void ShowNewHints()
        {
            if (SelectedScreen == null || !_screens.ContainsKey(SelectedScreen)) throw new Exception("Invalid selected screen");
            _rateLimitTask.Run();

            Screen<TElementName> currentScreen = _screens[SelectedScreen];
            string content = currentScreen.ConvertToString(_elements);
            Hint hint = new(content, 9999999, true);
            _player.ShowHint(hint);
        }
        private void OnRateLimitFinished()
        {
            if (_shouldUpdate)
            {
                ShowNewHints();
            }
        }
    }
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class ShowHint : MonoBehaviour, ICommand
    {
        public string Command { get; } = "show-hint";
        public string[] Aliases { get; } = new[] { "sh" };
        public string Description { get; } = "Shows a hint to you";

        private readonly static ComAbilities Instance = ComAbilities.Instance;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            Hint hint = new Hint(string.Join(" ", arguments), 5000, true);
            player.ShowHint(hint);
            response = "true";
            return true;
        }
    }
}
