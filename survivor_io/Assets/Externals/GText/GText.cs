/*
 * GText, Emoji and Hyper Link Solution for UGUI Text
 * by Garson
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JHT.Scripts.Common;
using JHT.Scripts.GameObjectPool;
using JHT.Scripts.ResourceManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class GText : Text, IPointerClickHandler, IPointerMoveHandler
{
	private RectTransform _rectTransform;

	private RectTransform RectTransform
	{
		get
		{
			if (_rectTransform.IsNull(false))
			{
				_rectTransform = GetComponent<RectTransform>();
			}

			return _rectTransform;
		}
	}
	
	[Serializable]
	public class HrefClickEvent : UnityEvent<GText, HrefInfo, Rect> { }
    
	public class UnderlineInfo
	{
		public string Text;
		public bool Show;
		public int StartIndex;
		public int StartIndexWithoutSpace;
		public Color ColorValue;
		public readonly List<Rect> BoxList = new ();
	}

	public class HrefInfo : UnderlineInfo
	{
		public string TagKey;
		public string Url;
		public bool Hovered;
	}

	// Truncate 예외 처리
	private readonly List<int> _uiVertexIndexList = new ();
	private FieldInfo _filedInfo;
	
    // 문자열 성능을 위한 텍스트 빌더
    private readonly StringBuilder _builderRichText = new ();
    private readonly StringBuilder _builderShown = new ();
    
    // 파싱 정보
    private List<TextParsingInfo> _textParsingInfoList = new ();
    
    // 하이퍼링크 정보
    private readonly List<HrefInfo> _hrefs = new ();
    
    // 언더라인 정보
    private readonly List<UnderlineInfo> _underlines = new ();
    
    // 텍스트 임시 정점 정보
    private readonly UIVertex[] _tempVerts = new UIVertex[4];
    
    // 하이퍼링크 이벤트
    public bool enableClick = true;
    public bool enableHover = true;
    
    public HrefClickEvent HrefClick { get; } = new ();
    
    public HrefClickEvent HrefHoverEnter { get; } = new ();
    
    public HrefClickEvent HrefHoverExit { get; } = new ();
    
    // 화면에 보여지는 텍스트
    private string _outputText = "";

    // UI 카메라 추가 설정용
    private Camera _uiCamera;

    // 기본 너비 오버라이드
    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(_outputText, settings) / pixelsPerUnit;
        }
    }
    
    // 기본 높이 오버라이드
    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(RectTransform.rect.size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(_outputText, settings) / pixelsPerUnit;
        }
    }

    // 문자열
    public override string text
    {
        get => m_Text;

        set
        {
            ParseText(value);
            base.text = value;
        }
    }

    private const char EmSpace = '\u2001';
    private readonly List<PosStringTuple> _emojiReplacements = new ();
    private static readonly Dictionary<string, Rect> EmojiRects = new ();
    private readonly List<GameObject> _rawImages = new ();

    private struct PosStringTuple
    {
	    public int Pos;
	    public string Emoji;

	    public PosStringTuple(int p, string s)
	    {
		    Pos = p;
		    Emoji = s;
	    }
    }

    private static void ParseEmojiInfo(string inputString)
    {
	    EmojiRects.Clear();
	    
	    using var reader = new StringReader(inputString);
	    var line = reader.ReadLine();
	    while (line is { Length: > 1 })
	    {
		    var split = line.Split(' ');
		    var x = float.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture);
		    var y = float.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture);
		    var width = float.Parse(split[3], System.Globalization.CultureInfo.InvariantCulture);
		    var height = float.Parse(split[4], System.Globalization.CultureInfo.InvariantCulture);
		    EmojiRects[split[0]] = new Rect(x, y, width, height);

		    line = reader.ReadLine();
	    }
    }

    private IEnumerator SetUITextThatHasEmoji()
    {
	    yield return null;

	    if (false == Application.isPlaying)
	    {
		    yield break;
	    }

	    foreach (var it in _rawImages)
	    {
		    it.SetActiveFalseWithPool();
	    }

        var textGen = cachedTextGenerator;
        for (var j = 0; j < _emojiReplacements.Count; j++)
        {
            var emojiIndex = _emojiReplacements[j].Pos;
            var newRawImage = GameObjectPoolManager.Instance.GetOrNewObject("EmojiInfo/RawImage", transform);
            _rawImages.Add(newRawImage);
            
            if (newRawImage.TryGetComponent(out RectTransform newRawImageRect))
            {
	            var sizeDelta = newRawImageRect.sizeDelta;
	            sizeDelta.x = fontSize;
	            sizeDelta.y = fontSize;
	            
	            newRawImageRect.sizeDelta = sizeDelta;
            }
            
            var imagePos = new Vector3(textGen.verts[emojiIndex * 4].position.x, textGen.verts[emojiIndex * 4].position.y, 0);
            
            if (TryGetComponentInRecursiveParent<CanvasScaler>(gameObject, out var canvasScaler))
            {
	            var screenWidth = Screen.width;
	            var screenHeight = Screen.height;
#if UNITY_EDITOR
	            screenWidth = Camera.main.pixelWidth;
	            screenHeight = Camera.main.pixelHeight;
#endif
	            
	            var canvasScale = GetScale(screenWidth, screenHeight, canvasScaler);
	            imagePos /= canvasScale;
            }
            
            // 좌측으로 치우지는 문제를 해결하기 위해 임시 조치
            // 더 좋은 방법이 있으면 수정해도 됨
            imagePos.x += fontSize / 10.0f;
            
            newRawImage.transform.localPosition = imagePos;

            var ri = newRawImage.GetComponent<RawImage>();
            ri.uvRect = EmojiRects[_emojiReplacements[j].Emoji];
        }
    }

    protected override void Start()
    {
	    base.Start();

	    _uiCamera = Camera.main;
    }

    public static void LoadEmojiInfo()
    {
	    var textAsset = ResourceManager.Instance.LoadOriginalAsset<TextAsset>("EmojiInfo/BakedEmojisInfo");
	    if (textAsset.IsNull())
	    {
		    return;
	    }
	    
	    ParseEmojiInfo(textAsset.text);
    }
    
    // UI 요소가 정점을 생성해야 할 때 콜백 함수.
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;
        
        Profiler.BeginSample("GText.Total");
        
        ParseText(m_Text);
        
        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        var extents = RectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate(_outputText, settings);

        // Apply the offset to the vertices
        var verts = cachedTextGenerator.verts;
        var unitsPerPixel = 1 / pixelsPerUnit;
        
        // 마지막 글자가 안보이는 문제로 인해 주석 처리
        #region
		/*//Last 4 verts are always a new line... (\n)
        int vertCount = verts.Count - 4;*/
        #endregion
        
        var vertCount = verts.Count;
        /*_prevUIVertexList = verts.ToList();*/

        // We have no verts to process just return (case 1037923)
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }
        
        var roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (var i = 0; i < vertCount; ++i)
            {
                var tempVertsIndex = i & 3;
                _tempVerts[tempVertsIndex] = verts[i];
                _tempVerts[tempVertsIndex].position *= unitsPerPixel;
                _tempVerts[tempVertsIndex].position.x += roundingOffset.x;
                _tempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(_tempVerts);
            }
        }
        else
        {
            for (var i = 0; i < vertCount; ++i)
            {
                var tempVertIndex = i & 3;
                {
                    _tempVerts[tempVertIndex] = verts[i];
                    _tempVerts[tempVertIndex].position *= unitsPerPixel;
                    if (tempVertIndex == 3)
                        toFill.AddUIVertexQuad(_tempVerts);
                }
            }
            ComputeBoundsInfo(toFill);
            DrawUnderLine(toFill);
        }

        m_DisableFontTextureRebuiltCallback = false;
	    
        StartCoroutine(SetUITextThatHasEmoji());
        
        Profiler.EndSample();
    }

    /*private void Update()
    {
	    ShowDebugBox();
    }

    private List<UIVertex> _prevUIVertexList = null;
    private void ShowDebugBox()
    {
	    if (_prevUIVertexList == null)
	    {
		    return;
	    }
	    
	    var scaleX = transform.lossyScale.x;
	    var scaleY = transform.lossyScale.y;
	    
	    if (TryGetComponentInRecursiveParent<CanvasScaler>(gameObject, out var canvasScaler))
	    {
		    var canvasScale = GetScale(Screen.width, Screen.height, canvasScaler);
		    scaleX /= canvasScale;
		    scaleY /= canvasScale;
	    }

	    var position = transform.position;
	    for (var i = 0; i < _prevUIVertexList.Count; i += 4)
	    {
		    Debug.DrawLine(_prevUIVertexList[i + 0].position * scaleX + position, _prevUIVertexList[i + 1].position * scaleY + position, Color.red, 0.1f);
		    Debug.DrawLine(_prevUIVertexList[i + 1].position * scaleX + position, _prevUIVertexList[i + 2].position * scaleY + position, Color.red, 0.1f);
		    Debug.DrawLine(_prevUIVertexList[i + 2].position * scaleX + position, _prevUIVertexList[i + 3].position * scaleY + position, Color.red, 0.1f);
		    Debug.DrawLine(_prevUIVertexList[i + 3].position * scaleX + position, _prevUIVertexList[i + 0].position * scaleY + position, Color.red, 0.1f);
	    }
    }*/

    private static int GetWhiteSpaceCountFromStringBuilder(StringBuilder sb)
    {
	    var count = 0; 
	    for (var i = 0; i < sb.Length; i++)
	    {
		    if (sb[i] == ' ')
		    {
			    count++;
		    }
	    }
	    
	    // 성능 개선 필요
	    count += Regex.Matches(sb.ToString(), "\n").Count;
	    return count;
    }
    
    public enum ParsingTextType
    {
	    Plain,
	    RichTextTag,
	    CustomTag
    }

    public struct TextParsingInfo
    {
	    public ParsingTextType TextType;
	    public bool IsStartTag;
	    public bool IsSingleTag;
	    public string TagKey;
	    public bool IsAttributeValue;
	    public string TagValue;
	    public string PlainText;
    }

    // 문자열 파싱
    private void ParseText(string mText)
    {
	    Profiler.BeginSample("GText.ParseText");
        /*if (!Application.isPlaying)
        {
            _outputText = mText;
            return;
        }*/

        _builderRichText.Length = 0;
        _builderShown.Length = 0;
        _textParsingInfoList.Clear();
        _hrefs.Clear();
        _underlines.Clear();
        _emojiReplacements.Clear();

        if (GTextParser.ParseTag(mText, ref _textParsingInfoList) && _textParsingInfoList.Count > 0)
        {
            for (var i = 0; i < _textParsingInfoList.Count; i++)
            {
                var textParsingInfo = _textParsingInfoList[i];
                switch (textParsingInfo.TextType)
                {
	                case ParsingTextType.Plain:
	                {
		                BuildPlain(_builderRichText, _builderShown, textParsingInfo);
	                }
		                break;
	                case ParsingTextType.RichTextTag:
	                {
		                BuildRichTextTag(_builderRichText, textParsingInfo);
	                }
		                break;
	                case ParsingTextType.CustomTag:
	                {
		                BuildCustomTag(_builderRichText, _builderShown, _textParsingInfoList, i);
	                }
		                break;
	                default:
		                throw new ArgumentOutOfRangeException();
                }
            }
            
            _outputText = _builderRichText.ToString();
        }
        else
        {
	        _outputText = mText;
        }
        
        Profiler.EndSample();
    }

    private bool TryGetNextPlainText(List<TextParsingInfo> textParsingInfoList, int textParsingInfoListIndex, out string plainText)
    {
	    for (var i = textParsingInfoListIndex + 1; i < textParsingInfoList.Count; i++)
	    {
		    var textParsingInfo = textParsingInfoList[i];
		    if (ParsingTextType.Plain == textParsingInfo.TextType)
		    {
			    plainText = textParsingInfo.PlainText;
			    return true;
		    }
	    }
	    
	    plainText = String.Empty;
	    return false;
    }

    private void BuildPlain(StringBuilder sbRichText, StringBuilder sbShown, TextParsingInfo textParsingInfo)
    {
	    sbRichText.Append(textParsingInfo.PlainText);
	    sbShown.Append(textParsingInfo.PlainText);
    }
    
    private void BuildRichTextTag(StringBuilder sbRichText, TextParsingInfo textParsingInfo)
    {
	    if (textParsingInfo.IsStartTag)
	    {
		    sbRichText.Append("<");
		    sbRichText.Append(textParsingInfo.TagKey);
		    sbRichText.Append(textParsingInfo.IsAttributeValue ? " " : "=");
		    sbRichText.Append(textParsingInfo.TagValue);
		    sbRichText.Append(">");
	    }
	    else
	    {
		    sbRichText.Append("</");
		    sbRichText.Append(textParsingInfo.TagKey);
		    sbRichText.Append(">");
	    }
    }
    
    private void BuildCustomTag(StringBuilder sbRichText, StringBuilder sbShown, List<TextParsingInfo> textParsingInfoList, int textParsingInfoListIndex)
    {
	    var textParsingInfo = textParsingInfoList[textParsingInfoListIndex];
	    if (textParsingInfo.IsStartTag)
	    {
		    switch (textParsingInfo.TagKey)
	        {
		        case GTextParser.CustomTagEmoji:
		        case GTextParser.CustomTagEmojiShort:
		        {
			        if (Application.isPlaying)
			        {
				        if (EmojiRects.ContainsKey(textParsingInfo.TagValue).IsFalse())
				        {
					        Debug.LogError($"Not Found Emoji Key => {textParsingInfo.TagValue}");
				        }
				        else
				        {
					        var len = (sbShown.Length - GetWhiteSpaceCountFromStringBuilder(sbShown));
					        _emojiReplacements.Add(new PosStringTuple(len, textParsingInfo.TagValue));   
				        }
			        }
			        
			        sbShown.Append(EmSpace);
			        sbRichText.Append(EmSpace);
		        }
			        break;
		        case GTextParser.CustomTagTooltip:
		        case GTextParser.CustomTagTooltipShort:
		        case GTextParser.CustomTagTooltipNoUnderline:
		        case GTextParser.CustomTagTooltipNoUnderlineShort:
	            case GTextParser.CustomTagHyperLink:
	            case GTextParser.CustomTagHyperLinkShort:
		        case GTextParser.CustomTagHyperLinkNoUnderline:
		        case GTextParser.CustomTagHyperLinkNoUnderlineShort:
	            case GTextParser.CustomTagUnderLine:
	            case GTextParser.CustomTagUnderLineShort:
		        case GTextParser.CustomTagUnderLineColor:
		        case GTextParser.CustomTagUnderLineColorShort:
	            {
	                var useHyperLink = GTextParser.UseHyperLink(textParsingInfo.TagKey);
	                var useUnderLine = GTextParser.UseUnderLine(textParsingInfo.TagKey);
	                var useFontColor = GTextParser.UseFontColor(textParsingInfo.TagKey);

	                var tooltipColorString = "#f2653a";
	                var colorString = textParsingInfo.TagKey.Equals("tooltip") ? tooltipColorString : "blue";
	                if (useFontColor)
	                {
		                sbRichText.Append("<color=");
		                sbRichText.Append(colorString);
		                sbRichText.Append(">");
	                }
	                    
	                var href = new HrefInfo();
	                href.TagKey = textParsingInfo.TagKey;
	                href.Show = true;
	                href.StartIndex = (sbShown.Length) * 4;
	                if (TryGetNextPlainText(textParsingInfoList, textParsingInfoListIndex, out var plainText))
	                {
		                href.Text = plainText;
	                }

	                {
		                href.StartIndexWithoutSpace = (sbShown.Length - GetWhiteSpaceCountFromStringBuilder(sbShown)) * 4;
	                }
	                
	                href.Url = textParsingInfo.TagValue;
	                if (useFontColor)
	                {
	                    if (false == ColorUtility.TryParseHtmlString(colorString, out var outColor).IsFalse())
	                    {
	                        href.ColorValue = outColor;
	                    }
	                }
	                else
	                {
	                    href.ColorValue = color;
	                }

	                if (GTextParser.CustomTagUnderLineColor == textParsingInfo.TagKey || 
	                    GTextParser.CustomTagUnderLineColorShort == textParsingInfo.TagKey)
	                {
		                var index = _hrefs.FindIndex(it => it.StartIndex == href.StartIndex
		                                  && it.StartIndexWithoutSpace == href.StartIndexWithoutSpace
		                                  && it.Text == href.Text);
		                if (0 <= index)
		                {
			                if (false == ColorUtility.TryParseHtmlString(textParsingInfo.TagValue, out var outColor).IsFalse())
			                {
				                _hrefs[index].ColorValue = outColor;
			                }
		                }
	                }
	                else
	                {
		                if (useHyperLink)
		                {
			                _hrefs.Add(href);   
		                }

		                if (useUnderLine)
		                {
			                _underlines.Add(href);   
		                }
	                }
	                break;
	            }
	        }
	    }
	    else
	    {
		    var useFontColor = GTextParser.UseHyperLink(textParsingInfo.TagKey);
		    if (useFontColor)
		    {
			    sbRichText.Append("</color>");
		    }
	    }
    }

    private int GetLineNo(int index)
    {
	    var lineNo = -1;
	    var lines = cachedTextGenerator.lines;
	    foreach (var line in lines)
	    {
		    if (line.startCharIdx * 4 <= index)
		    {
			    lineNo++;
		    }
	    }

	    return lineNo;
    }

    public static bool CompareRGB(Color32 a, Color32 b)
    {
	    return a.r == b.r && a.g == b.g && a.b == b.b;
    } 

    // Truncate 예외 처리를 위한 정보 수집
    private void ParsingUIVertexIndex(VertexHelper vertexHelper)
    {
	    _uiVertexIndexList.Clear();
	    
	    if (_filedInfo.IsNull(false))
	    {
		    _filedInfo = typeof(VertexHelper).GetField("m_Colors", 
			    BindingFlags.NonPublic | 
			    BindingFlags.Instance);
	    }

	    if (_filedInfo.IsNull())
	    {
		    return;
	    }
	    
	    // 컬러 값을 기준으로 필요한 문자열의 위치를 추출해 낸다
	    var colors = (List<Color32>)_filedInfo.GetValue(vertexHelper);
	    if (colors.IsNull())
	    {
		    return;
	    }

	    var startIndex = 0;
	    foreach (var href in _hrefs)
	    {
		    var colorRGB = href.ColorValue;
		    var textLen = href.Text.Length * 4;
		    var success = true;
		    for (var curIndex = startIndex; curIndex < colors.Count; curIndex++)
		    {
			    success = true;
			    for (var textIndex = 0; textIndex < href.Text.Length; textIndex++)
			    {
				    var ch = href.Text[textIndex];
				    var isSpace = ch == ' ';
				    if (isSpace)
				    {
					    continue;
				    }
				    
				    for (var j = 0; j < 4; j++)
				    {
					    if (colors.Count <= curIndex + (textIndex * 4) + j)
					    {
						    break;
					    }
					    
					    if (false == CompareRGB(colors[curIndex + (textIndex * 4) + j], colorRGB))
					    {
						    success = false;
						    break;
					    }
				    }

				    if (false == success)
				    {
					    break;
				    }
			    }

			    if (success)
			    {
				    if (curIndex + href.Text.Length * 4 < colors.Count)
				    {
					    for (var j = 0; j < 4; j++)
					    {
						    if (CompareRGB(colors[curIndex + (href.Text.Length * 4) + j], colorRGB))
						    {
							    success = false;
							    break;
						    }   
					    }
				    }
				    
				    if (success)
				    {
					    _uiVertexIndexList.Add(curIndex);
					    startIndex = curIndex + textLen;
					    break;
				    }
			    }
		    }

		    if (success.IsFalse(false))
		    {
			    return;
		    }
	    }
    }

    // 박스 크기 계산
    private void ComputeBoundsInfo(VertexHelper toFill)
    {
	    if (_hrefs.Count <= 0)
	    {
		    return;
	    }
	    
	    Profiler.BeginSample("GText.ComputeBoundsInfo");

	    var extents = RectTransform.rect.size;
	    var settings = GetGenerationSettings(extents);
	    cachedTextGenerator.Populate(_builderShown.ToString(), settings);

	    var isTruncate = horizontalOverflow == HorizontalWrapMode.Wrap 
	                     && verticalOverflow == VerticalWrapMode.Truncate 
	                     && cachedTextGenerator.characterCountVisible != _builderShown.Length;
	    if (isTruncate)
	    {
		    ParsingUIVertexIndex(toFill);
	    }

        var vert = new UIVertex();
        for (var index = 0; index < _hrefs.Count; index++)
        {
	        var href = _hrefs[index];
	        href.BoxList.Clear();

	        var startIndex = href.StartIndex;
	        var startIndexWithoutSpace = href.StartIndexWithoutSpace;
	        if (isTruncate)
	        {
		        if (_uiVertexIndexList.Count != 0)
		        {
			        if (index >= _uiVertexIndexList.Count)
			        {
				        continue;
			        }

			        startIndexWithoutSpace = _uiVertexIndexList[index];
		        }
	        }

	        if (startIndexWithoutSpace >= toFill.currentVertCount)
		        continue;

	        // Add hyper text vector index to bounds
	        toFill.PopulateUIVertex(ref vert, startIndexWithoutSpace);

	        var line = GetLineNo(startIndex);
	        var pos = vert.position;
	        var bounds = new Bounds(pos, Vector3.zero);
	        foreach (var ch in href.Text)
	        {
		        var isSpace = ch == ' ';
		        if (false == isSpace || isTruncate)
		        {
			        for (var i = 0; i < 4; i++)
			        {
				        if (startIndexWithoutSpace >= toFill.currentVertCount) break;

				        toFill.PopulateUIVertex(ref vert, startIndexWithoutSpace);
				        pos = vert.position;

				        if (false == isSpace)
				        {
					        if (line + 1 < cachedTextGenerator.lineCount
					            && cachedTextGenerator.lines[line + 1].startCharIdx * 4 <= startIndex)
					        {
						        //if in different lines
						        href.BoxList.Add(new Rect(bounds.min, bounds.size));
						        bounds = new Bounds(pos, Vector3.zero);

						        line += 1;
					        }
					        else
					        {
						        bounds.Encapsulate(pos); //expand bounds
					        }
				        }

				        startIndexWithoutSpace++;
			        }
		        }

		        startIndex += 4;
	        }

	        //add bound
	        href.BoxList.Add(new Rect(bounds.min, bounds.size));
        }

        Profiler.EndSample();
    }

    #region Debug

    private void OnGUI()
    {
	    foreach (var underline in _hrefs)
	    {
		    foreach (var rect in underline.BoxList)
		    {
			    RenderRectTest(rect, Color.red);
		    }
	    }
    }

    private void RenderRectTest(Rect rect, Color drawColor)
    {
	    var lb = RectTransform.TransformPoint(new Vector3(rect.xMin, rect.yMin));
	    var lt = RectTransform.TransformPoint(new Vector3(rect.xMin, rect.yMax));
	    var rb = RectTransform.TransformPoint(new Vector3(rect.xMax, rect.yMin));
	    var rt = RectTransform.TransformPoint(new Vector3(rect.xMax, rect.yMax));
	    Debug.DrawLine(lb, rb, drawColor, 0.1f);
	    Debug.DrawLine(rb, rt, drawColor, 0.1f);
	    Debug.DrawLine(rt, lt, drawColor, 0.1f);
	    Debug.DrawLine(lt, lb, drawColor, 0.1f);
    }
    #endregion

    private static bool TryGetComponentInRecursiveParent<T>(GameObject goParent, out T comp) where T : Component
    {
	    while (null != goParent)
	    {
		    if (goParent.TryGetComponent(out comp))
		    {
			    return true;
		    }
		    
		    if (null != goParent.transform
		             && null != goParent.transform.parent
		             && null != goParent.transform.parent.gameObject)
		    {
			    goParent = goParent.transform.parent.gameObject;
		    }
		    else
		    {
			    break;
		    }
	    }

	    comp = null;
	    return false;
    }
    
    private float GetScale(int width, int height, CanvasScaler canvasScaler)
    {
	    var scalerReferenceResolution = canvasScaler.referenceResolution;
	    var widthScale = width / scalerReferenceResolution.x;
	    var heightScale = height / scalerReferenceResolution.y;
   
	    switch (canvasScaler.screenMatchMode)
	    {
		    case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
			    var matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
 
			    return Mathf.Pow(widthScale, 1f - matchWidthOrHeight)*
			           Mathf.Pow(heightScale, matchWidthOrHeight);
     
		    case CanvasScaler.ScreenMatchMode.Expand:
			    return Mathf.Min(widthScale, heightScale);
     
		    case CanvasScaler.ScreenMatchMode.Shrink:
			    return Mathf.Max(widthScale, heightScale);
     
		    default:
			    throw new ArgumentOutOfRangeException();
	    }
   
    }
    
    // 언더라인 드로우
    private void DrawUnderLine(VertexHelper toFill)
    {
        if(_underlines.Count <= 0)
            return;

        Profiler.BeginSample("GText.DrawUnderLine");
        var extents = RectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate("_", settings);
        var uList = cachedTextGenerator.verts;
        var heightScale = 0.7f;
        var h = uList[1].position.y - uList[2].position.y;
        h *= heightScale;
        
        if (TryGetComponentInRecursiveParent<CanvasScaler>(gameObject, out var canvasScaler))
        {
	        var screenWidth = Screen.width;
	        var screenHeight = Screen.height;
#if UNITY_EDITOR
	        screenWidth = Camera.main.pixelWidth;
	        screenHeight = Camera.main.pixelHeight;
#endif
	        
	        var canvasScale = GetScale(screenWidth, screenHeight, canvasScaler);
	        h /= canvasScale;
        }
        
        var tempVector3Array = new Vector3[4];

        var widthRate = 0.0f;

        var midX = uList.Average(it => it.uv0.x);
        var midY = uList.Average(it => it.uv0.y);

        List<UIVertex> uListTemp = new();
        for (var index = 0; index < uList.Count; index++)
        {
	        var it = uList[index];
	        it.uv0 = new(midX - (index / 10000.0f), midY + (index / 10000.0f), 0 , 0);
	        uListTemp.Add(it);
        }

        foreach (var info in _underlines)
        {
	        if(!info.Show)
		        continue;

	        foreach (var rect in info.BoxList)
	        {
		        if (rect.width <= 0 || rect.height <= 0)
			        continue;

		        tempVector3Array[0] = rect.min;
		        tempVector3Array[0].x -= rect.width * widthRate;
		        tempVector3Array[1] = tempVector3Array[0] + new Vector3(rect.width * (2 * widthRate + 1.0f), 0);
		        tempVector3Array[2] = tempVector3Array[0] + new Vector3(rect.width * (2 * widthRate + 1.0f), -h);
		        tempVector3Array[3] = tempVector3Array[0] + new Vector3(0, -h);

		        for (var k = 0; k < 4; k++)
		        {
			        _tempVerts[k] = uListTemp[k];
			        _tempVerts[k].color = info.ColorValue;
			        _tempVerts[k].position = tempVector3Array[k];
		        }

		        toFill.AddUIVertexQuad(_tempVerts);
	        }
        }
        
        Profiler.EndSample();
    }

    // 유니티 클릭 이벤트
    public void OnPointerClick(PointerEventData eventData)
    {
	    /*Debug.Log("GText.OnPointerClick");*/
	    
	    if (_hrefs.Count <= 0)
	    {
		    return;
	    }
	    
	    // 이유는 모르지만 박스를 파싱하지 못했을 때 재시도
	    if (_hrefs[0].BoxList.Count <= 0)
	    {
		    UpdateGeometry();
	    }

	    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
		        RectTransform, eventData.position, eventData.pressEventCamera, out var lp).IsFalse(false))
	    {
		    if (_uiCamera.IsNull() || RectTransformUtility.ScreenPointToLocalPointInRectangle(
			        RectTransform, eventData.position, _uiCamera, out lp).IsFalse())
		    {
			    Debug.LogError("GText.ScreenPointToLocalPointInRectangle Failed");
			    return;
		    }
	    }

	    foreach (var hrefInfo in _hrefs)
	    {
		    var boxes = hrefInfo.BoxList;
		    for (var i = 0; i < boxes.Count; ++i)
		    {
			    if (boxes[i].Contains(lp))
			    {
				    if (enableClick)
				    {
					    Debug.Log($"GText.hrefClick => {hrefInfo.Url}");
					    HrefClick.Invoke(this, hrefInfo, boxes[i]);
				    }
			        
				    return;
			    }
		    }
	    }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
	    /*Debug.Log("GText.OnPointerMove");*/
	    
	    if (_hrefs.Count <= 0)
	    {
		    return;
	    }
	    
	    // 이유는 모르지만 박스를 파싱하지 못했을 때 재시도
	    if (_hrefs[0].BoxList.Count <= 0)
	    {
		    UpdateGeometry();
	    }
	    
	    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
		        RectTransform, eventData.position, eventData.pressEventCamera, out var lp).IsFalse(false))
	    {
		    if (_uiCamera.IsNull() || RectTransformUtility.ScreenPointToLocalPointInRectangle(
			        RectTransform, eventData.position, _uiCamera, out lp).IsFalse())
		    {
			    Debug.LogError("GText.ScreenPointToLocalPointInRectangle Failed");
			    return;
		    }
	    }

	    foreach (var hrefInfo in _hrefs)
	    {
		    var boxes = hrefInfo.BoxList;
		    var oneMoreHovered = false;
		    for (var i = 0; i < boxes.Count; ++i)
		    {
			    if (boxes[i].Contains(lp))
			    {
				    oneMoreHovered = true;
				    if (false == hrefInfo.Hovered)
				    {
					    hrefInfo.Hovered = true;
					    if (enableHover)
					    {
						    Debug.Log($"GText.hrefEnter => {hrefInfo.Url}");
						    HrefHoverEnter.Invoke(this, hrefInfo, boxes[i]);
					    }
					    
					    return;
				    }
			    }
		    }

		    if (oneMoreHovered)
		    {
			    return;
		    }
		    
		    for (var i = 0; i < boxes.Count; ++i)
		    {
			    if (hrefInfo.Hovered && false == boxes[i].Contains(lp))
			    {
				    hrefInfo.Hovered = false;
				    if (enableHover)
				    {
					    Debug.Log($"GText.hrefExit => {hrefInfo.Url}");
					    HrefHoverExit.Invoke(this, hrefInfo, boxes[i]);
				    }
				    
				    return;
			    }
		    }
	    }
    }
}
