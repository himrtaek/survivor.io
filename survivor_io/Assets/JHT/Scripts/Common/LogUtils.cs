using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Text;
using JHT.Scripts.Common.PerformanceExtension;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace JHT.Scripts.Common
{
	public class LogUtil
	{
		public const string TabString = "    ";
		
		public static string UnindentString(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				return string.Empty;
			}

			var tabString = TabString;
			if (source.StartsWith(tabString))
			{
				source = source.Substring(tabString.Length);
			}
			return source.Replace($"\n{tabString}", "\n");
		}
		
		private static readonly Dictionary<string, string> ColorByFilter = new()
		{
			
		};

		public static string GetColor(string sMessage)
		{
			foreach (var it in ColorByFilter)
			{
				if (0 == sMessage.IndexOf(it.Key, StringComparison.Ordinal))
				{
					return it.Value;
				}
			}

			return string.Empty;
		}

		public static string CheckColor(string sMessage)
		{
			foreach (var it in ColorByFilter)
			{
				if (0 == sMessage.IndexOf(it.Key, StringComparison.Ordinal))
				{
					return SetColor(it.Value, sMessage);
				}
			}

			return sMessage;
		}

		public static string SetColor(string sColor, string sMessage)
		{
			_stringBuilder.AppendFormat("<color={0}>", sColor);

			var iLastIndex = GetSmallestNonNegative(sMessage.IndexOf("\r\n", StringComparison.Ordinal),
				sMessage.IndexOf("\r", StringComparison.Ordinal));
			iLastIndex = GetSmallestNonNegative(iLastIndex, sMessage.IndexOf("\n", StringComparison.Ordinal));

			if (iLastIndex < 0)
			{
				_stringBuilder.Append(sMessage);
				_stringBuilder.Append("</color>");
			}
			else
			{
				_stringBuilder.Append(sMessage.Insert(iLastIndex, "</color>"));
			}

			var ret = _stringBuilder.ToString();
			_stringBuilder.Clear();
			return ret;
		}
	
		private static int GetSmallestNonNegative(int a , int b)
		{
			if (a >= 0 && b >= 0)
				return Math.Min(a,b);
			if (a >= 0 && b < 0)
				return a;
			if (a < 0 && b >= 0)
				return b;
			return -1;
		}
	
		public static bool IsLargeLog(string str)
		{
			if (14000 <= str.Length)
			{
				return true;
			}

			return false;
		}

		private static Utf16ValueStringBuilder _stringBuilder = ZString.CreateStringBuilder();
		
		public enum LogLevel
		{
			Info,
			Warning,
			Error
		}
		
		public static string Log(object myObj, LogLevel logLevel = LogLevel.Info, bool showLog = true, string color = "", string name = null, int iDepth = 0)
		{
			if (10 < iDepth)
			{
				return "**more than 10 Depth **";
			}

			var sDepth = "";
			for (var i = 0; i <= iDepth; ++i)
			{
				sDepth = ZString.Concat(sDepth, TabString);
			}
			
			var sStr = "";
			if (null == myObj)
			{
				sStr = ZString.Concat(sStr, sDepth, name, " : null\n");
				return sStr;
			}
			
			if (0 == iDepth)
			{
				sStr = ZString.Concat(sStr,  "{ObjectType : ",  myObj.GetType().Name,  "}\n");
				sStr = ZString.Concat(sStr,  "{\n");
			}

			var myObjType = myObj.GetType();
			if (LogUtil.IsLargeLog(sStr))
			{
				// 아무것도 하지 않음
			}
			else if (myObj is IEnumerable enumerable && myObjType != typeof(string))
			{
				sStr = ZString.Concat(sStr, sDepth, name ?? myObj.GetType().FullName, " : [\n");

				var iCount = 0;
				foreach (var it in enumerable)
				{
					if (LogUtil.IsLargeLog(sStr))
					{
						break;
					}
								
					sStr = ZString.Concat(sStr, sDepth, TabString, iCount++.ToStringCached(), " =>\n");
					sStr = ZString.Concat(sStr, Log(it, logLevel, false, "", name, iDepth + 2));
				}

				sStr = ZString.Concat(sStr, sDepth, "]\n");
			}
			else if (myObj is Vector3 vector3MyObj)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : {1}\n", name ?? myObj.GetType().FullName, vector3MyObj));
			}
			else if (myObj is Quaternion quaternionMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : {1}({2})\n", name ?? myObj.GetType().FullName, quaternionMyObj, quaternionMyObj.eulerAngles));
			}
			else if (myObj is GameObject gameObjectMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : {1}({2})\n",name ?? myObj.GetType().FullName, gameObjectMyObj.name, gameObjectMyObj.GetHashCode()));
			}
			else if (myObj is UnityEngine.Object unityObject)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : {1}\n", name ?? myObj.GetType().FullName, unityObject));
			}
			else if (myObj.GetType().IsEnum)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : {1}({2})\n", name ?? myObj.GetType().FullName, myObj, Convert.ToInt32(myObj)));
			}
			else if (myObj is decimal decimalMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : {1}\n", name ?? myObj.GetType().FullName, decimalMyObj));
			}
			else if (myObj is string stringMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : \"{1}\"\n", name ?? myObj.GetType().FullName, stringMyObj));
			}
			else if (myObj.GetType().IsPrimitive)
			{
				sStr = ZString.Concat(sStr, sDepth, ZString.Format("{0} : {1}\n", name ?? myObj.GetType().FullName, myObj));
			}
			else
			{
				bool atLeastOne = false;
				foreach (var member in myObj.GetType().GetMembers())
				{
					if (LogUtil.IsLargeLog(sStr)) break;

					try
					{
						if (member is not PropertyInfo && member is not FieldInfo)
						{
							continue;
						}

						atLeastOne = true;
						var prop = member as PropertyInfo;
						var field = member as FieldInfo;
						var obj = prop != null ? prop.GetValue(myObj) : field.GetValue(myObj);

						var subStr = Log(obj, logLevel, false, "", member.Name, iDepth + 1);
						if (subStr.Substring(sDepth.Length).StartsWith(ZString.Concat(TabString, member.Name, " : ")))
						{
							sStr = ZString.Concat(sStr, UnindentString(subStr));
						}
						else
						{
							sStr = ZString.Concat(sStr, sDepth, member.Name, " : {\n");
							sStr = ZString.Concat(sStr, subStr);
							sStr = ZString.Concat(sStr, sDepth, "}\n");
						}
					}
					catch (Exception e)
					{
						if (e.InnerException?.GetType() == typeof(NotSupportedException))
						{
							sStr = ZString.Concat(sStr, sDepth, member.Name, " : NotSupportedException", "\n");
						}
						else
						{
							Debug.LogException(e);
						}
					}
				}

				if (false == atLeastOne)
				{
					sStr = ZString.Concat(sStr, sDepth, name ?? myObj.GetType().FullName, " : {}\n");
				}
			}

			if (0 != iDepth)
			{
				return sStr;
			}
			
			sStr = ZString.Concat(sStr,  "}");
			return Log(sStr, logLevel, showLog, color);
		}
		
		public class EnumConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (value.GetType().IsEnum)
				{
					var enumName = Enum.GetName(value.GetType(), value);
					var enumValue = Convert.ToInt32(value);

					writer.WriteValue($"{enumName}({enumValue})");
				}
				else
				{
					throw new JsonSerializationException("Expected enum type.");
				}
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				throw new NotImplementedException("Deserialization is not implemented.");
			}

			public override bool CanConvert(Type objectType)
			{
				return objectType.IsEnum;
			}
		}
		
		public class EnumerableConverter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (value is IEnumerable enumerable)
				{
					writer.WriteStartArray();
					var index = 0;
					foreach (var it in enumerable)
					{
						writer.WriteStartObject();
						writer.WritePropertyName(index++.ToString());
						serializer.Serialize(writer, it);
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
				}
				else
				{
					throw new JsonSerializationException("Expected enum type.");
				}
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				throw new NotImplementedException("Deserialization is not implemented.");
			}

			public override bool CanConvert(Type objectType)
			{
				return typeof(IEnumerable).IsAssignableFrom(objectType) && objectType != typeof(string);
			}
		}
		
		public class IgnorePropertiesResolver : DefaultContractResolver
		{
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);

				// 무시할 프로퍼티 설정
				if (typeof(GameObject).IsAssignableFrom(property.DeclaringType) || typeof(Component).IsAssignableFrom(property.DeclaringType))
				{
					switch (property.PropertyName)
					{
						case "animation":
						case "audio":
						case "camera":
						case "collider":
						case "collider2D":
						case "constantForce":
						case "guiElement":
						case "guiText":
						case "guiTexture":
						case "hingeJoint":
						case "light":
						case "networkView":
						case "particleSystem":
						case "renderer":
						case "rigidbody":
						case "rigidbody2D":
							property.ShouldSerialize = instance => false;
							break;
					}
				}

				return property;
			}
		}
		
		public static string LogWithJson(object myObj, LogLevel logLevel = LogLevel.Info, bool showLog = true, bool bShowObjectType = true, string color = "", bool setColor = true)
		{
			var sStr = "";
			if (null == myObj)
			{
				return sStr;
			}
			
			if (bShowObjectType)
			{
				sStr = ZString.Concat(sStr,  "{ObjectType : ",  myObj.GetType().Name,  "}\n");
			}

			var serializeObjectString = "";
			try
			{
				serializeObjectString = JsonConvert.SerializeObject(myObj,
					new JsonSerializerSettings
					{
						Converters = { new EnumConverter(), new EnumerableConverter() },
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
						ContractResolver = new IgnorePropertiesResolver(),
						Formatting = Formatting.Indented
					});
			}
			catch (Exception e)
			{
				serializeObjectString = ZString.Concat("Serialize Error : ", e.Message);
				Debug.LogException(e);
			}
			
			sStr = ZString.Concat(sStr, serializeObjectString);
			
			return Log(sStr, logLevel, showLog, color);
		}

		private static string Log(string sStr, LogLevel logLevel, bool showLog, string color)
		{
			var sRetFinal = sStr;
			if (LogUtil.IsLargeLog(sStr))
			{
				sRetFinal = ZString.Concat("**Message too long** ", sRetFinal);
			}

			color = color != "" ? color : LogUtil.GetColor(sStr);
			if (false == string.IsNullOrEmpty(color))
			{
				sRetFinal = LogUtil.SetColor(color, sRetFinal);
			}

			if (showLog)
			{
				switch (logLevel)
				{
					case LogLevel.Info:
						Debug.Log(sRetFinal);
						break;
					case LogLevel.Warning:
						Debug.LogWarning(sRetFinal);
						break;
					case LogLevel.Error:
						Debug.LogError(sRetFinal);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
				}
			}

			return sRetFinal;
		}
	}
}
