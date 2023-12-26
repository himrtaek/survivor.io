using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JHT.Scripts;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.ResourceManager;
using UnityEngine;

namespace SK
{
    public class SKGameScene : BaseScene
    {
	    #region inspector
	    [SerializeField] private SKPlayerInput playerInput;
	    #endregion

	    private bool _showCheat;
	    
	    protected override void OnStartEnd()
	    {
		    base.OnStartEnd();

		    var stageId = 1u;
		    SKGameManager.Instance.Init(playerInput, stageId);
		    playerInput.Init();
	    }

	    protected override void Update()
	    {
		    base.Update();
		    
		    if (Input.GetKeyDown(KeyCode.F5))
		    {
			    Reload();
			    return;
		    }
	    }

	    public static void Reload()
	    {
		    ResourceManager.Instance.Clear();
		    SKGameManager.Instance.Pause();
		    SceneManager.Instance.ChangeScene(SceneType.SKGameScene, true, true, isReload:true);
	    }

	    private void OnGUI()
	    {
		    if (false == SKGameManager.Instance.IsInit)
		    {
			    return;
		    }

		    float fScreenMin = Screen.width < Screen.height ? Screen.width : Screen.height;
		    float fScreenMax = Screen.width < Screen.height ? Screen.height : Screen.width;
		    float fScale = fScreenMin / 1280.0f;

		    float fontSizeRate = 30;
		    int fontSize = (int)(fontSizeRate * fScale);
		    var styleButton = new GUIStyle(GUI.skin.button) { fontSize = fontSize };

		    if (GUILayout.Button(_showCheat ? "치트 창 닫기" : "치트 창 열기", styleButton))
		    {
			    _showCheat = !_showCheat;
		    }

		    if (_showCheat)
		    {
			    var playerInvincibilityToggleText =
			    SKGameManager.Instance.CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.PlayerInvincibility)
				    ? "플레이어 무적(On -> Off)"
				    : "플레이어 무적(Off -> On)";
			    
			    if (GUILayout.Button(playerInvincibilityToggleText, styleButton))
			    {
				    SKGameManager.Instance.PlayerInvincibilityToggle();
			    }
			    
			    var monsterInvincibilityToggleText =
				    SKGameManager.Instance.CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.MonsterInvincibility)
					    ? "몬스터 무적(On -> Off)"
					    : "몬스터 무적(Off -> On)";
			    
			    if (GUILayout.Button(monsterInvincibilityToggleText, styleButton))
			    {
				    SKGameManager.Instance.MonsterInvincibilityToggle();
			    }
			    
			    var lockExpToggleText =
				    SKGameManager.Instance.CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.LockExp)
					    ? "경험치 잠금(On -> Off)"
					    : "경험치 잠금(Off -> On)";
			    
			    if (GUILayout.Button(lockExpToggleText, styleButton))
			    {
				    SKGameManager.Instance.LockExpToggle();
			    }
			    
			    if (GUILayout.Button("레벨업", styleButton))
			    {
				    SKGameManager.Instance.LevelUp();
			    }
			    
			    var pausePlayTimeToggleText =
				    SKGameManager.Instance.CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.PausePlayTime)
					    ? "몬스터 스폰 정지(On -> Off)"
					    : "몬스터 스폰 정지(Off -> On)";
			    
			    if (GUILayout.Button(pausePlayTimeToggleText, styleButton))
			    {
				    SKGameManager.Instance.PausePlayTimeToggle();
			    }
			    
			    if (GUILayout.Button("10초 빨리감기", styleButton))
			    {
				    SKGameManager.Instance.AddPlayTime(10);
			    }
			    
			    if (GUILayout.Button("30초 빨리감기", styleButton))
			    {
				    SKGameManager.Instance.AddPlayTime(30);
			    }
			    
			    if (GUILayout.Button("좀비 10마리 추가", styleButton))
			    {
				    SKGameManager.Instance.SpawnZombie(10);
			    }
			    
			    var infinitySpawnToggleText =
				    SKGameManager.Instance.CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.InfinitySpawn)
					    ? "좀비 무한 스폰(On -> Off)"
					    : "좀비 무한 스폰(Off -> On)";
			    
			    if (GUILayout.Button(infinitySpawnToggleText, styleButton))
			    {
				    SKGameManager.Instance.InfinitySpawnToggle();
			    }
		    }
	    }
    }
}
