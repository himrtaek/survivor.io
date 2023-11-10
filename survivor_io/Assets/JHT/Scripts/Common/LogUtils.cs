using System;
using System.Collections;
using Cysharp.Text;
using JHT.Scripts.Common.PerformanceExtension;
using Unity.VisualScripting;
using UnityEngine;

namespace JHT.Scripts.Common
{
	[IncludeInSettings(true)] public class LogUtils
	{
		private static bool IsLargeLog(string str)
		{
			if (14000 <= str.Length)
			{
				return true;
			}

			return false;
		}

		public enum LogLevel
		{
			Info,
			Warning,
			Error
		}
		
		public static string PrintPropertiesAndFields(object myObj, LogLevel logLevel = LogLevel.Info, string sStart = "", string sEnd = "", bool bShowLog = true, bool bShowObjectType = true, int iDepth = 0, string color = "", bool setColor = true)
		{
			if (10 < iDepth)
			{
				return "**more than 10 Depth **";
			}
			
			if (bShowLog)
			{
				return string.Empty;
			}
			
			var sStr = "";
			if (null == myObj)
			{
				return sStr;
			}
			if (bShowObjectType)
			{
				sStr = ZString.Concat(sStr,  "{ObjectType : ",  myObj.GetType().ToString(),  "}\n");
			}

			var sDepth = "";
			for (var i = 0; i < iDepth; ++i)
			{
				sDepth = ZString.Concat(sDepth, "    ");
			}

			if (myObj.GetType() != typeof(String) && myObj.GetType() != typeof(Vector3) && myObj.GetType() != typeof(Quaternion) && myObj.GetType() != typeof(GameObject)
			    && false == typeof(UnityEngine.Object).IsAssignableFrom(myObj))
			{
				foreach (var prop in myObj.GetType().GetProperties())
				{
					if (IsLargeLog(sStr))
					{
						break;
					}
					
					try
					{
						var obj = prop.GetValue(myObj, null);
#if !USE_UNITY_TEST
						if (prop.PropertyType.IsStruct() == false &&
						    null == obj)
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, prop.Name, " : null\n");
						}
						else if (prop.PropertyType.IsGenericType)
						{
							if (typeof(IEnumerable).IsAssignableFrom(obj))
							{
								sStr = ZString.Concat(sStr, sDepth);
								sStr = ZString.Concat(sStr, prop.Name, " : [\n");

								var iCount = 0;
								foreach (var it in obj as IEnumerable)
								{
									if (IsLargeLog(sStr))
									{
										break;
									}
								
									sStr = ZString.Concat(sStr, sDepth, "    ", iCount++.ToStringCached(), "=>\n");
									sStr = ZString.Concat(sStr, PrintPropertiesAndFields(it, logLevel, "", "", false, false, iDepth + 1, setColor: false));
								}

								sStr = ZString.Concat(sStr, sDepth);
								sStr = ZString.Concat(sStr, "]\n");
							}
							else
							{
								sStr = ZString.Concat(sStr, sDepth);
								sStr = ZString.Concat(sStr, prop.Name, " : ", obj, "\n");
							}
						}
						else if ((prop.PropertyType.IsClass || prop.PropertyType.IsStruct()) &&
						         prop.PropertyType != typeof(string))
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, prop.Name, " : {\n");

							sStr = ZString.Concat(sStr, PrintPropertiesAndFields(obj, logLevel, "", "", false, false, iDepth + 1, setColor: false));

							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, "}\n");
						}
						else if (prop.PropertyType.IsEnum)
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, ZString.Format("{0} : {1}({2})\n", (object)prop.Name, obj, Convert.ToInt32(obj)));
							
						}
						else
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, prop.Name, " : ", obj, "\n");
						}
#endif
					}
					catch (Exception e)
					{
						if (null != e.InnerException && e.InnerException.GetType() == typeof(NotSupportedException))
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, prop.Name, " : NotSupportedException", "\n");
						}
						else
						{
							Debug.LogException(e);	
						}
					}
				}
			}

			if (IsLargeLog(sStr))
			{
				// 아무것도 하지 않음
			}
			else if (myObj is String stringMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("String : {0}\n", stringMyObj));
			}
			else if (myObj is Vector3 vector3MyObj)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("Vector3 : {0}\n", vector3MyObj));
			}
			else if (myObj is Quaternion quaternionMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("Quaternion : {0}\n", quaternionMyObj));
			}
			else if (myObj is GameObject gameObjectMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("GameObject : {0}({1})\n", gameObjectMyObj.name, gameObjectMyObj.GetHashCode()));
			}
			else if (typeof(UnityEngine.Object).IsAssignableFrom(myObj))
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("{0} : {1}\n", myObj.GetType().FullName, myObj));
			}
			else if (myObj is bool boolMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("Boolean : {0}\n", boolMyObj));
			}
			else if (myObj is int intMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("Int32 : {0}\n", intMyObj));
			}
			else if (myObj is long longMyObj)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("long : {0}\n", longMyObj));
			}
			else if (myObj.GetType().IsEnum)
			{
				sStr = ZString.Concat(sStr, sDepth);
				sStr = ZString.Concat(sStr, ZString.Format("{0} : {1}({2})\n", myObj.GetType(), myObj, (int) myObj));
			}
			else
			{
				foreach (var field in myObj.GetType().GetFields())
				{
					if (IsLargeLog(sStr))
					{
						break;
					}

					try
					{
						var obj = field.GetValue(myObj);
#if !USE_UNITY_TEST
						if (field.FieldType.IsStruct() == false &&
						    null == obj)
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, field.Name, " : null\n");
						}
						else if (field.FieldType.IsGenericType)
						{
							if (typeof(IEnumerable).IsAssignableFrom(obj))
							{
								sStr = ZString.Concat(sStr, sDepth);
								sStr = ZString.Concat(sStr, field.Name, " : [\n");

								var iCount = 0;
								foreach (var it in obj as IEnumerable)
								{
									if (IsLargeLog(sStr))
									{
										break;
									}

									sStr = ZString.Concat(sStr, sDepth, "    ", iCount++.ToStringCached(), "=>\n");
									sStr = ZString.Concat(sStr, PrintPropertiesAndFields(it, logLevel, "", "", false, false, iDepth + 1, setColor: false));
								}

								sStr = ZString.Concat(sStr, sDepth);
								sStr = ZString.Concat(sStr, "]\n");
							}
							else
							{
								sStr = ZString.Concat(sStr, sDepth);
								sStr = ZString.Concat(sStr, field.Name, " : ", obj, "\n");
							}
						}
						else if ((field.FieldType.IsClass || field.FieldType.IsStruct()) &&
						         field.FieldType != typeof(string))
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, field.Name, " : {\n");

							sStr = ZString.Concat(sStr, PrintPropertiesAndFields(obj, logLevel, "", "", false, false, iDepth + 1, setColor: false));

							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, "}\n");
						}
						else
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, field.Name, " : ", obj, "\n");
						}
#endif
					}
					catch (Exception e)
					{
						if (null != e.InnerException && e.InnerException.GetType() == typeof(NotSupportedException))
						{
							sStr = ZString.Concat(sStr, sDepth);
							sStr = ZString.Concat(sStr, field.Name, " : NotSupportedException", "\n");
						}
						else
						{
							Debug.LogException(e);	
						}
					}
				}
			}
			
			var sRetLocal = ZString.Concat(sStart, sStr, sEnd);
			var sRetFinal = sRetLocal;
			if (0 == iDepth)
			{
				if (IsLargeLog(sRetLocal))
				{
					sRetFinal = ZString.Concat("**Message too long** ", sRetFinal);	
				}
			}

			if (setColor)
			{
				var sColor = color != "" ? color : CheckColor(sRetLocal);
				if (false == string.IsNullOrEmpty(sColor))
				{
					sRetFinal = SetColor(sColor, sRetFinal);
				}
			}

			if (bShowLog)
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

		private static string CheckColor(string sMessage)
		{
			if (0 == sMessage.IndexOf("{ObjectType : CG_", StringComparison.Ordinal)
			    || 0 == sMessage.IndexOf("{ObjectType : CM_", StringComparison.Ordinal))
			{
				return "orange";
			}

			if (0 == sMessage.IndexOf("{ObjectType : GC_", StringComparison.Ordinal)
			    || 0 == sMessage.IndexOf("{ObjectType : MC_", StringComparison.Ordinal)
			    || 0 == sMessage.IndexOf("{ObjectType : AA_", StringComparison.Ordinal))
			{
				return "green";
			}

			return "";
		}
		
		private static int GetSmallestNonNegative(int a , int b)
		{
			if (a >= 0 && b >= 0)
				return Math.Min(a,b);
			else if (a >= 0 && b < 0)
				return a;
			else if (a < 0 && b >= 0)
				return b;
			else
				return -1;
		}

		private static Utf16ValueStringBuilder _zStringBuilder = ZString.CreateStringBuilder();

		private static string SetColor(string sColor, string sMessage)
		{
			_zStringBuilder.AppendFormat("<color={0}>", sColor);

			var iLastIndex = GetSmallestNonNegative(sMessage.IndexOf("\r\n", StringComparison.Ordinal),
				sMessage.IndexOf("\r", StringComparison.Ordinal));
			iLastIndex = GetSmallestNonNegative(iLastIndex, sMessage.IndexOf("\n", StringComparison.Ordinal));

			if (iLastIndex < 0)
			{
				_zStringBuilder.Append(sMessage);
				_zStringBuilder.Append("</color>");
			}
			else
			{
				_zStringBuilder.Append(sMessage.Insert(iLastIndex, "</color>"));
			}

			var ret = _zStringBuilder.ToString();
			_zStringBuilder.Clear();
			return ret;
		}
	}
}
