using RueI.Enums;

namespace RueI.Records
{

    public record ParserContext(
        float CurrentLineHeight, 
        float CurrentLineWidth, 
        float Size,
        float NewOffset,
        float CurrentCSpace,
        bool ShouldParse, 
        bool IsMonospace,
        bool IsBold,
        CaseStyle CurrentCase
    );
}
