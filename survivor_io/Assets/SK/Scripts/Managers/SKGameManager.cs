using System;
using System.Collections.Generic;
using JHT.Scripts.Common;
using JHT.Scripts.Common.CollectionExtension;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.ResourceManager;
using UnityEngine;

namespace SK
{
	public enum SKIngameItemType
	{
		Gold,
		Exp,
		Weapon,
		SupportItem,
		Magnet,
		Bomb,
		Heal,
		RandomBox,
	}
	
    public class SKGameManager : MonoSingleton<SKGameManager>
    {
	    public SKItemSpawnManager ItemSpawnManager { get; private set; } = new();
	    public SKMonsterSpawnManager MonsterSpawnManager { get; private set; } = new();
	    public SKBackgroundManager BackgroundManager { get; private set; } = new();
	    public SKObjectManager ObjectManager { get; private set; } = new();
	    public SKStatManager PlayerStatManager { get; private set; }
	    public SKPlayerInput PlayerInput { get; private set; }

	    public uint Level { get; private set; } = 1;
	    
	    public float _elapsedExp;
	    public uint ElapsedExp => (uint)_elapsedExp;
	    
	    public float _elapsedGold;
	    public uint ElapsedGold => (uint)_elapsedGold;
	    
	    public uint ElapsedKill { get; private set; }
	    public DateTime PlayTime { get; private set; }

	    private Camera _mainCamera;
	    private bool _init;
	    public bool IsInit => _init;
	    private float _simulateTimer;

	    #region Pause

	    private int _pauseStack;
	    public bool IsPause => 0 < _pauseStack;
	    public event Action<bool> OnPause;

	    #endregion

	    #region BossBattle

	    private int _bossBattleStack;
	    public bool IsBossBattle => 0 < _bossBattleStack;
	    public event Action<bool, SKObjectMonster> OnBossBattle;
	    private List<SKObject> _bossRingBlockObjectList = new();

	    public void ShowBossBattleWarningUI()
	    {
		    Debug.Log("보스 습격 경고!");
	    }

	    public void BossBattleStart(SKObjectMonster skObjectMonster)
	    {
		    var prevIsBossBattle = IsBossBattle;
		    _bossBattleStack++;

		    if (false == prevIsBossBattle && IsBossBattle)
		    {
			    OnBossBattle?.Invoke(true, skObjectMonster);
			    if (BackgroundManager.BackgroundType != SKBackgroundManager.SKBackgroundType.Box)
			    {
				    SpawnBossRingBlock(SKConstants.BossRingBlockWidth, SKConstants.BossRingBlockHeight);   
			    }
			    ObjectManager.DestroyAllMonsterWithoutBoss();
		    }
	    }

	    public void BossBattleEnd(SKObjectMonster skObjectMonster)
	    {
		    var prevIsBossBattle = IsBossBattle;
		    _bossBattleStack--;

		    if (prevIsBossBattle && false == IsBossBattle)
		    {
			    OnBossBattle?.Invoke(false, skObjectMonster);
			    DestroyBossRingBlock();
		    }
	    }
	    
	    #endregion
	    
	    public bool IsClear { get; private set; }

	    #region Update

	    public event Action<float> OnGameFixedUpdate;
	    public event Action<float> OnGameUpdate;
	    public event Action<float> OnGameLateUpdate;

	    #endregion
	    
	    public void Init(SKPlayerInput playerInput, uint stageId)
	    {
		    _init = true;
		    _mainCamera = Camera.main;

		    PlayerInput = playerInput;
		    ItemSpawnManager.Init();
		    MonsterSpawnManager.Init(stageId);
		    BackgroundManager.Init(_mainCamera, stageId);
		    ObjectManager.Init(transform);
		    
		    PlayerStatManager = ObjectManager.ObjectPlayer.StatManager;
		    
		    InitPlayerBaseStat();
		    /*InitPlayerWeapon();*/
	    }

	    private void InitPlayerBaseStat()
	    {
		    var baseStatSetting = ResourceManager.Instance.LoadOriginalAsset<SKPlayerBaseStatSetting>(SKPlayerBaseStatSetting
			    .FilePath, useCache:false, saveCache:false);

		    foreach (var statData in baseStatSetting.statDataList)
		    {
			    PlayerStatManager.AddStatData(statData.StatType, statData.StatExprType, StatSourceType.BaseSetting, GetInstanceID(),
				    statData.Value);
		    }
	    }

	    private void InitPlayerWeapon()
	    {
		    ObjectManager.ObjectPlayer.AddWeapon(1);
	    }

	    /*private void OnDrawGizmos()
	    {
		    var camRect = BackgroundManager.GetCameraVisibleRect();
		    
		    Debug.DrawLine(camRect.min, new Vector3(camRect.xMin, camRect.yMax), Color.blue, 0.3f);
		    Debug.DrawLine(camRect.max, new Vector3(camRect.xMin, camRect.yMax), Color.blue, 0.3f);
		    Debug.DrawLine(camRect.max, new Vector3(camRect.xMax, camRect.yMin), Color.blue, 0.3f);
		    Debug.DrawLine(camRect.min, new Vector3(camRect.xMax, camRect.yMin), Color.blue, 0.3f);
	    }*/

	    void Update()
	    {
		    var deltaTime = Time.deltaTime;
		    UpdateImpl(deltaTime);
	    }

	    private void UpdateImpl(float deltaTime)
	    {
		    if (false == _init)
		    {
			    return;
		    }

		    if (IsPause)
		    {
			    return;
		    }

		    // Physics
		    {
			    if (Physics2D.simulationMode == SimulationMode2D.Script)
			    {
				    _simulateTimer += deltaTime;

				    var fixedDeltaTime = Time.fixedDeltaTime;
				    while (fixedDeltaTime <= _simulateTimer)
				    {
					    OnGameFixedUpdate?.Invoke(fixedDeltaTime);

					    _simulateTimer -= fixedDeltaTime;
					    Physics2D.Simulate(fixedDeltaTime);
					    /*ObjectManager.CheckContacts();*/
				    }
			    }
		    }

		    // Logic
		    {
			    // Update
			    {
				    if (false == IsBossBattle && false == CheatFlagType.HasFlagNonAlloc(SKCheatFlagType.PausePlayTime))
				    {
					    PlayTime = PlayTime.AddSeconds(deltaTime);
					    MonsterSpawnManager.GameUpdate(deltaTime);
					    ItemSpawnManager.GameUpdate(deltaTime);
				    }

				    if (CheatFlagType.HasFlagNonAlloc(SKCheatFlagType.InfinitySpawn))
				    {
					    SpawnZombie(1);
				    }

				    ObjectManager.GameUpdate(deltaTime);
				    OnGameUpdate?.Invoke(deltaTime);
			    }

			    // LateUpdate
			    {
				    OnGameLateUpdate?.Invoke(deltaTime);
			    }
			    
			    // 클리어 확인
			    if (MonsterSpawnManager.IsAllSpawned
			        && false == IsBossBattle 
			        && ObjectManager.GetObjectCountByType(SKObject.SKObjectType.Monster) <= 0)
			    {
				    Pause();
				    StageClear();
			    
				    Debug.Log("스테이지가 클리어되었습니다");
			    }
		    }
	    }

	    void LateUpdate()
	    {
		    var deltaTime = Time.deltaTime;
		    LateUpdateImpl(deltaTime);
	    }

	    private void LateUpdateImpl(float deltaTime)
	    {
		    if (false == _init)
		    {
			    return;
		    }

		    if (IsPause)
		    {
			    return;
		    }

		    BackgroundManager.GameLateUpdate(deltaTime);
	    }

	    public void AddExp(uint exp)
	    {
		    if (CheatFlagType.HasFlagNonAlloc(SKGameManager.SKCheatFlagType.LockExp))
		    {
			    return;
		    }
		    
		    var increasedExpEarnedPercent = PlayerStatManager.GetStatResultOnlyPercent(StatType.ExpEarned);
		    var addExpAsFloat = exp + exp * increasedExpEarnedPercent;

		    _elapsedExp += addExpAsFloat;

		    var newLevel = ElapsedExp / 10;
		    if (newLevel != Level)
		    {
			    Level = (uint)newLevel;
		    }
	    }

	    public void AddGold(uint gold)
	    {
		    var increasedGoldEarnedPercent = PlayerStatManager.GetStatResultOnlyPercent(StatType.GoldEarned);
		    var addGoldAsFloat = gold + gold * increasedGoldEarnedPercent;

		    _elapsedGold += addGoldAsFloat;
	    }

	    public void AddKill(uint kill)
	    {
		    ElapsedKill += kill;
	    }
	    
	    public void SpawnZombie(uint count)
	    {
		    var gameObjectAsset = ResourceManager.Instance.LoadOriginalAsset<GameObject>(SKConstants.ZombieFilePath);
		    for (int i = 0; i < count; i++)
		    {
			    SKMonsterSpawner.Spawn(gameObjectAsset, SKMonsterSpawnInfo.SpawnPositionConstraintType.InCamera);
		    }
	    }

	    public void Pause()
	    {
		    var isPause = IsPause;
		    ++_pauseStack;
		    if (false == isPause && IsPause)
		    {
			    OnPause?.Invoke(true);
		    }
	    }
	    
	    public void Resume()
	    {
		    if (IsClear)
		    {
			    return;
		    }
		    
		    var isPause = IsPause;
		    --_pauseStack;
		    _pauseStack = Math.Max(0, _pauseStack);

		    if (isPause && false == IsPause)
		    {
			    OnPause?.Invoke(false);
		    }
	    }

	    public void StageClear()
	    {
		    IsClear = true;
	    }

	    public void GameOver()
	    {
		    Pause();
		    
		    Debug.Log("게임오버");
	    }

	    public void AddDamageText(Vector3 worldPosition, ulong damage)
	    {
		    Debug.Log(damage.ToStringCached());
	    }

	    public Rect GetCameraVisibleRect(Vector3 cameraPosition)
	    {
		    if (_mainCamera.IsNull())
		    {
			    return Rect.zero;
		    }

		    var screenHeight = Math.Max(Screen.width, Screen.height);
		    var screenWidth = Math.Min(Screen.width, Screen.height);
		    
		    var height = _mainCamera.orthographicSize;
		    var width = height / screenHeight * screenWidth;
		    var rect = new Rect(cameraPosition.x - width, cameraPosition.y - height, width * 2.0f, height * 2.0f);
		    
		    return rect;
	    }

	    public Rect GetCameraVisibleRect()
	    {
		    if (_mainCamera.IsNull())
		    {
			    return Rect.zero;
		    }
		    
		    var cameraPosition = _mainCamera.transform.position;
		    return GetCameraVisibleRect(cameraPosition);
	    }

	    public void SpawnBossRingBlock(uint width, uint height)
	    {
		    var playerPosition = SKGameManager.Instance.ObjectManager.ObjectPlayer.transform.position;
		    var playerPosition2D = new Vector2(playerPosition.x, playerPosition.y);
		    if (BackgroundManager.BackgroundType == SKBackgroundManager.SKBackgroundType.InfinityVertical)
		    {
			    playerPosition2D.x = 0;
		    }
		    playerPosition2D.x -= width / 2.0f - SKConstants.BossRingBlockSizeHalf;
		    playerPosition2D.y -= height / 2.0f - SKConstants.BossRingBlockSizeHalf;
	        
		    var offsetCell = Vector2Int.zero;
		    var directionIndex = 0;
		    Vector2Int[] directionOffset = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
		    while (true)
		    {
			    var newOffsetCell = offsetCell + directionOffset[directionIndex];
			    if (newOffsetCell.x < 0 || newOffsetCell.y < 0 || width <= newOffsetCell.x || height <= newOffsetCell.y)
			    {
				    directionIndex++;
				    if (directionOffset.Length <= directionIndex)
				    {
					    break;
				    }
				    
				    continue;
			    }

			    offsetCell = newOffsetCell;
			    var spawnPosition = playerPosition2D + offsetCell;
			    SpawnBossRingBlock(spawnPosition);
		    }
	    }

	    private void SpawnBossRingBlock(Vector2 position)
	    {
		    _bossRingBlockObjectList.Add(SKSpawner.Spawn(new SKSpawnInfoBase()
		    {
			    gameObject = ResourceManager.Instance.LoadOriginalAsset<GameObject>(SKConstants.BossRingBlockFilePath),
			    position = position,
		    }, null));
	    }

	    private void DestroyBossRingBlock()
	    {
		    foreach (var skObject in _bossRingBlockObjectList)
		    {
			    skObject.DestroyObject();
		    }
		    
		    _bossRingBlockObjectList.Clear();
	    }

	    #region Cheat
	    
	    [Flags]
	    public enum SKCheatFlagType
	    {
		    None,
		    
		    PlayerInvincibility = 1 << 0,
		    MonsterInvincibility = 1 << 1,
		    LockExp = 1 << 2,
		    PausePlayTime = 1 << 3,
		    InfinitySpawn = 1 << 4,

		    All = int.MaxValue
	    }

	    public SKCheatFlagType CheatFlagType { get; private set; }

	    public void PlayerInvincibilityToggle()
	    {
		    if (false == CheatFlagType.HasFlagNonAlloc(SKCheatFlagType.PlayerInvincibility))
			    CheatFlagType |= SKCheatFlagType.PlayerInvincibility;
		    else
			    CheatFlagType &= ~SKCheatFlagType.PlayerInvincibility;
	    }

	    public void MonsterInvincibilityToggle()
	    {
		    if (false == CheatFlagType.HasFlagNonAlloc(SKCheatFlagType.MonsterInvincibility))
			    CheatFlagType |= SKCheatFlagType.MonsterInvincibility;
		    else
			    CheatFlagType &= ~SKCheatFlagType.MonsterInvincibility;
	    }
	    
	    public void LockExpToggle()
	    {
		    if (false == CheatFlagType.HasFlagNonAlloc(SKCheatFlagType.LockExp))
			    CheatFlagType |= SKCheatFlagType.LockExp;
		    else
			    CheatFlagType &= ~SKCheatFlagType.LockExp;
	    }
	    
	    public void PausePlayTimeToggle()
	    {
		    if (false == CheatFlagType.HasFlagNonAlloc(SKCheatFlagType.PausePlayTime))
			    CheatFlagType |= SKCheatFlagType.PausePlayTime;
		    else
			    CheatFlagType &= ~SKCheatFlagType.PausePlayTime;
	    }
	    
	    public void InfinitySpawnToggle()
	    {
		    if (false == CheatFlagType.HasFlagNonAlloc(SKCheatFlagType.InfinitySpawn))
			    CheatFlagType |= SKCheatFlagType.InfinitySpawn;
		    else
			    CheatFlagType &= ~SKCheatFlagType.InfinitySpawn;
	    }
	    
	    public void LevelUp()
	    {
		    AddExp(10);
	    }

	    public void AddPlayTime(ulong second)
	    {
		    if (false == IsBossBattle)
		    {
			    float deltaTimeRemain = second;
			    var deltaTime = 1 / 60.0f;
			    while (deltaTime <= deltaTimeRemain)
			    {
				    deltaTimeRemain -= deltaTime;

				    PlayTime = PlayTime.AddSeconds(deltaTime);
				    ItemSpawnManager.GameUpdate(deltaTime);
				    MonsterSpawnManager.GameUpdate(deltaTime);
			    }
		    }
	    }

	    #endregion
    }
}
