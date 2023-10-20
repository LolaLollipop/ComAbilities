using RueI.Records;
using System.Text;

namespace RueI.Enums
{
    internal enum ParserState
    {
        CollectingTags,
        DescendingTag,
        CollectingColorParams,
        CollectingMeasureParams
    }
}
