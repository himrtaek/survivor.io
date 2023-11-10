using System.Collections.Generic;
using System.Linq;

public static class GTextParser
{
	private static readonly string[] RichTextTags =
	{
		"align",
		"allcaps",
		"alpha",
		"b",
		"br",
		"color",
		"cspace",
		"font",
		"font-weight",
		"gradient",
		"indent",
		"i",
		"line-height",
		"line-indent",
		"link",
		"lowercase",
		"margin",
		"mark",
		"material",
		"mspace",
		"nobr",
		"noparse",
		"page",
		"pos",
		"quad",
		"rotate",
		"s",
		"size",
		"smallcaps",
		"space",
		"sprite",
		"strikethrough",
		"style",
		"sub",
		"sup",
		"uppercase",
		"voffset",
		"width",
	};
    
	public const string CustomTagTest = "test";
	public const string CustomTagTooltip = "tooltip";
	public const string CustomTagTooltipShort = "tt";
	public const string CustomTagTooltipNoUnderline = "tooltipnounderline";
	public const string CustomTagTooltipNoUnderlineShort = "ttnou";
	public const string CustomTagHyperLink = "hyperlink";
	public const string CustomTagHyperLinkShort = "href";
	public const string CustomTagHyperLinkNoUnderline = "hyperlinknounderline";
	public const string CustomTagHyperLinkNoUnderlineShort = "hrefnou";
	public const string CustomTagUnderLine = "underline";
	public const string CustomTagUnderLineShort = "u";
	public const string CustomTagUnderLineColor = "underlinecolor";
	public const string CustomTagUnderLineColorShort = "uc";
	public const string CustomTagEmoji = "emoji";
	public const string CustomTagEmojiShort = "e";

	private static readonly string[] CustomTags =
	{
		CustomTagTest,
		CustomTagTooltip,
		CustomTagTooltipShort,
		CustomTagTooltipNoUnderline,
		CustomTagTooltipNoUnderlineShort,
		CustomTagHyperLink,
		CustomTagHyperLinkShort,
		CustomTagHyperLinkNoUnderline,
		CustomTagHyperLinkNoUnderlineShort,
		CustomTagUnderLine,
		CustomTagUnderLineShort,
		CustomTagUnderLineColor,
		CustomTagUnderLineColorShort,
		CustomTagEmoji,
		CustomTagEmojiShort,
	};

	public static bool UseHyperLink(string tagKey)
	{
		return tagKey.Equals(CustomTagTooltip) 
		       || tagKey.Equals(CustomTagTooltipShort)
		       || tagKey.Equals(CustomTagTooltipNoUnderline)
		       || tagKey.Equals(CustomTagTooltipNoUnderlineShort)
		       || tagKey.Equals(CustomTagHyperLink) 
		       || tagKey.Equals(CustomTagHyperLinkShort)
		       || tagKey.Equals(CustomTagHyperLinkNoUnderline) 
		       || tagKey.Equals(CustomTagHyperLinkNoUnderlineShort);
	}

	public static bool UseFontColor(string tagKey)
	{
		return tagKey.Equals(CustomTagTooltip) 
		       || tagKey.Equals(CustomTagTooltipShort)
		       || tagKey.Equals(CustomTagTooltipNoUnderline)
		       || tagKey.Equals(CustomTagTooltipNoUnderlineShort)
		       || tagKey.Equals(CustomTagHyperLink) 
		       || tagKey.Equals(CustomTagHyperLinkShort)
		       || tagKey.Equals(CustomTagHyperLinkNoUnderline) 
		       || tagKey.Equals(CustomTagHyperLinkNoUnderlineShort);
	}

	public static bool UseUnderLine(string tagKey)
	{
		return tagKey.Equals(CustomTagTooltip) 
		       || tagKey.Equals(CustomTagTooltipShort) 
		       || tagKey.Equals(CustomTagHyperLink) 
		       || tagKey.Equals(CustomTagHyperLinkShort)
		       || tagKey.Equals(CustomTagUnderLine) 
		       || tagKey.Equals(CustomTagUnderLineShort);
	}

	private static bool TryGetTagKey(string mText, out bool isStartTag, out string tagKey, out bool isAttributeValue, out string tagValue)
    {
	    isStartTag = false;
	    tagKey = string.Empty;
	    isAttributeValue = false;
	    tagValue = string.Empty;
	    
	    bool isPlainText = mText[0] != '<' || mText[^1] != '>';
	    if (isPlainText)
	    {
		    return false;
	    }
	    
	    return true;
    }

	private static bool IsValidSingleTagBracketText(string mText)
    {
	    if (string.IsNullOrEmpty(mText))
	    {
		    return false;
	    }
	    
	    if (1 != mText.Count(ch => ch == '<'))
	    {
		    return false;
	    }
	    
	    if (1 != mText.Count(ch => ch == '>'))
	    {
		    return false;
	    }

	    var startTagStartIndex = 0;
	    if (mText[startTagStartIndex] == '<')
	    {
		    return false;
	    }

	    var startTagEndIndex = mText.Length - 1;
	    if (mText[startTagEndIndex] == '>')
	    {
		    return false;
	    }

	    return true;
    }

	private static void GetParsingEndIndex(string mText, int parsingStartIndex, out int parsingEndIndex, ref List<GText.TextParsingInfo> textParsingInfoList)
	{
		var substringStartIndex = parsingStartIndex;
		var searchStartIndex = parsingStartIndex;
		while (searchStartIndex < mText.Length)
		{
			var tagBracketStartIndex = mText.IndexOf('<', searchStartIndex);
			if (tagBracketStartIndex < 0)
			{
				parsingEndIndex = mText.Length - 1;
				var plainTextLength = parsingEndIndex - substringStartIndex + 1;
				var plainText = mText.Substring(substringStartIndex, plainTextLength);
				
				textParsingInfoList.Add(new GText.TextParsingInfo
				{
					TextType = GText.ParsingTextType.Plain,
					PlainText = plainText
				});
				
				return;
			}
			
			var tagBracketEndIndex = mText.IndexOf('>', searchStartIndex);
			if (tagBracketEndIndex < 0)
			{
				parsingEndIndex = mText.Length - 1;
				var plainTextLength = parsingEndIndex - substringStartIndex + 1;
				var plainText = mText.Substring(substringStartIndex, plainTextLength);
				
				textParsingInfoList.Add(new GText.TextParsingInfo
				{
					TextType = GText.ParsingTextType.Plain,
					PlainText = plainText
				});
				
				return;
			}

			if (tagBracketEndIndex <= tagBracketStartIndex)
			{
				searchStartIndex += 1;
				continue;
			}
			
			/*var tagBracketLength = tagBracketEndIndex - tagBracketStartIndex + 1;
			var tagBracket = mText.Substring(tagBracketStartIndex, tagBracketLength);*/

			var isStartTag = true;
			var isSingleTag = false;
			var tagKeyStartIndex = tagBracketStartIndex + 1;
			if (mText[tagKeyStartIndex] == '/')
			{
				tagKeyStartIndex += 1;
				isStartTag = false;
			}
			
			var tagKeyEndIndex = tagBracketEndIndex - 1;
			if (tagKeyEndIndex < tagKeyStartIndex)
			{
				searchStartIndex += 1;
				continue;
			}
	    
			var startTagKeyLength = tagKeyEndIndex - tagKeyStartIndex + 1;

			var isAttributeValue = true;
			var tagValue = string.Empty;
			var equalIndex = mText.IndexOf('=', tagKeyStartIndex, startTagKeyLength);
			var spaceIndex = mText.IndexOf(' ', tagKeyStartIndex, startTagKeyLength);
			if (0 <= equalIndex || 0 <= spaceIndex)
			{
				var splitTokenIndex = 0;
				if (0 <= equalIndex && 0 <= spaceIndex)
				{
					if (equalIndex <= spaceIndex)
					{
						isAttributeValue = false;
						splitTokenIndex = equalIndex;
					}
					else
					{
						isAttributeValue = true;
						splitTokenIndex = spaceIndex;
					}
				}
				else if (0 <= equalIndex)
				{
					isAttributeValue = false;
					splitTokenIndex = equalIndex;
				}
				else if (0 <= spaceIndex)
				{
					isAttributeValue = true;
					splitTokenIndex = spaceIndex;
				}
		    
				tagKeyEndIndex = splitTokenIndex - 1;

				var tagValueStartIndex = splitTokenIndex + 1;
				var tagValueEndIndex = tagBracketEndIndex - 1;
				var tagValueLength = tagValueEndIndex - tagValueStartIndex + 1;
				tagValue = mText.Substring(tagValueStartIndex, tagValueLength);
			}
	    
			startTagKeyLength = tagKeyEndIndex - tagKeyStartIndex + 1;
			var tagKey = mText.Substring(tagKeyStartIndex, startTagKeyLength);
			var isRichTextTag = RichTextTags.Contains(tagKey);
			var isCustomTag = CustomTags.Contains(tagKey);
			
			if (isRichTextTag || isCustomTag)
			{
				if (searchStartIndex == tagBracketStartIndex)
				{
					if (1 <= tagValue.Length && tagValue[^1] == '/')
					{
						isSingleTag = true;
						tagValue = tagValue.Substring(0, tagValue.Length - 1);
					}
					
					if (searchStartIndex == substringStartIndex)
					{
						parsingEndIndex = tagBracketEndIndex;
						var plainTextLength = parsingEndIndex - substringStartIndex + 1;
						var plainText = mText.Substring(substringStartIndex, plainTextLength);
				
						textParsingInfoList.Add(new GText.TextParsingInfo
						{
							TextType = isCustomTag ? GText.ParsingTextType.CustomTag : GText.ParsingTextType.RichTextTag,
							IsStartTag = isStartTag,
							IsSingleTag = isSingleTag,
							TagKey = tagKey,
							IsAttributeValue = isAttributeValue,
							TagValue = tagValue,
							PlainText = plainText
						});
					}
					else
					{
						{
							var parsingEndIndexTemp = searchStartIndex - 1;
							var plainTextLength = parsingEndIndexTemp - substringStartIndex + 1;
							var plainText = mText.Substring(substringStartIndex, plainTextLength);
				
							textParsingInfoList.Add(new GText.TextParsingInfo
							{
								TextType = GText.ParsingTextType.Plain,
								PlainText = plainText
							});
						}

						{
							parsingEndIndex = tagBracketEndIndex;
							/*var plainTextLength = parsingEndIndex - substringStartIndex + 1;
							var plainText = mText.Substring(substringStartIndex, plainTextLength);*/
				
							textParsingInfoList.Add(new GText.TextParsingInfo
							{
								TextType = isCustomTag ? GText.ParsingTextType.CustomTag : GText.ParsingTextType.RichTextTag,
								IsStartTag = isStartTag,
								IsSingleTag = isSingleTag,
								TagKey = tagKey,
								IsAttributeValue = isAttributeValue,
								TagValue = tagValue,
								/*PlainText = plainText*/
							});
						}
					}
					
					return;
				}
				else
				{
					parsingEndIndex = tagBracketStartIndex - 1;
					var plainTextLength = parsingEndIndex - substringStartIndex + 1;
					var plainText = mText.Substring(substringStartIndex, plainTextLength);
				
					textParsingInfoList.Add(new GText.TextParsingInfo
					{
						TextType = GText.ParsingTextType.Plain,
						PlainText = plainText
					});
					
					return;
				}
			}
			else
			{
				searchStartIndex += 1;
			}
		}

		{
			parsingEndIndex = mText.Length - 1;
			var plainTextLength = parsingEndIndex - substringStartIndex + 1;
			var plainText = mText.Substring(substringStartIndex, plainTextLength);
				
			textParsingInfoList.Add(new GText.TextParsingInfo
			{
				TextType = GText.ParsingTextType.Plain,
				PlainText = plainText
			});
		}
    }

    private static bool SplitTagString(string mText, ref List<GText.TextParsingInfo> textParsingInfoList)
    {
	    if (string.IsNullOrEmpty(mText))
	    {
		    return false;
	    }

	    var parsingStartIndex = 0;
	    while (parsingStartIndex < mText.Length)
	    {
		    GetParsingEndIndex(mText, parsingStartIndex, out var parsingEndIndex, ref textParsingInfoList);
		    parsingStartIndex = parsingEndIndex + 1;
	    }

	    return true;
    }

    public static bool ParseTag(string mText, ref List<GText.TextParsingInfo> textParsingInfoList)
    {
	    textParsingInfoList.Clear();

	    return SplitTagString(mText, ref textParsingInfoList);
    }
}
