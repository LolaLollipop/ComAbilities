namespace ComAbilities.UI
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines a record that contains information used for displaying multiple elements
    /// </summary>
    /// <param name="Content">The hint's content.</param>
    /// <param name="Offset">The offset that should be applied.</param>
    public record struct ParsedData(string Content, float Offset);

    /// <summary>
    /// Represents a non-cached element that evaluates and parses a function when getting its content.
    /// </summary>
    public class DynamicElement : Element
    {
        /// <summary>
        /// Defines a method used to get content for an element.
        /// </summary>
        /// <returns>A string with the new content.</returns>
        public delegate string GetContent();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicElement"/> class.
        /// </summary>
        /// <param name="contentGetter">A dekegate invoked every time the display is updated.</param>
        public DynamicElement(GetContent contentGetter)
        {
            ContentGetter = contentGetter;
        }

        /// <summary>
        /// Gets or sets a method that returns the new content and is called every time the display is updated
        /// </summary>
        public GetContent ContentGetter { get; set; }

        public override ParsedData ParsedData
        {
            get => Parse(ContentGetter());
        }
    }

    /// <summary>
    /// Represents a simple cached element with settable content.
    /// </summary>
    public class SetElement : Element
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetElement"/> class.
        /// </summary>
        /// <param name="position">The position of the element, where 0 is the hint baseline (roughly middle-bottom of the screen)</param>
        /// <param name="content">The content of the element</param>
        public SetElement(float position, string content = "")
        {
            Position = position;
            Content = content;
            ParsedData = Parse(content);
        }

        /// <summary>
        /// Gets the raw content of the element.
        /// </summary>
        public virtual string Content { get; protected set; }

        /// <summary>
        /// Gets the offset of the element.
        /// </summary>
        public float Offset { get; private set; } = 0;

        /// <summary>
        /// Sets the content of this element.
        /// </summary>
        /// <param name="content">The text to set the content to (will be parsed)</param>
        public virtual void Set(string content)
        {
            Content = content;
            ParsedData = Parse(content);
        }
    }

    /// <summary>
    /// Represents the base class for all elements.
    /// </summary>
    public abstract class Element : IComparable<Element>
    {
        protected static readonly Regex ParserRegex = new(@"<(?:line-height=(-?[0-9]\d*(?:\.\d+?)?)px>|/line-height>|noparse>|/noparse>|br>)|\n");

        /// <summary>
        /// Gets or sets a value indicating whether or not this element is enabled and will show.
        /// </summary>
        public virtual bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets the data used for parsing.
        /// </summary>
        public virtual ParsedData ParsedData { get; protected set; }

        /// <summary>
        /// Gets or sets the position of the element, in pixels, relative to the baseline.
        /// </summary>
        public virtual float Position { get; set; }

        /// <summary>
        /// Gets or sets the priority of the hint (determining if it shows above another hint).
        /// </summary>
        public virtual int ZIndex { get; set; } = 1;

        /// <summary>
        /// Calculates the offset for two hints.
        /// </summary>
        /// <param name="hintOnePos">The first hint's vertical position.</param>
        /// <param name="hintOneTotalLines">The first hint's total line-height, excluding the vertical position.</param>
        /// <param name="hintTwoPos">The second hint's vertical position.</param>
        /// <returns>A float indicating the new offset.</returns>
        public static float CalculateOffset(float hintOnePos, float hintOneTotalLines, float hintTwoPos)
        {
            float calc = (hintOnePos + (2 * hintOneTotalLines)) - hintTwoPos;
            return calc / -2;
        }

        public int CompareTo(Element other) => this.ZIndex - other.ZIndex;

        /// <summary>
        /// Parses an element's content, providing the offset to maintain a constant position
        /// </summary>
        /// <param name="content">The content to parsed.</param>
        /// <returns>A <see cref="ParsedData"/> containing the new content and position</returns>
        protected static ParsedData Parse(string content)
        {
            bool shouldParse = true;
            float currentHeight = PlayerDisplay.DefaultHeight; // in pixels
            float newOffset = 0;

            content = $"<line-height={PlayerDisplay.DefaultHeight}px>" + content;
            string newString = ParserRegex.Replace(content, (match) =>
            {
                string small = match.Value.Substring(0, Math.Min(5, match.Value.Length));

                switch (small)
                {
                    case "<line" when shouldParse:
                        currentHeight = float.Parse(match.Groups[1].ToString());
                        break;
                    case "</lin" when shouldParse:
                        currentHeight = PlayerDisplay.DefaultHeight;
                        return $"<line-height={currentHeight}px>";
                    case "</nop":
                        shouldParse = true;
                        break;
                    case "<nopa":
                        shouldParse = false;
                        break;
                    case "<br>" when shouldParse:
                        newOffset += currentHeight;
                        break;
                    case "\n":
                        newOffset += currentHeight;
                        break;
                }

                return match.ToString();
            });
            return new ParsedData(newString, newOffset);
        }
    }
}