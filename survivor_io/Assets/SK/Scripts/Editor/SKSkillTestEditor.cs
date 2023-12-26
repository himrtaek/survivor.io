using System.Collections.Generic;
using JHT.Scripts;
using UnityEngine;

using UnityEditor;

namespace SK
{
	public class SKSkillTestEditor : EditorWindow
	{
		private SKObject _selectedSKObject;
		private SKSkillType _selectedSkillType;
		private long _selectedSkillValue1;
		private long _selectedSkillValue2;
		private long _selectedSkillValue3;
		private long _selectedSkillValue4;
		private List<SKSkill> _skillList = new();
		
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
				EditorGUILayout.TextField("SkillType", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue1", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue2", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue3", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue4", GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;
				
			EditorGUILayout.BeginHorizontal();
			{
				_selectedSkillType = (SKSkillType)EditorGUILayout.EnumPopup(_selectedSkillType, GUILayout.Width(150));
				_selectedSkillValue1 = EditorGUILayout.LongField(_selectedSkillValue1, GUILayout.Width(150));
				_selectedSkillValue2 = EditorGUILayout.LongField(_selectedSkillValue2, GUILayout.Width(150));
				_selectedSkillValue3 = EditorGUILayout.LongField(_selectedSkillValue3, GUILayout.Width(150));
				_selectedSkillValue4 = EditorGUILayout.LongField(_selectedSkillValue4, GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("데이터 추가"))
			{
				var skSkill = new SKSkill(_selectedSkillType, _selectedSkillValue1, _selectedSkillValue2, _selectedSkillValue3,
					_selectedSkillValue4);
				skSkill.DoAction(_selectedSKObject, StatSourceType.Test, (long)_selectedSkillType);
				
				_skillList.Add(skSkill);
			}
			
			EditorGUILayout.Space();

			GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.TextField("SkillType", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue1", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue2", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue3", GUILayout.Width(150));
				EditorGUILayout.TextField("SkillValue4", GUILayout.Width(150));
				EditorGUILayout.TextField("삭제", GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();

			var removeIndex = -1;
			for (int i = 0; i < _skillList.Count; i++)
			{
				var skSKill = _skillList[i];
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.EnumPopup(skSKill.SkillType, GUILayout.Width(150));
					EditorGUILayout.LongField(skSKill.SkillValue1, GUILayout.Width(150));
					EditorGUILayout.LongField(skSKill.SkillValue2, GUILayout.Width(150));
					EditorGUILayout.LongField(skSKill.SkillValue3, GUILayout.Width(150));
					EditorGUILayout.LongField(skSKill.SkillValue4, GUILayout.Width(150));
					GUI.enabled = true;
					if (GUILayout.Button("삭제", GUILayout.Width(150)))
					{
						removeIndex = i;
					}
					GUI.enabled = false;
				}
				EditorGUILayout.EndHorizontal();
			}

			if (0 <= removeIndex)
			{
				var skillType = _skillList[removeIndex].SkillType;
				_skillList[removeIndex].UnDoAction(_selectedSKObject, StatSourceType.Test, (long)skillType);
				_skillList.RemoveAt(removeIndex);
			}
			
			GUI.enabled = true;
		}

		static void OpenWindowImpl()
		{
			SKSkillTestEditor getWindow = EditorWindow.GetWindow<SKSkillTestEditor>(false, "스킬 테스트 에디터", true);
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

		[MenuItem("SK/스킬 테스트 에디터 열기", false, 9999)]
		static public void OpenWindow()
		{
			OpenWindowImpl();
		}
	}
}
