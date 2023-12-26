using System;
using System.Collections.Generic;
using JHT.Scripts;
using JHT.Scripts.Common;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.GameObjectPool;
using JHT.Scripts.ResourceManager;
using UnityEngine;

namespace SK
{
    public class SKObjectManager
    {
	    private Transform _objectRoot;
	    public Transform ObjectRoot => _objectRoot;
	    private readonly Dictionary<SKObject.SKObjectType, GameObject> _goParentByObjectType = new();
	    
	    private ulong _lastSpawnKey; 
	    private Dictionary<int, Dictionary<ulong, SKObject>> _objectByType = new();
	    private Dictionary<ulong, SKObject> _updateObjectBySpawnId = new();
	    private List<(SKObject skObject, float remainTime)> _destroyObjects = new();
	    private List<int> _destroyObjectRemoveIndexList = new();
	    private Queue<SKObject> _destroyObjectTemp = new();
	    private List<SKObject> _updateObjectsTemp = new();
	    private List<SKObject> _destroyObjectsTemp = new();
	    
	    public SKObjectPlayer ObjectPlayer
	    {
		    get;
		    private set;
	    }

	    public void Init(Transform objectRoot)
	    {
		    _objectRoot = objectRoot;
		    
		    var originalAsset = ResourceManager.Instance.LoadOriginalAsset<GameObject>(SKConstants.PlayerFilePath);
		    var instance = SKGameManager.Instance.ObjectManager.SpawnObject(originalAsset, null, false);
		    if (false == instance.TryGetComponent(out SKObjectPlayer player).IsFalse())
		    {
			    ObjectPlayer = player;
		    }
	    }

	    public void GameUpdate(float deltaTime)
	    {
		    if (AppMain.IsQuit)
		    {
			    return;
		    }
		    
		    ProcessObjectUpdate(deltaTime);
		    ProcessObjectDestroy(deltaTime);
	    }

	    public void CheckContacts()
	    {
		    /*ProcessObjectCollision();*/
	    }

	    public void AddUpdateObjectList(SKObject skObject)
	    {
		    _updateObjectBySpawnId.Add(skObject.SpawnId, skObject);
	    }
	    
	    public void RemoveUpdateObjectList(SKObject skObject)
	    {
		    _updateObjectBySpawnId.Remove(skObject.SpawnId);
	    }

	    private void ProcessObjectUpdate(float deltaTime)
	    {
		    foreach (var it in _updateObjectBySpawnId)
		    {
			    _updateObjectsTemp.Add(it.Value);
		    }

		    foreach (var skObject in _updateObjectsTemp)
		    {
			    if (skObject.ObjectState != SKObject.SKObjectStateType.Spawned)
			    {
				    continue;
			    }
			    
			    skObject.GameUpdate(deltaTime);
		    }

		    _updateObjectsTemp.Clear();
	    }

	    /*private void ProcessObjectCollision()
	    {
		    void CheckObjectCollision(SKObject.SKObjectType type1, SKObject.SKObjectType type2)
		    {
			    if (false == _objectByType.TryGetValue((int)type1, out var dict1))
			    {
				    return;
			    }
			    
			    if (false == _objectByType.TryGetValue((int)type2, out var dict2))
			    {
				    return;
			    }

			    foreach (var skObject1 in dict1.Values)
			    {
				    if (false == skObject1.TryGetSKComponent(out SKComponentAttacker attacker))
				    {
					    continue;
				    }
				    
				    foreach (var skObject2 in dict2.Values)
				    {
					    if (false == skObject2.TryGetSKComponent(out SKComponentAttackee attackee))
					    {
						    continue;
					    }

					    if (false == attacker.Collider2D.bounds.Intersects(attackee.Collider2D.bounds))
					    {
						    continue;
					    }
					    
					    attacker.OnCollision(attackee);
				    }   
			    }
		    }
		    
		    CheckObjectCollision(SKObject.SKObjectType.Player, SKObject.SKObjectType.Monster);
		    CheckObjectCollision(SKObject.SKObjectType.PlayerWeapon, SKObject.SKObjectType.MonsterWeapon);
		    CheckObjectCollision(SKObject.SKObjectType.PlayerWeapon, SKObject.SKObjectType.Monster);
	    }*/

	    private void ProcessObjectDestroy(float deltaTime)
	    {
		    for (var i = 0; i < _destroyObjects.Count; i++)
		    {
			    var it = _destroyObjects[i];
			    var remainTime = it.remainTime - deltaTime;
			    if (remainTime <= 0)
			    {
				    _destroyObjectTemp.Enqueue(it.skObject);
				    _destroyObjectRemoveIndexList.Add(i);
			    }
			    else
			    {
				    _destroyObjects[i] = (it.skObject, remainTime);
			    }
		    }

		    for (int i = _destroyObjectRemoveIndexList.Count - 1; i >= 0; i--)
		    {
			    var removeIndex = _destroyObjectRemoveIndexList[i];
			    _destroyObjects.RemoveAt(removeIndex);
		    }
		    
		    _destroyObjectRemoveIndexList.Clear();
		    
		    foreach (var skObject in _destroyObjectTemp)
		    {
			    DestroyObjectNow(skObject);
		    }

		    _destroyObjectTemp.Clear();
	    }

	    private void DestroyObjectNow(SKObject skObject)
	    {
		    // 오브젝트 리스트에서 제거
		    if (0 < skObject.SpawnId)
		    {
			    if (skObject.EnableGameUpdate && _updateObjectBySpawnId.Remove(skObject.SpawnId).IsFalse())
			    {

			    }
			    
			    if (false == _objectByType.TryGetValue((int)skObject.ObjectType, out var objectBySpawnId).IsFalse())
			    {
				    if (objectBySpawnId.Remove(skObject.SpawnId).IsFalse())
				    {

				    }
			    }
		    }

		    // 자식 정리
		    while (0 < skObject.SpawnChildObjectList.Count)
		    {
			    DestroyObjectNow(skObject.SpawnChildObjectList[0]);
		    }

		    if (0 < skObject.SpawnObjectSpawnId)
		    {
			    var spawnerSkObject = SKGameManager.Instance.ObjectManager.GetObjectBySpawnId(skObject.SpawnObjectSpawnId);
			    if (spawnerSkObject)
			    {
				    spawnerSkObject.StatManager.SyncEndTo(skObject.StatManager, StatType.AttackPower);
			    }
		    }

		    // 부모에서 분리
		    if (skObject.SpawnParentObject)
		    {
			    skObject.SpawnParentObject.RemoveChildObject(skObject);
			    skObject.SpawnParentObject = null;
			    skObject.transform.SetParent(GetParentByObjectType(skObject.ObjectType).transform, false);
		    }
		    
		    // 상태 변경
		    skObject.ChangeState(SKObject.SKObjectStateType.Destroyed);

		    // 이벤트 브로드캐스팅
		    var eventParam = SKEventParam.GetOrNewParam<SKDestroyEventParam>();
		    skObject.EventManager.BroadCast(SKEventManager.SKEventType.Destroy, eventParam);
		    
		    // 파괴 콜백
		    skObject.OnSKObjectDestroy();

		    // 트리거 클리어
		    skObject.TriggerManager.Clear();

		    // 스텟 클리어
		    skObject.StatManager.Clear();

		    // Active Off
		    skObject.gameObject.SetActive(false);
	    }

	    public GameObject GetParentByObjectType(SKObject.SKObjectType objectType)
	    {
		    if (false == _goParentByObjectType.TryGetValue(objectType, out var go) || go.IsNull(false))
		    {
			    for (int i = 0; i < ObjectRoot.childCount; ++i)
			    {
				    if (objectType.ToStringCached() == ObjectRoot.GetChild(i).name)
				    {
					    go = ObjectRoot.GetChild(i).gameObject;
					    break;
				    }
			    }

			    if (go.IsNull(false))
			    {
				    go = new GameObject(objectType.ToStringCached()) { hideFlags = HideFlags.DontSave };
				    go.transform.SetParent(ObjectRoot, false);
			    }

			    _goParentByObjectType[objectType] = go;
		    }

		    return go;
	    }

	    private ulong GenerateSpawnKey()
	    {
		    return ++_lastSpawnKey;
	    }

	    public SKObject SpawnObject(string filePath, SKObject spawnerSkObject, bool spawnerIsParent, Action<SKObject> onSpawnBefore = null)
	    {
		    var originalAsset = ResourceManager.Instance.LoadOriginalAsset<GameObject>(filePath);
		    if (originalAsset.IsNull())
		    {
			    return null;
		    }
		    
		    return SpawnObject(originalAsset, spawnerSkObject, spawnerIsParent, onSpawnBefore);
	    }

	    public SKObject SpawnObject(GameObject originalAsset, SKObject spawnerSkObject, bool spawnerIsParent, Action<SKObject> onSpawnBefore = null)
	    {
		    if (originalAsset.TryGetComponent(out SKObject skObjectOriginal).IsFalse())
		    {
			    return null;
		    }

		    var parentTransform = spawnerIsParent
			    ? spawnerSkObject.transform
			    : GetParentByObjectType(skObjectOriginal.ObjectType).transform;
		    
		    var go = GameObjectPoolManager.Instance.GetOrNewObject(originalAsset, parentTransform);
		    if (go.IsNull())
		    {
			    return null;
		    }
		    
		    if (go.TryGetComponent(out SKObject skObject).IsFalse())
		    {
			    return null;
		    }

		    if (false == _objectByType.TryGetValue((int)skObject.ObjectType, out var objectBySpawnId))
		    {
			    objectBySpawnId = new();
			    _objectByType.Add((int)skObject.ObjectType, objectBySpawnId);
		    }

		    var spawnId = GenerateSpawnKey();
		    skObject.SetSpawnId(spawnId);
		    
		    objectBySpawnId.Add(skObject.SpawnId, skObject);

		    skObject.SetEnableGameUpdate(skObject.AutoEnableGameUpdate, true);
		    
		    if (skObject.EnableGameUpdate)
		    {
			    AddUpdateObjectList(skObject);
		    }

		    if (spawnerIsParent)
		    {
			    spawnerSkObject.AddChildObject(skObject);
			    skObject.SpawnParentObject = spawnerSkObject;
		    }

		    if (spawnerSkObject)
		    {
			    skObject.SpawnObjectSpawnId = spawnerSkObject.SpawnId;
			    
			    switch (skObject.StatOverrideType)
			    {
				    case SKObject.SKStatOverrideType.Auto:
				    {
					    if (false == skObject is SKObjectCreature)
					    {
						    if (skObject is SKObjectSpawner or SKObjectPlayerWeaponSpawner)
						    {
							    // 스폰하려는 객체가 스포너인 경우 부모의 스텟 동기화
							    spawnerSkObject.StatManager.SyncStartTo(skObject.StatManager, StatType.AttackPower);   
						    }
						    else
						    {
							    // 스폰하려는 객체가 스포너가 아닌 경우 부모의 스텟 복사
							    spawnerSkObject.StatManager.CopyTo(skObject.StatManager, StatType.AttackPower);
						    }   
					    }
				    }
					    break;
				    case SKObject.SKStatOverrideType.Sync:
				    {
					    spawnerSkObject.StatManager.SyncStartTo(skObject.StatManager, StatType.AttackPower);
				    }
					    break;
				    case SKObject.SKStatOverrideType.Copy:
				    {
					    spawnerSkObject.StatManager.CopyTo(skObject.StatManager, StatType.AttackPower);
				    }
					    break;
				    default:
					    throw new ArgumentOutOfRangeException();
			    }
		    }
		    
		    onSpawnBefore?.Invoke(skObject);

		    if (spawnerSkObject)
		    {
			    var spawnChildObjectEventParam = SKEventParam.GetOrNewParam<SKSpawnChildObjectEventParam>();
			    spawnChildObjectEventParam.SpawnObject = skObject;
			    spawnerSkObject.EventManager.BroadCast(SKEventManager.SKEventType.SpawnChildObject, spawnChildObjectEventParam);
		    }

		    {
			    skObject.ChangeState(SKObject.SKObjectStateType.ReadyForSpawn);
			    
			    var eventParam = SKEventParam.GetOrNewParam<SKReadyForSpawnEventParam>();
			    eventParam.SpawnId = spawnId;
			    skObject.EventManager.BroadCast(SKEventManager.SKEventType.ReadyForSpawn, eventParam);
		    }

		    {
			    // 스폰 콜백
			    skObject.OnSKObjectSpawn();
			    
			    skObject.ChangeState(SKObject.SKObjectStateType.Spawned);
			    
			    var eventParam = SKEventParam.GetOrNewParam<SKSpawnEventParam>();
			    eventParam.SpawnId = spawnId;
			    skObject.EventManager.BroadCast(SKEventManager.SKEventType.Spawn, eventParam);
		    }
		    
		    /*N2Logger<LogCombat>.Log($"{skObject.name}이 스폰되었습니다");*/
		    
		    return skObject;
	    }

	    public void DestroyObject(SKObject skObject, float delay = 0)
	    {
		    if (SKObject.SKObjectStateType.ReadyForDestroy <= skObject.ObjectState)
		    {
			    return;
		    }
		    
		    skObject.ChangeState(SKObject.SKObjectStateType.ReadyForDestroy);
		    /*N2Logger<LogCombat>.Log($"{skObject.name} 파괴");*/
		    
		    skObject.OnSKObjectReadyForDestroy();
		    
		    _destroyObjects.Add((skObject, delay));
	    }

	    public void DestroyAllMonsterWithoutBoss()
	    {
		    if (false == _objectByType.TryGetValue((int)SKObject.SKObjectType.Monster, out var objectBySpawnId))
		    {
			    return;
		    }

		    foreach (var (spawnId, skObject) in objectBySpawnId)
		    {
			    if (skObject.ObjectState != SKObject.SKObjectStateType.Spawned)
			    {
				    continue;
			    }

			    if (skObject is SKObjectMonster objectMonster)
			    {
				    if (SKObjectMonster.SKMonsterGradeType.Boss <= objectMonster.MonsterGrade)
				    {
					    continue;
				    }
			    }
			    else
			    {
				    continue;
			    }
			    
			    _destroyObjectsTemp.Add(skObject);
		    }

		    foreach (var skObject in _destroyObjectsTemp)
		    {
			    if (skObject is SKObjectMonster objectMonster)
			    {
				    objectMonster.ComponentAttackee.OnDeath();
			    }
		    }

		    _destroyObjectsTemp.Clear();
	    }

	    public int GetObjectCountByType(SKObject.SKObjectType objectType)
	    {
		    if (false == _objectByType.TryGetValue((int)objectType, out var objectBySpawnId))
		    {
			    return 0;
		    }

		    return objectBySpawnId.Count;
	    }

	    public SKObject GetObjectBySpawnId(ulong spawnId)
	    {
		    foreach (var it in _objectByType)
		    {
			    if (it.Value.TryGetValue(spawnId, out var skObject))
			    {
				    return skObject;
			    }
		    }

		    return null;
	    }
	    
	    public GameObject GetNearestMonster()
	    {
		    if (ObjectPlayer.IsNull())
		    {
			    return null;
		    }

		    var playerPosition = ObjectPlayer.transform.position;
		    
		    float nearestDistance = float.MaxValue;
		    GameObject nearestObject = null;
		    {
			    if (false == _objectByType.TryGetValue((int)SKObject.SKObjectType.Monster, out var objectBySpawnId))
			    {
				    return null;
			    }
			    
			    foreach (var (spawnId, skObject) in objectBySpawnId)
			    {
				    var distance = Vector3.Distance(playerPosition, skObject.transform.position);
				    if (distance < nearestDistance)
				    {
					    nearestDistance = distance;
					    nearestObject = skObject.gameObject;
				    }
			    }
		    }

		    // 랜덤 아이템 박스도 추적에 포함
		    {
			    if (false == _objectByType.TryGetValue((int)SKObject.SKObjectType.RandomItemBox, out var objectBySpawnId))
			    {
				    return nearestObject;
			    }
		    
			    foreach (var (spawnId, skObject) in objectBySpawnId)
			    {
				    var distance = Vector3.Distance(playerPosition, skObject.transform.position);
				    if (distance < nearestDistance)
				    {
					    nearestDistance = distance;
					    nearestObject = skObject.gameObject;
				    }
			    }
		    }

		    return nearestObject;
	    }

	    private List<(float distance, SKObject skObject)> _nearMonsterList = new();
	    public void GetNearestMonster(ref List<GameObject> refList)
	    {
		    if (false == _objectByType.TryGetValue((int)SKObject.SKObjectType.Monster, out var _objectBySpawnId))
		    {
			    return;
		    }

		    if (ObjectPlayer.IsNull())
		    {
			    return;
		    }

		    var playerPosition = ObjectPlayer.transform.position;
		    
		    foreach (var (spawnId, skObject) in _objectBySpawnId)
		    {
			    var distance = Vector3.Distance(playerPosition, skObject.transform.position);
			    _nearMonsterList.Add((distance, skObject));
		    }
		    
		    _nearMonsterList.Sort((a, b) => a.distance.CompareTo(b.distance));
		    foreach (var it in _nearMonsterList)
		    {
			    refList.Add(it.skObject.gameObject);
		    }
		    
		    _nearMonsterList.Clear();
	    }

	    public void OnMagnetCapture()
	    {
		    if (false == ObjectPlayer.TryGetSKComponent(out SKComponentDropItemCapture dropItemCapture))
		    {
			    return;
		    }
		    
		    if (false == _objectByType.TryGetValue((int)SKObject.SKObjectType.DropItem, out var objectBySpawnId))
		    {
			    return;
		    }
		    
		    foreach (var (spawnId, skObject) in objectBySpawnId)
		    {
			    if (skObject is SKObjectDropItem objectDropItem)
			    {
				    dropItemCapture.OnCollision(objectDropItem, true);
			    }
		    }
	    }
    }
}
