namespace ComAbilities.UI
{
    using System.Text;

    using Exiled.API.Features;
    using MEC;

    /// <summary>
    /// Represents a display for a player.
    /// </summary>
    public class PlayerDisplay
    {
        /// <summary>
        /// Gets the default height if a line-height is not provided.
        /// </summary>
        public const float DefaultHeight = 35; // in pixels;

        /// <summary>
        /// Gets a string used to close all tags.
        /// </summary>
        private const string Closer = "</noparse></align></color></b></i></cspace></line-height></line-indent></link></lowercase></uppercase></smallcaps></margin></mark></mspace></pos></size></s></u></voffset></width>";
        private const float HintRateLimit = 0.55f;

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

        private static Comparison<Element> Comparer { get; } = (Element first, Element other) => other.ZIndex - first.ZIndex;

        private List<Element> elements { get; } = new();

        private Player Player { get; set; }

        /// <summary>
        /// Adds an element to the player's display.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Add(Element element) => elements.Add(element);

        /// <summary>
        /// Adds multiple elements to the player's display.
        /// </summary>
        /// <param name="elementArr">The elements to add.</param>
        public void Add(params Element[] elementArr) => elements.AddRange(elementArr);

        /// <summary>
        /// Adds multiple elements to the player's display.
        /// </summary>
        /// <param name="elementIEnum">The <see cref="IEnumerable{Element}>"/> to add.</param>
        public void AddRange(IEnumerable<Element> elementIEnum) => elements.AddRange(elementIEnum);

        /// <summary>
        /// Removes an element from a player's display.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void Remove(Element element) => elements.Remove(element);

        /// <summary>
        /// Updates this display and shows the player the new hint, if the rate limit is not active
        /// </summary>
        public void Update()
        {
            if (!rateLimitActive)
            {
                rateLimitActive = true;
                Timing.CallDelayed(HintRateLimit, OnRateLimitFinished);

                Hint hint = new(ParseElements(), 9999999, true);
                Player.ShowHint(hint);
            }
            else
            {
                shouldUpdate = true;
                return;
            }
        }

        internal string ParseElements()
        {
            if (!elements.Any())
            {
                return string.Empty;
            }

            StringBuilder sb = new();
            float totalOffset = 0;

            float lastPosition = 0;
            float lastOffset = 0;

            elements.Sort(Comparer);

            for (int i = 0; i < elements.Count; i++)
            {
                Log.Debug(i);
                Element curElement = elements[i];
                ParsedData parsedData = curElement.ParsedData;

                if (i != 0)
                {
                    float calcedOffset = Element.CalculateOffset(lastPosition, lastOffset, curElement.Position);
                    Log.Debug(calcedOffset);
                    sb.Append($"<line-height={calcedOffset}px>\n</line-height>");
                    totalOffset += calcedOffset;
                } else
                {
                    totalOffset += curElement.Position;
                }

                sb.Append(parsedData.Content);
                sb.Append(Closer);

                totalOffset += parsedData.Offset;
                lastPosition = curElement.Position;
                lastOffset = parsedData.Offset;
            }

            sb.Insert(0, $"<line-height={totalOffset}px>\n");
            return sb.ToString();
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
