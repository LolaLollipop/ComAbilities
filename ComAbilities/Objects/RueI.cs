using Exiled.API.Features;
using MEC;
using System.Text;
using System.Text.RegularExpressions;


/// Handles 
namespace ComAbilities.UI
{ 
    public abstract class Box
    {
        public abstract string HorizontalBar { get; set; }
        public abstract string VerticalBar { get; set; }
        public abstract string TopRightCorner { get; set; }
        public abstract string BottomRightCorner { get; set; }
        public abstract string TopLeftCorner { get; set; }
        public abstract string BottomLeftCorner { get; set; }
    }
    /// <summary>
    /// Text in a display with a position
    /// </summary>
    public class Element
    {
       // private static readonly Regex lineRegex = new("\n|<br>");
        private static readonly Regex parserRegex = new(@"<(?:line-height=(-?[0-9]\d*(?:\.\d+?)?)px>|/line-height>|noparse>|/noparse>|br>)|\n");

        /// <summary>
        /// The text of the element
        /// </summary>
        public string Content { get; private set; }
        public float Position { get; private set; } = 0;
        public float Offset { get; private set; } = 0;

        /// <summary>
        /// Creates a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="position">The position of the element, where 0 is the hint baseline (roughly middle-bottom of the screen)</param>
        /// <param name="content">The content of the element</param>
        internal Element(float position, string content) {

            Content = Parse(content, position, out float newOffset);
            Offset = newOffset;
            Position = position;
        }

        /// <summary>
        /// Parses an element's content, providing the offset to maintain a constant position
        /// </summary>
        /// <param name="content">The content to parse</param>
        /// <param name="position">The position of the element, where 0 is the hint baseline (roughly middle-bottom of the screen)</param>
        /// <param name="offset">The returned offset to account for other elements</param>
        /// <returns>The parsed string</returns>
        public static string Parse(string content, float position, out float offset)
        {
            bool shouldParse = true;
            float currentHeight = PlayerDisplay.DefaultHeight; // in pixels
            float newOffset = 0;

            Log.Debug(position);
            content = $"<line-height={position}px><br>" + content;
            string newString = parserRegex.Replace(content, (match) =>
            {
                string small = match.Value.Substring(0, Math.Min(5, match.Value.Length));

                switch (small)
                {
                    case "<line":
                        if (!shouldParse) break;
                        currentHeight = float.Parse(match.Groups[1].ToString());
                        break;
                    case "</lin":
                        if (!shouldParse) break;
                        currentHeight = PlayerDisplay.DefaultHeight;
                        return $"<line-height={currentHeight}px>";
                    case "</nop":
                        shouldParse = true;
                        break;
                    case "<nopa":
                        shouldParse = false;
                        break;
                    case "<br>":
                        if (shouldParse) newOffset += currentHeight;
                        break;
                    case "\n":
                        newOffset += currentHeight;
                        break;
                }
                return match.ToString();
            });
            offset = newOffset;
            return newString;
        }

        /// <summary>
        /// Sets the content of this element
        /// </summary>
        /// <param name="content">The text to set the content to (will be parsed)</param>
        public void Set(string content)
        {
            //MatchCollection matches = parserRegex.Matches(content);
            string parsedContent = Parse(content, Position, out float offset);
            Offset = offset;
            Content = parsedContent;
        }
       // public static string GetFirstFive(string content) => content.Substring(0, Math.Min(5, content.Length));
    }
    public class PlayerDisplay
    {
        /// <summary>
        /// Gets the default height if a line-height is not provided
        /// </summary>
        public const float DefaultHeight = 35; // in pixels;

        private List<Element> _elements = new();
        private const string Closer = "</align></color></b></i></cspace></line-height></line-indent></link></lowercase></uppercase></smallcaps></margin></mark></mspace></noparse></pos></size></s></u></voffset></width>";
        private Player Player { get; set; }

        private CoroutineHandle? RateLimitTask;
        private bool RateLimitActive = false;
        private const float HintRateLimit = 0.55f;
        private bool ShouldUpdate = false;

        /// <summary>
        /// Creates a new instance of the <see cref="PlayerDisplay"/> class.
        /// </summary>
        /// <param name="player">The <see cref="Exiled.API.Features.Player"/>to assign the display to</param>
        public PlayerDisplay(Player player)
        {
            Player = player;
        }

        ~PlayerDisplay()
        {
            if (RateLimitTask is CoroutineHandle ch) Timing.KillCoroutines(ch);
        }

        /// <summary>
        /// Creates a new element and puts it in the player's display
        /// </summary>
        /// <param name="position">The position of the element, where 0 is the hint baseline (roughly middle-bottom of the screen)</param>
        /// <param name="content">The text to immediately set the element's content to</param>
        /// <returns>The new element</returns>
        public Element CreateElement(float position, string content = "")
        {
            Element element = new(position, content);
            _elements.Add(element);
            return element;
        }
        public string ParseElements()
        {
            StringBuilder sb = new();

            float offset = 0;
            foreach (var element in _elements)
            {
                sb.Append($"<line-height={-offset}px><br>");
                sb.Append(element.Content);
                sb.Append(Closer);
                offset = element.Offset;
            }
            Log.Debug(sb.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Updates this display and shows the player the new hint, if the rate limit is not active
        /// </summary>
        public void Update()
        {
            if (RateLimitActive)
            {
                ShouldUpdate = true;
                return;
            }
            RateLimitActive = true;
            Timing.CallDelayed(HintRateLimit, OnRateLimitFinished);

            Hint hint = new(ParseElements(), 9999999, true);
            Player.ShowHint(hint);
        }
        private void OnRateLimitFinished()
        {
            RateLimitActive = false;
            if (ShouldUpdate)
            {
                Update();
            }
        }
    }
}