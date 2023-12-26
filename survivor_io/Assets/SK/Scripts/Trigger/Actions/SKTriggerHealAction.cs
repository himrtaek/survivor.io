using Unity.VisualScripting;
using UnityEngine;

namespace SK
{
    public class SKTriggerHealAction : SKTriggerAction
    {
	    private uint _percent;
	    
	    private SKTriggerHealAction(Builder actionBuilder, SKObject skObject) : base(SKTriggerActionType.Heal, actionBuilder, skObject)
	    {
		    _percent = actionBuilder.Percent;
	    }
	    
	    public override void DoAction()
	    {
		    if (SkObject is SKObjectCreature skObjectCreature)
		    {
			    var maxHp = SkObject.StatManager.GetStatResultValue(StatType.MaxHp);
			    var healValue = (ulong)(maxHp * (_percent / 100.0f));
			    skObjectCreature.ComponentAttackee.Heal(healValue);
		    }
		    else
		    {
			    Debug.LogError($"{SkObject.name}은 크리쳐 오브젝트가 아닙니다");
		    }
	    }

	    public class Builder : SKTriggerActionBuilder
	    {
		    public uint Percent;

		    public Builder SetHealPercent(uint percent)
		    {
			    Percent = percent;
			    return this;
		    }

		    protected override SKTriggerAction BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerHealAction(this, skObject);
		    }
	    }
    }
}
