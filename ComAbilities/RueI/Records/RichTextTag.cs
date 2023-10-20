using RueI.Delegates;
using RueI.Enums;
using System.Text;

namespace RueI.Records
{
    public abstract record RichTextTag(string Name, TagStyle Style);

    public record MeasurementTag(string Name, Action<StringBuilder, ParserContext, float, MeasurementStyle> Parse) : RichTextTag(Name, TagStyle.Measurement);

    public record NoParamsTag(string Name, Action<StringBuilder, ParserContext> Parse) : RichTextTag(Name, TagStyle.NoParams);
}
