using JHT.Scripts.Common;
using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(SKComponentAttackee))]
    public class SKObjectRandomItemBox : SKObjectCreature
    {
	    public override SKObjectType ObjectType { get; } = SKObjectType.RandomItemBox;

	    [SerializeField] private uint dropItemPoolId;
	    
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    SkObject.EventManager.AddListener(SKEventManager.SKEventType.HPZero, OnReceiveEvent);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.EventManager.RemoveListener(SKEventManager.SKEventType.HPZero, OnReceiveEvent);
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    switch (eventParam)
		    {
			    case SKHPZeroEventParam hpZeroEventParam:
			    {
				    if (0 < dropItemPoolId)
				    {
					    SKObjectDropItem.SpawnDropItem(dropItemPoolId, transform.position);
				    }
			    }
				    break;
		    }
	    }

	    public static void SpawnDropItem(SKIngameItemType itemType, uint key, Vector3 position)
	    {
		    var filePath = SKSpawnSetting.GetFilePathByItemId(itemType, key);
		    var skObject = SKGameManager.Instance.ObjectManager.SpawnObject(filePath, null, false);
		    if (false == skObject.IsNull())
		    {
			    skObject.transform.position = position;
		    }
	    }
    }
}
