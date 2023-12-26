using JHT.Scripts.Common;

namespace SK
{
    public class SKComponentBlackholeAttack : SKComponentBase
    {
	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    SkObject.EventManager.AddListener(SKEventManager.SKEventType.Attack, OnReceiveEvent);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.EventManager.RemoveListener(SKEventManager.SKEventType.Attack, OnReceiveEvent);

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

		    if (attackEventParam.Attackee.SkObject is SKObjectMonster objectMonster
		        && objectMonster.TryGetSKComponent(out SKMovementFollow movementFollow)
		        && objectMonster.MonsterGrade <= SKObjectMonster.SKMonsterGradeType.Rare)
		    {
			    objectMonster.BuffManager.AddBuff(SKBuffType.FreezeMove, new SKBuffOption(duration:1));

			    if (false == objectMonster.TryGetInstantMovement(out SKInstantMovementBlackHole _))
			    {
				    var instantMovementBlackHole = new SKInstantMovementBlackHole(objectMonster, objectMonster.gameObject, transform.position, 2, 360);
				    instantMovementBlackHole.LifeTime = 0.5f;
				    objectMonster.AddInstantMovement(instantMovementBlackHole);
			    }
		    }
	    }
    }
}
