using System;
using System.Collections.Generic;
using Cysharp.Text;
using JHT.Scripts;
using JHT.Scripts.Common;
using JHT.Scripts.ResourceManager;
using UnityEngine;

namespace SK
{
    public class SKObjectPlayer : SKObjectCreature
    {
	    [SerializeField] private SKComponentDropItemCapture dropItemCapture;
	    
	    public override SKObjectType ObjectType { get; } = SKObjectType.Player;

	    public Dictionary<uint, (uint level, SKObject skObject)> PlayerWeaponByType { get; private set; } = new();
	    public Dictionary<uint, (uint level, SKSupportItem supportItemInfo)> SupportItemByType { get; private set; } = new();

	    private Transform _mainCameraTransform;
	    private Rect _camBoundRect;

	    protected override void Awake()
	    {
		    base.Awake();
		    
		    _mainCameraTransform = SceneManager.Instance.CurScene.MainCamera.transform;
		    _camBoundRect = SKGameManager.Instance.BackgroundManager.CameraBoundRect;
	    }

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    dropItemCapture.SkObject.EventManager.AddListener(SKEventManager.SKEventType.DropItemCapture, OnReceiveEvent);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    dropItemCapture.SkObject.EventManager.RemoveListener(SKEventManager.SKEventType.DropItemCapture, OnReceiveEvent);
		    
		    SKGameManager.Instance.GameOver();
		    
		    base.OnSKObjectDestroy();
	    }

	    public void AddGold(uint goldId)
	    {
		    SKGameManager.Instance.AddGold(goldId);
	    }

	    public void AddExp(uint expId)
	    {
		    SKGameManager.Instance.AddExp(expId);
	    }

	    public void AddWeapon(uint weaponId, uint level = 1)
	    {
		    if (PlayerWeaponByType.TryGetValue(weaponId, out var weaponInfo))
		    {
			    level = Math.Max(weaponInfo.level + 1, level);
			    RemoveWeapon(weaponId);
		    }
		    
		    var weaponPrefabPath = ZString.Concat("SKWeaponData_", weaponId, "/Weapon_", weaponId, "_", level);
		    var weaponSkObject = SKGameManager.Instance.ObjectManager.SpawnObject(weaponPrefabPath, this, true);
		    if (weaponSkObject)
		    {
			    PlayerWeaponByType.Add(weaponId, (level, weaponSkObject));   
		    }
	    }

	    public void RemoveWeapon(uint weaponId)
	    {
		    if (PlayerWeaponByType.TryGetValue(weaponId, out var weaponInfo))
		    {
			    if (false == weaponInfo.skObject.IsNull())
			    {
				    weaponInfo.skObject.DestroyObject();   
			    }
		    
			    PlayerWeaponByType.Remove(weaponId);
		    }
	    }

	    public void AddSupportItem(uint supportItemId, uint level = 1)
	    {
		    if (SupportItemByType.TryGetValue(supportItemId, out var supportItemInfoTemp))
		    {
			    level = supportItemInfoTemp.level + 1;
			    RemoveSupportItem(supportItemId);
		    }

		    var supportItemInfo = SKSupportItem.CreateSupportItem(this, supportItemId, level);
		    
		    supportItemInfo.DoAction();
		    
		    SupportItemByType.Add(supportItemId, (level, supportItemInfo));
	    }

	    public void RemoveSupportItem(uint supportItemId)
	    {
		    if (SupportItemByType.TryGetValue(supportItemId, out var supportItemInfo))
		    {
			    supportItemInfo.supportItemInfo.UnDoAction();
		    
			    SupportItemByType.Remove(supportItemId);
		    }
	    }

	    public override void GameUpdate(float deltaTime)
	    {
		    base.GameUpdate(deltaTime);

		    var playerPosition = ClampCameraPosition();
		    
		    _mainCameraTransform.position =
			    new Vector3(playerPosition.x, playerPosition.y, _mainCameraTransform.position.z);
	    }

	    private Vector3 ClampCameraPosition()
	    {
		    var cameraPosition = transform.position;
		    var cameraVisibleRect = SKGameManager.Instance.GetCameraVisibleRect(cameraPosition);
		    if (_camBoundRect.width <= cameraVisibleRect.width)
		    {
			    cameraPosition.x = 0;
		    }
		    else if (cameraVisibleRect.xMin < _camBoundRect.xMin)
		    {
			    cameraPosition.x += (_camBoundRect.xMin - cameraVisibleRect.xMin);
		    }
		    else if (_camBoundRect.xMax < cameraVisibleRect.xMax)
		    {
			    cameraPosition.x -= (cameraVisibleRect.xMax - _camBoundRect.xMax);
		    }
		    
		    if (_camBoundRect.height <= cameraVisibleRect.height)
		    {
			    cameraPosition.y = 0;
		    }
		    else if (cameraVisibleRect.yMin < _camBoundRect.yMin)
		    {
			    cameraPosition.y += (_camBoundRect.yMin - cameraVisibleRect.yMin);
		    }
		    else if (_camBoundRect.yMax < cameraVisibleRect.yMax)
		    {
			    cameraPosition.y -= (cameraVisibleRect.yMax - _camBoundRect.yMax);
		    }

		    return cameraPosition;
	    }

	    public void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    switch (eventParam)
		    {
			    case SKDropItemCaptureEventParam dropItemCaptureEventParam:
				    OnCaptureItem(dropItemCaptureEventParam.ObjectDropItem);
				    break;
		    }
	    }

	    public void OnCaptureItem(SKObjectDropItem objectDropItem)
	    {
		    OnCaptureItem(objectDropItem.ItemType, objectDropItem.ItemId);
	    }

	    public void OnCaptureItem(SKIngameItemType itemType, uint itemId)
	    {
		    switch (itemType)
		    {
			    case SKIngameItemType.Gold:
				    AddGold(itemId);
				    break;
			    case SKIngameItemType.Exp:
				    AddExp(itemId);
				    break;
			    case SKIngameItemType.Weapon:
				    AddWeapon(itemId);
				    break;
			    case SKIngameItemType.SupportItem:
				    AddSupportItem(itemId);
				    break;
			    case SKIngameItemType.Magnet:
			    {
				    SKGameManager.Instance.ObjectManager.OnMagnetCapture();
			    }
				    break;
			    case SKIngameItemType.Bomb:
			    {
				    SKPlayerWeaponSpawner.Spawn(new SKSpawnInfoBase()
				    {
					    gameObject = ResourceManager.Instance.LoadOriginalAsset<GameObject>("SKDropItemData/Bomb"),
					    isChild = false,
					    addParentPosition = true,
					    isLocalPosition = true,
					    isLocalRotation = true,
				    }, 0, false, this);
			    }
				    break;
			    case SKIngameItemType.Heal:
			    {
				    var maxHp = StatManager.GetStatResultValue(StatType.MaxHp);
				    var healPercent = 10;
				    var healValue = (ulong)(maxHp * (healPercent / 100.0f));
				    ComponentAttackee.Heal(healValue);
			    }
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }
    }
}
