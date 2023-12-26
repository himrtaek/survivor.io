using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Text;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SK
{
	public class SKObjectIDExporter : EditorWindow
	{
		private static readonly string[] ExportDirectories =
		{
			"_InGameData/Resources/SK",
		};

		private bool _isCompiling;
		private bool _isShowAllField;
		private List<FieldInfo> _fieldInfoList = new();
		
		private int _selectedComponentIndex;
		private Type _selectedComponentType;
		private string[] _listOfSkComponentName = new[] { "" };
		private List<Type> _listOfSkComponentType = new(); 

		private static List<(GameObject gameObject, string message)> _errorBySKObjectData = new();
		private List<ISKObjectData> _skObjectDataNoID = new();
		private List<ISKObjectData> _skObjectDataConflictID = new();
		private SortedDictionary<uint, SortedDictionary<uint, ISKObjectData>> _skObjectsSuccessByID = new();
		private static Vector2 _scrollPosition;
		private static int[] columnWidthArray = new[] { 50, 100, 100, 300, 300, 400, 100 };

		private void OnGUI()
		{
			if (EditorApplication.isCompiling)
			{
				if (false == _isCompiling)
				{
					_isCompiling = true;
					Clear();	
				}
				
				return;
			}
			else
			{
				if (true == _isCompiling)
				{
					_isCompiling = false;
				}
			}
			
			if (GUILayout.Button("Refresh"))
			{
				Refresh();
			}

			GUI.enabled = _skObjectDataConflictID.Count <= 0 && 0 < _skObjectsSuccessByID.Count;
			if (GUILayout.Button("Export"))
			{
				Export();
			}
			GUI.enabled = true;
			
			var isShowAllField = GUILayout.Toggle(_isShowAllField, "모든 필드 표시");
			if (isShowAllField != _isShowAllField)
			{
				_fieldInfoList.Clear();
				_isShowAllField = isShowAllField;
			}
			
			EditorGUILayout.Space();
			
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			
			GUI.enabled = false;

			if (0 < _errorBySKObjectData.Count)
			{
				GUILayout.Label("에러 오브젝트 리스트");
			
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField("오브젝트", GUILayout.Width(columnWidthArray[4]));
					EditorGUILayout.TextField("오류 내용");
				}
				EditorGUILayout.EndHorizontal();

				foreach (var it in _errorBySKObjectData)
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.ObjectField(it.gameObject, typeof(GameObject), false, GUILayout.Width(columnWidthArray[4]));
						EditorGUILayout.TextField(it.message);
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else if (0 < _skObjectDataConflictID.Count)
			{
				GUILayout.Label("충돌 리스트");
			
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField("상태", GUILayout.Width(columnWidthArray[0]));
					EditorGUILayout.TextField("ID", GUILayout.Width(columnWidthArray[1]));
					EditorGUILayout.TextField("SubKey", GUILayout.Width(columnWidthArray[2]));
					EditorGUILayout.TextField("이름", GUILayout.Width(columnWidthArray[3]));
					EditorGUILayout.TextField("오브젝트", GUILayout.Width(columnWidthArray[4]));
				}
				EditorGUILayout.EndHorizontal();

				foreach (var it in _skObjectDataConflictID)
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.TextField("충돌", GUILayout.Width(columnWidthArray[0]));
						EditorGUILayout.TextField(it.DataID.ToString(), GUILayout.Width(columnWidthArray[1]));
						EditorGUILayout.TextField(it.DataSubKey.ToString(), GUILayout.Width(columnWidthArray[2]));
						EditorGUILayout.TextField(it.DataObjectName.ToString(), GUILayout.Width(columnWidthArray[3]));
						EditorGUILayout.ObjectField(it.MyObject, typeof(GameObject), false, GUILayout.Width(columnWidthArray[4]));
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("정상 리스트");
			
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField("상태", GUILayout.Width(columnWidthArray[0]));
					EditorGUILayout.TextField("ID", GUILayout.Width(columnWidthArray[1]));
					EditorGUILayout.TextField("SubKey", GUILayout.Width(columnWidthArray[2]));
					EditorGUILayout.TextField("이름", GUILayout.Width(columnWidthArray[3]));
					EditorGUILayout.TextField("오브젝트", GUILayout.Width(columnWidthArray[4]));

					GUI.enabled = true;
					var selectedComponentIndex = EditorGUILayout.Popup(_selectedComponentIndex, _listOfSkComponentName, GUILayout.Width(columnWidthArray[5]));
					if (selectedComponentIndex != _selectedComponentIndex)
					{
						_selectedComponentIndex = selectedComponentIndex;
						_selectedComponentType = _listOfSkComponentType[selectedComponentIndex];
						_fieldInfoList.Clear();
					}

					if (GUILayout.Button("전체 복사", GUILayout.Width(columnWidthArray[6])))
					{
						var stringBuilder = ZString.CreateUtf8StringBuilder();
						foreach (var skObjectsSuccessBySubKey in _skObjectsSuccessByID)
						{
							foreach (var it in skObjectsSuccessBySubKey.Value)
							{
								var componentData = GetComponentData(it.Value.MyObject);
								stringBuilder.Append(componentData);
								stringBuilder.Append("\n");
							}
						}

						GUIUtility.systemCopyBuffer = stringBuilder.ToString();
					}
					
					GUI.enabled = false;

					DrawComponentFieldName();
				}
				EditorGUILayout.EndHorizontal();

				foreach (var skObjectsSuccessBySubKey in _skObjectsSuccessByID)
				{
					foreach (var it in skObjectsSuccessBySubKey.Value)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.TextField("정상", GUILayout.Width(columnWidthArray[0]));
							if (it.Value is SKObject skObject)
							{
								GUI.enabled = true;
								var newDataId = EditorGUILayout.TextField(it.Value.DataID.ToString(), GUILayout.Width(columnWidthArray[1]));
								if (it.Value.DataID.ToString() != newDataId)
								{
									if (uint.TryParse(newDataId, out var dataId))
									{
										skObject.ChangeSerializeDataID(dataId);
									}
								}
						
								GUI.enabled = false;
								EditorGUILayout.TextField(it.Value.DataSubKey.ToString(), GUILayout.Width(columnWidthArray[2]));
								GUI.enabled = true;
						
								var dataObjectName = EditorGUILayout.TextField(it.Value.DataObjectName.ToString(), GUILayout.Width(columnWidthArray[3]));
								if (it.Value.DataObjectName != dataObjectName)
								{
									skObject.ChangeSerializeDataObjectName(dataObjectName);
								}
								GUI.enabled = false;
							}
							else if (it.Value is SKComponentObjectChild componentObjectChild)
							{
								GUI.enabled = false;
							
								EditorGUILayout.TextField(it.Value.DataID.ToString(), GUILayout.Width(columnWidthArray[1]));
								EditorGUILayout.TextField(it.Value.DataSubKey.ToString(), GUILayout.Width(columnWidthArray[2]));
							
								GUI.enabled = true;
							
								var dataObjectName = EditorGUILayout.TextField(it.Value.DataObjectName.ToString(), GUILayout.Width(columnWidthArray[3]));
								if (it.Value.DataObjectName != dataObjectName)
								{
									componentObjectChild.ChangeSerializeDataObjectName(dataObjectName);
								}
							
								GUI.enabled = false;
							}
							
							EditorGUILayout.ObjectField(it.Value.MyObject, typeof(GameObject), false, GUILayout.Width(columnWidthArray[4]));
							var componentData = GetComponentData(it.Value.MyObject);
							EditorGUILayout.TextField(componentData, GUILayout.Width(columnWidthArray[5]));
			
							GUI.enabled = true;
							if (GUILayout.Button("Copy", GUILayout.Width(columnWidthArray[6])))
							{
								GUIUtility.systemCopyBuffer = componentData;
							}

							DrawComponentFieldData(it.Value.MyObject);
							GUI.enabled = false;
						}
						EditorGUILayout.EndHorizontal();
					}
				}

				EditorGUILayout.Space();
				
				GUILayout.Label("미할당 리스트");
			
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField("상태", GUILayout.Width(columnWidthArray[0]));
					EditorGUILayout.TextField("ID", GUILayout.Width(columnWidthArray[1]));
					EditorGUILayout.TextField("SubKey", GUILayout.Width(columnWidthArray[2]));
					EditorGUILayout.TextField("이름", GUILayout.Width(columnWidthArray[3]));
					EditorGUILayout.TextField("오브젝트", GUILayout.Width(columnWidthArray[4]));
					DrawComponentFieldName();
				}
				EditorGUILayout.EndHorizontal();
			
				foreach (var it in _skObjectDataNoID)
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.TextField("미할당", GUILayout.Width(columnWidthArray[0]));

						if (it is SKObject skObject)
						{
							GUI.enabled = true;
							var newDataId = EditorGUILayout.TextField(it.DataID.ToString(), GUILayout.Width(columnWidthArray[1]));
							if (it.DataID.ToString() != newDataId)
							{
								if (uint.TryParse(newDataId, out var dataId))
								{
									skObject.ChangeSerializeDataID(dataId);
								}
							}
						
							GUI.enabled = false;
							EditorGUILayout.TextField(it.DataSubKey.ToString(), GUILayout.Width(columnWidthArray[2]));
							GUI.enabled = true;
						
							var dataObjectName = EditorGUILayout.TextField(it.DataObjectName.ToString(), GUILayout.Width(columnWidthArray[3]));
							if (it.DataObjectName != dataObjectName)
							{
								skObject.ChangeSerializeDataObjectName(dataObjectName);
							}
							GUI.enabled = false;
						}
						else if (it is SKComponentObjectChild componentObjectChild)
						{
							GUI.enabled = false;
							
							EditorGUILayout.TextField(it.DataID.ToString(), GUILayout.Width(columnWidthArray[1]));
							EditorGUILayout.TextField(it.DataSubKey.ToString(), GUILayout.Width(columnWidthArray[2]));
							
							GUI.enabled = true;
							
							var dataObjectName = EditorGUILayout.TextField(it.DataObjectName.ToString(), GUILayout.Width(columnWidthArray[3]));
							if (it.DataObjectName != dataObjectName)
							{
								componentObjectChild.ChangeSerializeDataObjectName(dataObjectName);
							}
							
							GUI.enabled = false;
						}
						
						EditorGUILayout.ObjectField(it.MyObject, typeof(GameObject), false, GUILayout.Width(columnWidthArray[4]));
						
						GUI.enabled = true;
						DrawComponentFieldData(it.MyObject);
						GUI.enabled = false;
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			
			GUI.enabled = true;

			EditorGUILayout.EndScrollView();
		}

		private string GetComponentData(GameObject gameObject)
		{
			var componentFromDataBase = gameObject.GetComponent(_selectedComponentType) as SKComponentFromDataBase;
			if (null == componentFromDataBase)
			{
				return "";
			}
			
			return componentFromDataBase.GetFieldCSVData();
		}

		private void DrawComponentFieldName()
		{
			EditorGUILayout.TextField("Component", GUILayout.Width(100));
			
			foreach (var fieldInfo in _fieldInfoList)
			{
				EditorGUILayout.TextField(fieldInfo.Name, GUILayout.Width(fieldInfo.Name.Length * 10));
			}
		}

		private void DrawComponentFieldData(GameObject gameObject)
		{
			GUI.enabled = false;
			
			var componentFromDataBase = gameObject.GetComponent(_selectedComponentType) as SKComponentBase;
			EditorGUILayout.ObjectField(componentFromDataBase, typeof(SKComponentBase), false, GUILayout.Width(100));

			GUI.enabled = true;
			
			if (null == componentFromDataBase)
			{
				return;
			}

			var fieldInfoIsEmpty = _fieldInfoList.Count <= 0;
			foreach (var fieldInfo in componentFromDataBase.GetType().GetFields(BindingFlags.NonPublic |
				         BindingFlags.Public |
				         BindingFlags.Instance |
				         BindingFlags.DeclaredOnly))
			{
				if (fieldInfo.FieldType.IsClass)
				{
					continue;
				}
				
				var serializeField = false;
				var editableField = false;
				foreach (var customAttribute in fieldInfo.CustomAttributes)
				{
					if (customAttribute.AttributeType == typeof(SerializeField))
					{
						serializeField = true;
					}
					else if (customAttribute.AttributeType == typeof(SKEditableField))
					{
						editableField = true;
					}
				}

				if (false == _isShowAllField && false == editableField)
				{
					continue;
				}

				if (false == serializeField)
				{
					continue;
				}

				if (fieldInfoIsEmpty)
				{
					_fieldInfoList.Add(fieldInfo);
				}

				var currenttValueAsString = fieldInfo.GetValue(componentFromDataBase).ToString();
				var newValueAsString = EditorGUILayout.TextField(currenttValueAsString, GUILayout.Width(fieldInfo.Name.Length * 10));
				if (currenttValueAsString != newValueAsString)
				{
					EditorUtility.SetDirty(gameObject);
					
					if (typeof(int) == fieldInfo.FieldType)
					{
						if (int.TryParse(newValueAsString, out var result))
						{
							fieldInfo.SetValue(componentFromDataBase, result);
						}
					}
					else if (typeof(ulong) == fieldInfo.FieldType)
					{
						if (ulong.TryParse(newValueAsString, out var result))
						{
							fieldInfo.SetValue(componentFromDataBase, result);
						}
					}
					else if (typeof(long) == fieldInfo.FieldType)
					{
						if (long.TryParse(newValueAsString, out var result))
						{
							fieldInfo.SetValue(componentFromDataBase, result);
						}
					}
					else if (typeof(float) == fieldInfo.FieldType)
					{
						if (float.TryParse(newValueAsString, out var result))
						{
							fieldInfo.SetValue(componentFromDataBase, result);
						}
					}
					else if (typeof(bool) == fieldInfo.FieldType)
					{
						if (bool.TryParse(newValueAsString, out var result))
						{
							fieldInfo.SetValue(componentFromDataBase, result);
						}
					}
				}
			}
		}

		static void OpenWindowImpl()
		{
			SKObjectIDExporter getWindow = GetWindow<SKObjectIDExporter>(false, "Object ID Exporter", true);
			Rect wr = getWindow.position;

			int minwidth = columnWidthArray.Sum() + 20;
			const int minheight = 800;

			if (wr.width < minwidth || wr.height < minheight)
			{
				if (wr.width < minwidth)
					wr.width = minwidth;
				if (wr.height < minheight)
					wr.height = minheight;

				getWindow.position = wr;
			}
		}

		[MenuItem("SK/SK Object ID Exporter")]
		public static void OpenWindow()
		{
			OpenWindowImpl();
		}

		private void Clear()
		{
			_errorBySKObjectData.Clear();
			_skObjectDataNoID.Clear();
			_skObjectDataConflictID.Clear();
			_skObjectsSuccessByID.Clear();
			_listOfSkComponentName = new[] { "" };
			_listOfSkComponentType.Clear();
		}

		private void ParsingSheetName()
		{
			var listOfSkComponent = 
				from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
				from type in domainAssembly.GetTypes()
				where typeof(SKComponentBase).IsAssignableFrom(type) && !type.IsAbstract
				select type;

			_listOfSkComponentType = listOfSkComponent.ToList();
			_listOfSkComponentName = listOfSkComponent.Select(it =>
			{
				var typeName = it.ToString();
				return typeName.Substring(typeName.LastIndexOf(".", StringComparison.Ordinal) + 1);
			}).ToArray();
			
			_selectedComponentType = _listOfSkComponentType[_selectedComponentIndex];
		}

		private void Refresh()
		{
			Clear();
			
			ParsingSheetName();
			
			var skObjectDataList = FindSKOjbect();
			if (null == skObjectDataList)
			{
				return;
			}
			
			foreach (var skObjectData in skObjectDataList)
			{
				if (skObjectData.DataID <= 0)
				{
					_skObjectDataNoID.Add(skObjectData);
				}
				else if (0 <= _skObjectDataConflictID.FindIndex(it => it.DataID == skObjectData.DataID))
				{
					_skObjectDataConflictID.Add(skObjectData);
				}
				else if (_skObjectsSuccessByID.TryGetValue(skObjectData.DataID, out var skObjectsSuccessBySubKeyTemp) && skObjectsSuccessBySubKeyTemp.TryGetValue(skObjectData.DataSubKey, out var existValue))
				{
					_skObjectDataConflictID.Add(existValue);
					_skObjectDataConflictID.Add(skObjectData);
					
					skObjectsSuccessBySubKeyTemp.Remove(skObjectData.DataSubKey);
				}
				else
				{
					if (false == _skObjectsSuccessByID.TryGetValue(skObjectData.DataID, out var skObjectsSuccessBySubKey))
					{
						skObjectsSuccessBySubKey = new();
						_skObjectsSuccessByID.Add(skObjectData.DataID, skObjectsSuccessBySubKey);
					}
					skObjectsSuccessBySubKey.Add(skObjectData.DataSubKey, skObjectData);
				}
			}
		}

		private void Export()
		{
			if (0 < _skObjectDataConflictID.Count)
			{
				EditorUtility.DisplayDialog("오류", "ID 충돌이 해결되어야 추출됩니다", "확인");
				return;
			}
			
			Debug.Log($"데이터 추출 시작");

			List<string> csvList = new();
			csvList.Add($"ID\tSubKey\tObjectName");
			foreach (var skObjectsSuccessBySubKey in _skObjectsSuccessByID)
			{
				foreach (var it in skObjectsSuccessBySubKey.Value)
				{
					Debug.Log($"{it.Value.DataID}, {it.Value.DataSubKey}, {it.Value.DataObjectName}");
					csvList.Add($"{it.Value.DataID}\t{it.Value.DataSubKey}\t{it.Value.DataObjectName}");
				}
			}
			
			var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			WriteCsvExcel(desktopPath + "/#SKObjectData", csvList);
			
			Debug.Log($"데이터 추출 끝");

			var argument = "/select, \"" + desktopPath + "\\#SKObjectData.xlsx\"";
			Process.Start("Explorer.exe", argument);
		}
		
		private static List<ISKObjectData> FindSKOjbect()
		{
			var skObjectList = new List<ISKObjectData>();
			
			string[] guids = AssetDatabase.FindAssets( "t:Prefab" );
			var successAll = true;
			foreach( var guid in guids )
			{
				var path = AssetDatabase.GUIDToAssetPath( guid );
				var isExportDirectory = false;
				foreach (var exportDirectory in ExportDirectories)
				{
					if (path.Contains(exportDirectory))
					{
						isExportDirectory = true;
						break;
					}
				}

				if (false == isExportDirectory)
				{
					continue;
				}
				
				GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>( path );

				var skObjectDataInChildren = go.GetComponentsInChildren<ISKObjectData>();
				var skComponentInChildren = go.GetComponentsInChildren<SKComponentBase>();
				if (skObjectDataInChildren.Length <= 0)
				{
					if (skComponentInChildren.Length <= 0)
					{
						continue;	
					}
					else
					{
						var message = $"{nameof(ISKObjectData)} 컴포넌트가 존재하지 않습니다. 프리팹 루트에 1개는 존재해야 합니다";
						_errorBySKObjectData.Add((go, message));
						Debug.LogError(message);
						continue;
					}
				}

				var success = true;
				var skObjectCount = 0;
				Dictionary<GameObject, ISKObjectData> skObjectDataByObject = new();
				foreach (var it in skObjectDataInChildren)
				{
					if (skObjectDataByObject.ContainsKey(it.MyObject))
					{
						var message = $"{nameof(ISKObjectData)} 컴포넌트가 중복해서 존재합니다. 오브젝트당 1개만 존재할 수 있습니다";
						_errorBySKObjectData.Add((it.MyObject, message));
						Debug.LogError(message);
						success = false;
					}
					else
					{
						skObjectDataByObject.Add(it.MyObject, it);
					}

					if (it as SKObject)
					{
						skObjectCount++;
					}
				}
				
				foreach (var it in skComponentInChildren)
				{
					if (false == it.SkObject)
					{
						var message = $"{go.name} 프리팹에 {it.gameObject} 오브젝트에 {it.GetType()} 컴포넌트가 SKObject 값이 Null입니다.";
						_errorBySKObjectData.Add((it.gameObject, message));
						Debug.LogError(message);
						success = false;
					}
				}

				Dictionary<ISKObjectData, HashSet<Type>> componentListTypeBySKObjectData = new();
				var componentsFromDataBaseInChildren = go.GetComponentsInChildren<SKComponentFromDataBase>();
				foreach (var it in componentsFromDataBaseInChildren)
				{
					HashSet<Type> typeList;
					if (it.gameObject.TryGetComponent(out SKComponentObjectChild componentObjectChild))
					{
						if (false == componentListTypeBySKObjectData.TryGetValue(componentObjectChild, out typeList))
						{
							typeList = new();
							componentListTypeBySKObjectData.Add(componentObjectChild, typeList);
						}
					}
					else if (it.SkObject)
					{
						if (false == componentListTypeBySKObjectData.TryGetValue(it.SkObject, out typeList))
						{
							typeList = new();
							componentListTypeBySKObjectData.Add(it.SkObject, typeList);
						}
					}
					else
					{
						continue;
					}

					if (it.IgnoreIDSubKeyCheck)
					{
						
					}
					else if (typeList.Contains(it.GetType()))
					{
						var message =
							$"{go.name} 프리팹에 {it.name} 오브젝트에 {it.GetType()} 컴포넌트가 중복해서 존재합니다. SKComponentObjectChild 컴포넌트를 세팅하세요";
						_errorBySKObjectData.Add((it.gameObject, message));
						Debug.LogError(message);
						success = false;
					}
					else
					{
						typeList.Add(it.GetType());	
					}
				}

				if (1 < skObjectCount)
				{
					var message = $"{go.name} 프리팹에 SK 오브젝트 컴포넌트가 {skObjectCount}개 존재합니다. 프리팹당 1개만 존재할 수 있습니다";
					Debug.LogError(message);
					_errorBySKObjectData.Add((go, message));
					success = false;
				}

				if (false == go.TryGetComponent(out SKObject skObject))
				{
					var message = $"{go.name} 프리팹에 SK 오브젝트 컴포넌트가 자식에 위치하고 있습니다. 최상단에 존재해야 합니다."; 
					Debug.LogError(message);
					_errorBySKObjectData.Add((go, message));
					success = false;
				}

				if (false == success)
				{
					successAll = false;
					continue;
				}
				
				foreach (var it in skObjectDataInChildren)
				{
					skObjectList.Add(it);	
				}
			}

			if (false == successAll)
			{
				EditorUtility.DisplayDialog("오류", "오류가 검색되었습니다. 에러 로그를 확인해주세요", "확인");
				return null;
			}
			
			return skObjectList;
		}

		private static void WriteCsvExcel(string filePath, List<string> csvList)
		{
			File.WriteAllLines($"{filePath}.txt", csvList);
			File.Delete($"{filePath}.xlsx");
			ConvertCsvTextToExcel($"{filePath}.txt");
		}
		
		private static string GetTxtToXlsxPath()
		{
			return ZString.Concat(Application.dataPath, @"/../../TxtToXlsx/TxtToXlsx.exe");
		}

		private static void ConvertCsvTextToExcel(string args)
		{
			int exitCode;
			ProcessStartInfo processInfo;
			Process process;

			processInfo = new ProcessStartInfo(GetTxtToXlsxPath(), args);
			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			// *** Redirect the output ***
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;

			process = Process.Start(processInfo);
			process.WaitForExit();

			// *** Read the streams ***
			// Warning: This approach can lead to deadlocks, see Edit #2
			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			exitCode = process.ExitCode;

			Debug.Log("output>>" +  output);
			
			if (false == string.IsNullOrEmpty(error))
			{
				Debug.LogError("error>>" +  error);
			}
			
			process.Close();
		}
	}
}
