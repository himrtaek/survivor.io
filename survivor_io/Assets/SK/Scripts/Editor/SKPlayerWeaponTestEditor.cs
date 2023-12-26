using JHT.Scripts;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;

using UnityEditor;

namespace SK
{
	public class SKPlayerWeaponTestEditor : EditorWindow
	{
		private SKObjectPlayer _objectPlayer;
		private int _selectedWeaponId;
		private int _selectedWeaponLevel;
		private int _selectedSupportItemId;
		private int _selectedSupportItemLevel;
		
		void OnGUI()
		{
			if (false == AppMain.IsPlaying)
			{
				EditorGUILayout.LabelField("게임 실행 중에만 작동합니다");
				return;
			}

			if (false == _objectPlayer)
			{
				_objectPlayer = SKGameManager.Instance.ObjectManager.ObjectPlayer;
				if (false == _objectPlayer)
				{
					return;	
				}
			}
			
			EditorGUILayout.Space();
			
			ProcessWeapon();
			
			EditorGUILayout.Space();
			
			ProcessSupportItem();
		}

		private void ProcessWeapon()
		{
			GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.TextField("Id", GUILayout.Width(150));
				EditorGUILayout.TextField("Level", GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;

			EditorGUILayout.BeginHorizontal();
			{
				_selectedWeaponId = (int)(SKPlayerWeaponType)EditorGUILayout.EnumPopup((SKPlayerWeaponType)_selectedWeaponId, GUILayout.Width(150));
				_selectedWeaponLevel = EditorGUILayout.IntField(_selectedWeaponLevel, GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("무기 추가"))
			{
				_objectPlayer.AddWeapon((uint)_selectedWeaponId, (uint)_selectedWeaponLevel);
			}

			if (GUILayout.Button("무기 삭제"))
			{
				_objectPlayer.RemoveWeapon((uint)_selectedWeaponId);
			}

			EditorGUILayout.Space();

			GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.TextField("Id");
				EditorGUILayout.TextField("Level");
			}
			EditorGUILayout.EndHorizontal();

			foreach (var it in _objectPlayer.PlayerWeaponByType)
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField(((SKPlayerWeaponType)it.Key).ToStringCached());
					EditorGUILayout.TextField(it.Value.level.ToStringCached());
				}
				EditorGUILayout.EndHorizontal();
			}

			GUI.enabled = true;
		}

		private void ProcessSupportItem()
		{
			GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.TextField("Id", GUILayout.Width(150));
				EditorGUILayout.TextField("Level", GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;

			EditorGUILayout.BeginHorizontal();
			{
				_selectedSupportItemId = (int)(SKPlayerWeaponType)EditorGUILayout.EnumPopup((SKSupportItemType)_selectedSupportItemId, GUILayout.Width(150));
				_selectedSupportItemLevel = EditorGUILayout.IntField(_selectedSupportItemLevel, GUILayout.Width(150));
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("지원품 추가"))
			{
				_objectPlayer.AddSupportItem((uint)_selectedSupportItemId, (uint)_selectedSupportItemLevel);
			}

			if (GUILayout.Button("지원품 삭제"))
			{
				_objectPlayer.RemoveSupportItem((uint)_selectedSupportItemId);
			}

			EditorGUILayout.Space();

			GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.TextField("Id");
				EditorGUILayout.TextField("Level");
			}
			EditorGUILayout.EndHorizontal();

			foreach (var it in _objectPlayer.SupportItemByType)
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField(((SKSupportItemType)it.Key).ToStringCached());
					EditorGUILayout.TextField(it.Value.level.ToStringCached());
				}
				EditorGUILayout.EndHorizontal();
			}

			GUI.enabled = true;
		}

		static void OpenWindowImpl()
		{
			SKPlayerWeaponTestEditor getWindow = EditorWindow.GetWindow<SKPlayerWeaponTestEditor>(false, "플레이어 무기 테스트 에디터", true);
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

		[MenuItem("SK/플레이어 무기 테스트 에디터 열기", false, 9999)]
		static public void OpenWindow()
		{
			OpenWindowImpl();
		}
	}
}
