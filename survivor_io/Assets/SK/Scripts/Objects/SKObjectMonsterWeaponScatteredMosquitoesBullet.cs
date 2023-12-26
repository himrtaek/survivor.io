using JHT.Scripts.Common;
using UnityEngine;

namespace SK
{
    public class SKObjectMonsterWeaponScatteredMosquitoesBullet : SKObjectMonsterWeapon
    {
	    [SerializeField] private SKComponentSpawnerCommon spawnerBase;
	    
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    EventManager.AddListener(SKEventManager.SKEventType.Attack, OnReceiveEvent);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    EventManager.RemoveListener(SKEventManager.SKEventType.Attack, OnReceiveEvent);
		    
		    base.OnSKObjectDestroy();
	    }

	    private void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    switch (eventParam)
		    {
			    case SKAttackEventParam attackEventParam:
				    OnAttack(attackEventParam);
				    break;
		    }
	    }

	    private void OnAttack(SKAttackEventParam attackEventParam)
	    {
		    if (attackEventParam.Attackee.IsNull())
		    {
			    return;
		    }

		    if (attackEventParam.Attackee.SkObject)
		    {
			    
		    }

		    if (attackEventParam.Attackee.SkObject.HasTag(SKConstants.SKObjectTags.BossRingBlock))
		    {
			    spawnerBase.Init();
			    spawnerBase.SpawnerUpdate(0);
			    DestroyObject();
		    }
	    }
    }
}
