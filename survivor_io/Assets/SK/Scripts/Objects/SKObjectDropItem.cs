using System;
using System.Collections.Generic;
using System.Data.Common;
using JHT.Scripts.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SK
{
	[RequireComponent(typeof(Collider2D))]
    public class SKObjectDropItem : SKObject
    {
	    #region Cache

	    [SerializeField] private Collider2D _collider2D;
	    public Collider2D Collider2D
	    {
		    get
		    {
			    if (false == _collider2D)
			    {
				    TryGetComponent(out _collider2D);
			    }
			    
			    return _collider2D;
		    }
	    }
	    
	    [SerializeField] private SpriteRenderer _spriteRenderer;
	    public SpriteRenderer SpriteRenderer
	    {
		    get
		    {
			    if (false == _spriteRenderer)
			    {
				    TryGetComponent(out _spriteRenderer);
			    }
			    
			    return _spriteRenderer;
		    }
	    }
	    
	    protected override void Reset()
	    {
		    base.Reset();
		    
		    if(false == _collider2D)
		    {
			    TryGetComponent(out _collider2D);
		    }
	    }

	    #endregion

	    public override bool AutoEnableGameUpdate => false;
	    
	    public override SKObjectType ObjectType { get; } = SKObjectType.DropItem;

	    public enum SKDropItemState
	    {
		    Idle,
		    Effect1,
		    Effect2,
		    Destroyed
	    }
	    
	    private SKDropItemState _dropItemState;
	    public SKDropItemState DropItemState => _dropItemState;

	    [SerializeField] private SKIngameItemType _itemType;
	    public SKIngameItemType ItemType => _itemType;
	    
	    [SerializeField] private uint _itemId;
	    public uint ItemId => _itemId;
	    
	    [SerializeField] private bool canMagnetCapture;
	    
	    [SerializeField] private float _effect1MoveTime = 0.15f;
	    [SerializeField] private float _effect2MoveTime = 0.15f;

	    private bool _init;
	    private SKTargetHelper _targetHelper = new();
	    private SKComponentDropItemCapture _dropItemCapture;
	    private Vector3 _moveStartPosition;
	    private Vector3 _moveEndPosition;
	    private float _moveTime;
	    private float _elapsedMoveTime;

	    protected override void Awake()
	    {
		    base.Awake();
		    
		    _targetHelper.SetData(gameObject, SKTargetHelper.SKTargetType.Player, SKTargetHelper.SKObjectUpdateType.Once, SKTargetHelper.SKPositionUpdateType.Realtime);
	    }

	    public bool Init(SKIngameItemType itemType, uint key)
	    {
		    _itemType = itemType;
		    _itemId = key;
		    _init = true;

		    return true;
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    Collider2D.enabled = true;
		    _dropItemState = SKDropItemState.Idle;
		    _elapsedMoveTime = 0;
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    _init = false;
		    
		    base.OnSKObjectDestroy();
	    }

	    public bool CanCapture(SKComponentDropItemCapture dropItemCapture, bool isMagnet)
	    {
		    if (false == _init)
		    {
			    return false;
		    }
		    
		    if (isMagnet && false == canMagnetCapture)
		    {
			    return false;
		    }
		    
		    if (SKDropItemState.Idle != _dropItemState)
		    {
			    return false;
		    }
		    
		    if (SKObjectStateType.ReadyForDestroy <= ObjectState)
		    {
			    return false;
		    }
	        
		    return true;
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);
		    
		    switch (_dropItemState)
		    {
			    case SKDropItemState.Idle:
				    break;
			    case SKDropItemState.Effect1:
				    SlideMove(deltaTime);
				    break;
			    case SKDropItemState.Effect2:
				    FollowPlayer(deltaTime);
				    break;
			    case SKDropItemState.Destroyed:
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    private void SlideMove(float deltaTime)
	    {
		    _elapsedMoveTime += deltaTime;
		    transform.position = Vector3.Lerp(_moveStartPosition, _moveEndPosition, _elapsedMoveTime /_moveTime);

		    if (Vector3.Distance(transform.position, _moveEndPosition) < 0.01f || _moveTime <= _elapsedMoveTime)
		    {
			    _moveStartPosition = transform.position;
			    _moveEndPosition = SKGameManager.Instance.ObjectManager.ObjectPlayer.transform.position;
			    _elapsedMoveTime = 0;
			    var distance = Vector3.Distance(_moveStartPosition, _moveEndPosition);
			    _moveTime = _effect2MoveTime * (distance / 1.5f);
			    _dropItemState = SKDropItemState.Effect2;
			    return;
		    }
	    }

	    private void FollowPlayer(float deltaTime)
	    {
		    _elapsedMoveTime += deltaTime;
		    _moveEndPosition = SKGameManager.Instance.ObjectManager.ObjectPlayer.transform.position;
		    transform.position = Vector3.Lerp(_moveStartPosition, _moveEndPosition, _elapsedMoveTime / _moveTime);

		    if (Vector3.Distance(transform.position, _moveEndPosition) < 0.01f || _moveTime <= _elapsedMoveTime)
		    {
			    OnEffectEnd();
		    }
	    }
	    
        public void OnCaptured(SKComponentDropItemCapture dropItemCapture)
        {
	        _dropItemCapture = dropItemCapture;
	        Collider2D.enabled = false;
	        
	        Vector3 direction = _targetHelper.GetDirectionWithNearCheck(transform.position);
	        if (direction == Vector3.zero)
	        {
		        OnEffectEnd();
	        }
	        else
	        {
		        SetEnableGameUpdate(true);
		        
		        var targetPosition = _targetHelper.GetTargetPosition();
		        _moveStartPosition = transform.position;
		        _moveEndPosition = _moveStartPosition + (_moveStartPosition - targetPosition).normalized * 1.0f;
		        _moveTime = _effect1MoveTime;
		        _dropItemState = SKDropItemState.Effect1;
	        }
        }

        public void OnEffectEnd()
        {
	        if (_dropItemCapture.IsNull())
	        {
		        return;
	        }
	        
	        var eventParam = SKEventParam.GetOrNewParam<SKDropItemCaptureEventParam>();
	        eventParam.ObjectDropItem = this;
	        _dropItemCapture.SkObject.EventManager.BroadCast(SKEventManager.SKEventType.DropItemCapture, eventParam);
	        
	        /*N2Logger<LogCombat>.Log($"{_dropItemCapture.name}이 [Type:{ItemType.ToStringCached()}] [Id:{ItemId.ToStringCached()}] 아이템 획득");*/
		        
	        _dropItemState = SKDropItemState.Destroyed;
	        DestroyObject();
        }

        public class DROPITEMPOOL
        {
	        public int ItemId;
	        public int ItemKey;
	        public int Probability;
        }

        private static Dictionary<uint, List<DROPITEMPOOL>> _dropItemPoolListByKey = new();

        public static void SpawnDropItem(uint dropItemPoolId, Vector3 position)
        {
	        int SelectKeyIndex (List<DROPITEMPOOL> probs) {

		        float total = 0;

		        foreach (var elem in probs) {
			        total += elem.Probability;
		        }

		        float randomPoint = Random.value * total;

		        for (int i= 0; i < probs.Count; i++) {
			        if (randomPoint < probs[i].Probability) {
				        return i;
			        }
			        else {
				        randomPoint -= probs[i].Probability;
			        }
		        }
		        return probs.Count - 1;
	        }

	        if (false == _dropItemPoolListByKey.TryGetValue(dropItemPoolId, out var dropItemPoolList))
	        {
		        dropItemPoolList = new();

		        switch (dropItemPoolId)
		        {
			        case 1:
				        dropItemPoolList.Add(new DROPITEMPOOL()
				        {
					        ItemId = (int)SKIngameItemType.Exp,
					        ItemKey = 1,
					        Probability = 1,
				        });
				        break;
			        case 2:
				        dropItemPoolList.Add(new DROPITEMPOOL()
				        {
					        ItemId = (int)SKIngameItemType.Magnet,
					        ItemKey = 1,
					        Probability = 1,
				        });
				        break;
		        }
		        
		        _dropItemPoolListByKey.Add(dropItemPoolId, dropItemPoolList);
	        }

	        var keyIndex = SelectKeyIndex(dropItemPoolList);
	        var selectedDropItemPool = dropItemPoolList[keyIndex];
	        if (selectedDropItemPool.ItemId < 0)
	        {
		        return;
	        }
	        
	        SpawnDropItem((SKIngameItemType)selectedDropItemPool.ItemId, (uint)selectedDropItemPool.ItemKey, position);
        }

        public static void SpawnDropItem(SKIngameItemType itemType, uint key, Vector3 position)
        {
	        var filePath = SKSpawnSetting.GetFilePathByItemId(itemType, key);
	        var skObject = SKGameManager.Instance.ObjectManager.SpawnObject(filePath, null, false);
	        if (false == skObject.IsNull())
	        {
		        if (false == skObject.TryGetComponent(out SKObjectDropItem skObjectDropItem).IsFalse())
		        {
			        skObjectDropItem.Init(itemType, key);
		        }
			    
		        skObject.transform.position = position;
	        }
        }
    }
}
