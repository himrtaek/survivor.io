using System;
using JHT.Scripts;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;

using UnityEditor;

namespace SK
{
	public class SKStatTestEditor : EditorWindow
	{
		private SKObject _selectedSKObject;
		private StatType _selectedStatType;
		private StatExprType _selectedStatExprType;
		private StatSourceType _selectedStatSourceType;
		private long _selectedStatKey;
		private long _selectedStatValue;
		
		void OnGUI()
		{
			if (false == AppMain.IsPlaying)
			{
				EditorGUILayout.LabelField("게임 실행 중에만 작동합니다");
				return;
			}

			if (false == _selectedSKObject)
			{
				_selectedSKObject = SKGameManager.Instance.ObjectManager.ObjectPlayer;
				if (false == _selectedSKObject)
				{
					return;	
				}
			}
		
			EditorGUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				EditorGUILayout.TextField("대상 SKObject");
				GUI.enabled = true;
				_selectedSKObject = (SKObject)EditorGUILayout.ObjectField(_selectedSKObject, typeof(SKObject), true);
			}
			EditorGUILayout.EndHorizontal();

			if (false == _selectedSKObject)
			{
				EditorGUILayout.LabelField("대상 SKObject를 선택하세요");
				return;
			}
			
			EditorGUILayout.Space();
			
			GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.TextField("StateType", GUILayout.Width(150));
				EditorGUILayout.TextField("StatExprType", GUILayout.Width(150));
				EditorGUILayout.TextField("StatSourceType", GUILayout.Width(150));
				EditorGUILayout.TextField("Key", GUILayout.Width(150));
				EditorGUILayout.TextField("Value", GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;
				
			EditorGUILayout.BeginHorizontal();
			{
				_selectedStatType = (StatType)EditorGUILayout.EnumPopup(_selectedStatType, GUILayout.Width(150));
				_selectedStatExprType = (StatExprType)EditorGUILayout.EnumPopup(_selectedStatExprType, GUILayout.Width(150));
				_selectedStatSourceType = (StatSourceType)EditorGUILayout.EnumPopup(_selectedStatSourceType, GUILayout.Width(150));
				_selectedStatKey = EditorGUILayout.LongField(_selectedStatKey, GUILayout.Width(150));
				_selectedStatValue = EditorGUILayout.LongField(_selectedStatValue, GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("데이터 추가"))
			{
				_selectedSKObject.StatManager.AddStatData(_selectedStatType, _selectedStatExprType, _selectedStatSourceType, _selectedStatKey, _selectedStatValue);
			}
			
			if (GUILayout.Button("데이터 삭제"))
			{
				_selectedSKObject.StatManager.RemoveStatData(_selectedStatType, _selectedStatExprType, _selectedStatSourceType, _selectedStatKey);
			}
			
			EditorGUILayout.Space();

			GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.TextField("State Type");
				EditorGUILayout.TextField("Value");
			}
			EditorGUILayout.EndHorizontal();
			
			for (int i = 0; i < Enum.GetValues(typeof(StatType)).Length; i++)
			{
				var statType = (StatType)i;
				var statValue = _selectedSKObject.StatManager.GetStatResultValue(statType);
				if (statValue == 0)
				{
					continue;
				}
				
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField(statType.ToStringCached());
					EditorGUILayout.TextField(statValue.ToStringCached());
				}
				EditorGUILayout.EndHorizontal();
			}
			GUI.enabled = true;
		}

		static void OpenWindowImpl()
		{
			SKStatTestEditor getWindow = EditorWindow.GetWindow<SKStatTestEditor>(false, "스텟 테스트 에디터", true);
			Rect wr = getWindow.position;

			const int minwidth = 750;
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

		[MenuItem("SK/스텟 테스트 에디터 열기", false, 9999)]
		static public void OpenWindow()
		{
			OpenWindowImpl();
		}
	}
}
