namespace ComAbilities.UI
{
    using System.Text;
    using System.Text.RegularExpressions;
    using Exiled.API.Features;
    using MEC;
    using VoiceChat;

    /// <summary>
    /// Text in a display with a position.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// Gets the Regex used to parse hints.
        /// </summary>
        protected static readonly Regex ParserRegex = new(@"<(?:line-height=(-?[0-9]\d*(?:\.\d+?)?)px>|/line-height>|noparse>|/noparse>|br>)|\n");

        /// <summary>
        /// Gets the text of the element.
        /// </summary>
        public virtual string Content { get; protected set; }
        /// <summary>
        /// Gets the position of the element relative to the baseline.
        /// </summary>
        public float Position { get; private set; } = 0;
        public float Offset { get; private set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="position">The position of the element, where 0 is the hint baseline (roughly middle-bottom of the screen)</param>
        /// <param name="content">The content of the element</param>
        internal Element(float position, string content)
        {

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
            content = $"<line-height={position}px><br><line-height={PlayerDisplay.DefaultHeight}>" + content;
            string newString = ParserRegex.Replace(content, (match) =>
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
        /// Sets the content of this element.
        /// </summary>
        /// <param name="content">The text to set the content to (will be parsed)</param>
        public virtual void Set(string content)
        {
            string parsedContent = Parse(content, Position, out float offset);
            Offset = offset;
            Content = parsedContent;
        }
    }

    /// <summary>
    /// Represents a display for a player.
    /// </summary>
    public class PlayerDisplay
    {
        /// <summary>
        /// Gets the default height if a line-height is not provided
        /// </summary>
        public const float DefaultHeight = 35; // in pixels;

        /// <summary>
        /// Gets a string used to close all tags.
        /// </summary>
        private const string Closer = "</noparse></align></color></b></i></cspace></line-height></line-indent></link></lowercase></uppercase></smallcaps></margin></mark></mspace></pos></size></s></u></voffset></width>";
        private const float HintRateLimit = 0.55f;

        private List<Element> elements = new();
        private Player Player { get; set; }

        private CoroutineHandle? rateLimitTask;
        private bool rateLimitActive = false;
        private bool shouldUpdate = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerDisplay"/> class.
        /// </summary>
        /// <param name="player">The <see cref="Exiled.API.Features.Player"/>to assign the display to.</param>
        public PlayerDisplay(Player player)
        {
            Player = player;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PlayerDisplay"/> class.
        /// </summary>
        ~PlayerDisplay()
        {
            if (rateLimitTask is CoroutineHandle ch)
            {
                Timing.KillCoroutines(ch);
            }
        }

        /// <summary>
        /// Creates a new element and puts it in the player's display.
        /// </summary>
        /// <param name="position">The position of the element, where 0 is the hint baseline (roughly middle-bottom of the screen).</param>
        /// <param name="content">The text to immediately set the element's content to.</param>
        /// <returns>The new element</returns>
        public Element CreateElement(float position, string content = "")
        {
            Element element = new(position, content);
            elements.Add(element);
            return element;
        }

        public string ParseElements()
        {
            StringBuilder sb = new();

            float offset = 0;
            foreach (var element in elements)
            {
                sb.Append($"<line-height={-offset}px><br></line-height>");
                sb.Append(element.Content);
                sb.Append(Closer);
                offset = element.Offset;
            }
            sb.Insert(0, $"<line-height={offset}>\n");
            return sb.ToString();
        }

        /// <summary>
        /// Updates this display and shows the player the new hint, if the rate limit is not active
        /// </summary>
        public void Update()
        {
            if (rateLimitActive)
            {
                shouldUpdate = true;
                return;
            }

            rateLimitActive = true;
            Timing.CallDelayed(HintRateLimit, OnRateLimitFinished);

            Hint hint = new(ParseElements(), 9999999, true);
            Player.ShowHint(hint);
        }

        private void OnRateLimitFinished()
        {
            rateLimitActive = false;
            if (shouldUpdate)
            {
                Update();
            }
        }
    }
}

