namespace RueI
{
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Exiled.API.Features;
    using RueI.Enums;
    using RueI.Records;

    /// <summary>
    /// Helps parse the content of elements.
    /// </summary>
    public class Parser
    {
        private readonly ParserNode baseNodes = new();
        private readonly List<RichTextTag> tags = new();

        public ReadOnlyCollection<RichTextTag> CurrentTags => tags.AsReadOnly();

        public void AddTag(RichTextTag tag)
        {
            tags.Add(tag);
            Reassemble();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParsedData Parse(string text)
        {
            ReadOnlyDictionary<char, float> charSizes = Constants.CharacterLengths;

            ParserState currentState = ParserState.CollectingTags;
            ParserNode? currentNode = null;
            StringBuilder tagBuffer = new();
            StringBuilder paramBuffer = new();

            StringBuilder sb = new();

            bool shouldParse = false;
            float currentHeight = Constants.DEFAULTHEIGHT; // in pixels
            float size = Constants.DEFAULTSIZE;
            float newOffset = 0;
            float currentLineWidth = 0;

            float currentCSpace = 0;
            bool isMonospace = false;
            bool isBold = false;

            CaseStyle currentCase = CaseStyle.Smallcaps;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void AddCharacter(char ch)
            {
                char functionalCase = currentCase switch
                {
                    CaseStyle.Smallcaps or CaseStyle.Uppercase => char.ToUpper(ch),
                    CaseStyle.Lowercase => char.ToLower(ch),
                    _ => ch
                };

                if (charSizes.TryGetValue(functionalCase, out float chSize))
                {
                    float multiplier = size / 35;
                    if (currentCase == CaseStyle.Smallcaps && char.IsLower(ch))
                    {
                        multiplier *= 0.8f;
                    }

                    if (isMonospace && currentLineWidth != 0)
                    {
                        currentLineWidth += currentCSpace;
                    }
                    else
                    {
                        currentLineWidth += (chSize * multiplier) + currentCSpace;
                        if (isBold)
                        {
                            currentLineWidth += Constants.BOLDINCREASE * multiplier;
                        }
                    }

                    sb.Append(ch);
                }
                else
                {
                    Log.Warn("INVALID CHARACTER " + ch);
                }
            }

            void FailTagMatch() // not a tag, unload buffer
            {
                sb.Append("<​"); // zero width space guarantees that the tag isnt matched
                foreach (char ch in tagBuffer.ToString())
                {
                    AddCharacter(ch);
                }

                foreach (char ch in paramBuffer.ToString())
                {
                    AddCharacter(ch);
                }

                tagBuffer.Clear();
                paramBuffer.Clear();

                currentState = ParserState.CollectingTags;
            }

            ParserContext GenerateContext()
            {
                return new(currentHeight, currentLineWidth, size, newOffset, currentCSpace, shouldParse, isMonospace, isBold, currentCase);
            }

            void LoadContext(ParserContext context)
            {
                (currentHeight, currentLineWidth, size, newOffset, currentCSpace, shouldParse, isMonospace, isBold, currentCase) = context;
            }

            foreach (char ch in text)
            {
                if (ch == '<')
                {
                    currentState = ParserState.CollectingTags;
                    currentNode = baseNodes;
                    continue;
                }
                else if (currentState == ParserState.DescendingTag)
                {
                    if ((ch > '\u0060' && ch < '\u007B') || ch == '-') // descend deeper into node
                    {
                        if (currentNode?.Branches?.TryGetValue(ch, out ParserNode node) == true)
                        {
                            currentNode = node;
                            tagBuffer.Append(ch);
                            continue;
                        }
                        else
                        {
                            FailTagMatch();
                        }
                    }
                    else if (ch == '=' || ch == '>')
                    {
                        if (currentNode?.Tag != null)
                        {
                            if (currentNode.Tag is NoParamsTag noParamsTag)
                            {
                                ParserContext context = GenerateContext();
                                noParamsTag.Parse(sb, context);
                                LoadContext(context);

                                tagBuffer.Clear();
                                continue;
                            }
                            else if (currentNode.Tag.Style != TagStyle.NoParams && ch == '=')
                            {
                                if (currentNode.Tag.Style == TagStyle.Measurement)
                                {
                                    currentState = ParserState.CollectingMeasureParams;
                                }
                                else
                                {
                                    currentState = ParserState.CollectingColorParams;
                                }

                                tagBuffer.Append("=");
                                continue;
                            }
                            else if (ch == '>' && (currentState == ParserState.CollectingColorParams || currentState == ParserState.CollectingMeasureParams)) {
                                if (currentState == ParserState.CollectingColorParams)
                                {

                                } 
                                else
                                {
                                    StringBuilder newParamBuffer = new();
                                    MeasurementStyle style = MeasurementStyle.Pixels;

                                    foreach (char paramChar in paramBuffer.ToString())
                                    {
                                        if (paramChar == 'e')
                                        {
                                            style = MeasurementStyle.Ems;
                                            break;
                                        } else if (paramChar == '%')
                                        {
                                            style = MeasurementStyle.Percentage;
                                            break;
                                        }

                                        newParamBuffer.Append(paramChar);
                                    }

                                    if (float.TryParse(newParamBuffer.ToString(), out float result))
                                    {
                                        MeasurementTag mesTag = (currentNode.Tag as MeasurementTag);
                                        ParserContext context = GenerateContext();

                                        mesTag.Parse(sb, context, result, style);
                                        LoadContext(context);

                                        tagBuffer.Clear();

                                        continue;
                                    } else
                                    {
                                        FailTagMatch();
                                    }
                                }
                            }
                            else
                            {
                                FailTagMatch();
                            }
                        }
                        else
                        {
                            FailTagMatch();
                        }
                    }
                }
                else if (currentState == ParserState.CollectingMeasureParams)
                {
                    paramBuffer.Append(ch);
                    continue;
                }

                AddCharacter(ch);
            }

            return new ParsedData(sb.ToString(), newOffset);
        }


        private void Reassemble()
        {
          //  baseNodes.Branches.Clear();

            foreach (RichTextTag tag in tags)
            {
                ParserNode currentNode = baseNodes;

                foreach (char ch in tag.Name)
                {
                    if (!currentNode.Branches.TryGetValue(ch, out ParserNode node))
                    {
                        node = new ParserNode();
                        currentNode.Branches.Add(ch, node);
                    }

                    currentNode = node;
                }

                currentNode.Tag = tag;
            }
        }

        public float CalculateCharacterLength(IEnumerable<char> chars, ParserContext context)
        {
            float buffer = 0;
            foreach (char ch in chars)
            {
                if (Constants.CharacterLengths.TryGetValue(ch, out float value))
                {

                }
            }
            return buffer;
        }

        internal float CalculateCharacterLength(char ch, ParserContext context)
        {
            float multiplier = context.Size;
            if (Constants.CharacterLengths.TryGetValue(ch, out float value))
            {
                return 1;
            } else
            {
                return 0;
            }
        }
    }
}
