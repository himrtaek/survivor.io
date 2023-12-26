using System;
using System.Collections.Generic;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.GameObjectPool;
using UnityEditor;
using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(PoolableObject))]
	[DisallowMultipleComponent]
    public abstract class SKObject : MonoBehaviour, ISKObjectData, ISKObjectLifeCycle
    {
	    #region Cache

	    [SerializeField] private PoolableObject poolableObject;

	    public PoolableObject PoolableObject
	    {
		    get
		    {
			    if (false == poolableObject)
			    {
				    TryGetComponent(out poolableObject);
			    }
			    
			    return poolableObject;
		    }
	    }

	    #endregion
	    
	    #region Update
	    public virtual bool AutoEnableGameUpdate => true;
	    
	    private bool _enableGameUpdate;
	    public virtual bool EnableGameUpdate => _enableGameUpdate;

	    public void SetEnableGameUpdate(bool value, bool onlyValueChange = false)
	    {
		    if (_enableGameUpdate != value)
		    {
			    if (false == onlyValueChange)
			    {
				    if (value)
				    {
					    SKGameManager.Instance.ObjectManager.AddUpdateObjectList(this);
				    }
				    else
				    {
					    SKGameManager.Instance.ObjectManager.RemoveUpdateObjectList(this);
				    }
			    }

			    _enableGameUpdate = value;
		    }
	    }
	    #endregion
	    
	    #region SKObjectType
	    public enum SKObjectType
	    {
		    Player,
		    PlayerWeapon,
		    Monster,
		    MonsterWeapon,
		    DropItem,
		    Spawner,
		    PlayerWeaponSpawner,
		    Deco,
		    DecoCollision,
		    RandomItemBox,
	    }
	    
	    public abstract SKObjectType ObjectType { get; }
	    #endregion
	    
	    #region SKObjectStateType
	    public enum SKObjectStateType
	    {
		    ReadyForSpawn,
		    Spawned,
		    ReadyForDestroy,
		    Destroyed,
	    }

	    public SKObjectStateType ObjectState { get; private set; }

	    public void ChangeState(SKObjectStateType objectStateType)
	    {
		    var beforeType = ObjectState;
		    ObjectState = objectStateType;
        
		    var eventParam = SKEventParam.GetOrNewParam<SKObjectStateChangeEventParam>();
		    eventParam.BeforeType = beforeType;
		    eventParam.AfterType = ObjectState;
		    EventManager.BroadCast(SKEventManager.SKEventType.StateChange, eventParam);
	    }
	    #endregion

	    #region SpawnParent
	    public ulong SpawnObjectSpawnId { get; set; }
	    public SKObject SpawnParentObject { get; set; }
	    public List<SKObject> SpawnChildObjectList { get; } = new();

	    public void AddChildObject(SKObject childObject)
	    {
		    SpawnChildObjectList.Add(childObject);
	    }
	    
	    public void RemoveChildObject(SKObject childObject)
	    {
		    SpawnChildObjectList.Remove(childObject);
	    }

	    #endregion

	    #region Tag
	    [SerializeField] private List<string> objectTagList = new();

	    public bool HasTag(string objectTag)
	    {
		    if (objectTagList == null)
		    {
			    return false;
		    }
		    
		    foreach (var objectTagTemp in objectTagList)
		    {
			    if (objectTagTemp == objectTag)
			    {
				    return true;
			    }
		    }

		    return false;
	    }

	    #endregion
	    
	    #region SKStatOverrideType
	    public enum SKStatOverrideType
	    {
		    Auto,
		    Sync,
		    Copy,
	    }

	    [SerializeField] private SKStatOverrideType statOverrideType;
	    public SKStatOverrideType StatOverrideType => statOverrideType;
	    #endregion

	    #region Stat
	    private SKStatManager _statManager;
	    public SKStatManager StatManager => _statManager ??= new(this);
	    #endregion

	    #region Stat
	    private SKBuffManager _buffManager;
	    public SKBuffManager BuffManager => _buffManager ??= new(this);
	    #endregion
	    
	    #region Trigger
	    private SKTriggerManager _triggerManager;
	    public SKTriggerManager TriggerManager => _triggerManager ??= new(this);

	    #endregion

	    #region Event
	    private SKEventManager _eventManager;
	    public SKEventManager EventManager => _eventManager ??= new(this);
	    #endregion

	    #region InstantMovement
	    private readonly List<int> _skInstantMovementRemoveTempList = new();
	    private readonly List<SKInstantMovementBase> _skInstantMovementList = new();

	    public void AddInstantMovement<T>(T movement) where T : SKInstantMovementBase
	    {
		    movement.OnStart();
		    _skInstantMovementList.Add(movement);
		    
		    _skInstantMovementList.Sort((movement1, movement2) =>
		    {
			    var compareTo = movement2.Priority.CompareTo(movement1.Priority);
			    if (compareTo != 0)
			    {
				    return compareTo;
			    }
			    else
			    {
				    return movement2.CreateTime.CompareTo(movement1.CreateTime);
			    }
		    });
	    }
	    
	    public bool TryGetInstantMovement<T>(out T movement) where T : SKInstantMovementBase
	    {
		    foreach (var instantMovementBase in _skInstantMovementList)
		    {
			    if (instantMovementBase.GetType() == typeof(T))
			    {
				    movement = instantMovementBase as T;
				    return true;
			    }
		    }

		    movement = null;
		    return false;
	    }

	    public bool RemoveInstantMovement<T>(T movement) where T : SKInstantMovementBase
	    {
		    movement.OnEnd();
		    return _skInstantMovementList.Remove(movement);
	    }

	    #endregion

	    #region SkComponent
	    private readonly Dictionary<Type, List<SKComponentBase>> _skComponentByType = new();

	    public void AddSKComponent<T>(T component) where T : SKComponentBase
	    {
		    var componentType = component.GetType();
		    if (false == _skComponentByType.TryGetValue(componentType, out var skComponentList))
		    {
			    skComponentList = new();
			    _skComponentByType.Add(componentType, skComponentList);
		    }
		    
		    skComponentList.Add(component);
	    }
	    
	    public bool TryGetSKComponent<T>(out T component) where T : SKComponentBase
	    {
		    var componentType = typeof(T);
		    if (_skComponentByType.TryGetValue(componentType, out var componentList) && 0 < componentList?.Count)
		    {
			    component = componentList[0] as T;
			    return true;
		    }
		    
		    component = null;
		    return false;
	    }

	    #endregion

	    #region DataID
	    public SKObject SkObject => this;
	    public GameObject MyObject => gameObject;

	    [SerializeField] private string dataObjectName;
	    public string DataObjectName => dataObjectName;
	    
	    [SerializeField] private uint dataID;
	    private uint _overrideDataId;
	    public uint DataID => 0 < _overrideDataId ? _overrideDataId : dataID;

	    public void OverrideDataID(uint dataId)
	    {
		    _overrideDataId = dataId;
	    }

	    public uint DataSubKey => 0;

		#if UNITY_EDITOR
	    public void ChangeSerializeDataObjectName(string newDataObjectName)
	    {
		    dataObjectName = newDataObjectName;
		    EditorUtility.SetDirty(gameObject);
	    }
	    
	    public void ChangeSerializeDataID(uint newDataID)
	    {
		    dataID = newDataID;
		    EditorUtility.SetDirty(gameObject);
	    }
	    #endif

	    #endregion

	    #region Spawn
	    public ulong SpawnId { get; private set; }
	    public void SetSpawnId(ulong spawnId)
	    {
		    SpawnId = spawnId;
	    }
	    #endregion

	    public void SerializeFieldCopyTo(SKObject skObject)
	    {
		    skObject.dataObjectName = dataObjectName;
		    skObject.dataID = dataID;
		    skObject.objectTagList = new(objectTagList);
	    }
	    
	    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	    protected virtual void Reset()
	    {
		    if(false == poolableObject)
		    {
			    TryGetComponent(out poolableObject);
		    }
	    }
	    
	    protected virtual void Awake()
	    {
		    var objectLayerError = false;
		    var objectLayerName = LayerMask.LayerToName(gameObject.layer);
		    var spawnerLayerName = "Spawner";
		    if (objectLayerName == spawnerLayerName)
		    {
			    switch (ObjectType)
			    {
				    case SKObjectType.Spawner:
				    case SKObjectType.PlayerWeaponSpawner:
					    break;
				    default:
					    objectLayerError = true;
					    break;
			    }
		    }
		    else
		    {
			    if (ObjectType.ToStringCached() != objectLayerName)
			    {
				    objectLayerError = true;
			    }
		    }
		    
		    if (objectLayerError)
		    {
			    Debug.LogError($"{gameObject.name} 오브젝트의 레이어가 잘못 설정되었습니다. {objectLayerName}");
		    }
	    }

	    #region LifeCycle
	    public virtual void OnSKObjectSpawn()
	    {
		    for (var i = 0; i < _skInstantMovementList.Count; i++)
		    {
			    var instantMovement = _skInstantMovementList[i];
			    instantMovement.OnSKObjectSpawn();
		    }

		    foreach (var it in _skComponentByType)
		    {
			    foreach (var it2 in it.Value)
			    {
				    it2.OnSKObjectSpawn();   
			    }
		    }
	    }

	    public virtual void OnSKObjectReadyForDestroy()
	    {
		    for (var i = 0; i < _skInstantMovementList.Count; i++)
		    {
			    var instantMovement = _skInstantMovementList[i];
			    instantMovement.OnSKObjectReadyForDestroy();
		    }

		    foreach (var it in _skComponentByType)
		    {
			    foreach (var it2 in it.Value)
			    {
				    it2.OnSKObjectReadyForDestroy();   
			    }
		    }
	    }
	    
	    public virtual void OnSKObjectDestroy()
	    {
		    for (var i = 0; i < _skInstantMovementList.Count; i++)
		    {
			    var instantMovement = _skInstantMovementList[i];
			    instantMovement.OnSKObjectDestroy();
			    instantMovement.OnEnd();
		    }

		    foreach (var it in _skComponentByType)
		    {
			    foreach (var it2 in it.Value)
			    {
				    it2.OnSKObjectDestroy();   
			    }
		    }
		    
		    _skInstantMovementList.Clear();
	    }
	    
	    public virtual void GameUpdate(float deltaTime)
	    {
		    var ignoreOtherMovement = false;
		    for (var i = 0; i < _skInstantMovementList.Count; i++)
		    {
			    var instantMovement = _skInstantMovementList[i];
			    instantMovement.AddElapsedTime(deltaTime);
			    
			    if (0 < instantMovement.LifeTime && instantMovement.LifeTime < instantMovement.ElapsedTime)
			    {
				    _skInstantMovementRemoveTempList.Add(i);
			    }

			    if (ignoreOtherMovement)
			    {
				    continue;
			    }
			    
			    instantMovement.GameUpdate(deltaTime);

			    if (instantMovement.IgnoreOtherMovement)
			    {
				    ignoreOtherMovement = true;
			    }
		    }

		    for (int i = _skInstantMovementRemoveTempList.Count - 1; i >= 0; i--)
		    {
			    RemoveInstantMovement(_skInstantMovementList[i]);
		    }
		    
		    _skInstantMovementRemoveTempList.Clear();

		    if (0 < _skComponentByType.Count)
		    {
			    foreach (var it in _skComponentByType)
			    {
				    foreach (var it2 in it.Value)
				    {
					    if (ignoreOtherMovement && it2 is SKMovementBase movementBase)
					    {
						    continue;
					    }
					    
					    it2.GameUpdate(deltaTime);   
				    }
			    }
		    }
		    
		    TriggerManager.Update(deltaTime);
		    BuffManager.Update(deltaTime);
	    }
	    #endregion
	    
	    public void DestroyObject(float delay = 0)
	    {
		    SKGameManager.Instance.ObjectManager.DestroyObject(this, delay);
	    }
    }
}
