using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public abstract class SKTriggerAndOrCondition : SKTriggerCondition
    {
	    protected List<SKTriggerCondition> ConditionList { get; private set; }

	    protected SKTriggerAndOrCondition(SKTriggerConditionType conditionType, AndOrBuilder conditionBuilder, SKObject skObject) : base(conditionType, conditionBuilder, skObject)
	    {
		    ConditionList = new();

		    foreach (var conditionBuilderTemp in conditionBuilder.ConditionBuilderList)
		    {
			    ConditionList.Add(conditionBuilderTemp.Build(SKObject));
		    }
	    }
	    
	    public override void Reset()
	    {
		    foreach (var condition in ConditionList)
		    {
			    condition.Reset();
		    }
		    
		    base.Reset();
	    }
	    
	    public override void Clear()
	    {
		    foreach (var condition in ConditionList)
		    {
			    condition.Clear();
		    }
		    
		    base.Clear();
	    }
	    
	    public override void Update(float deltaTime)
	    {
		    foreach (var condition in ConditionList)
		    {
			    condition.Update(deltaTime);
		    }
	    }
	    
	    public override void UpdateWithoutSatisfied(float deltaTime)
	    {
		    foreach (var condition in ConditionList)
		    {
			    condition.UpdateWithoutSatisfied(deltaTime);
		    }
	    }
	    
	    public override void ResetSatisfied()
	    {
		    base.ResetSatisfied();
		    
		    foreach (var condition in ConditionList)
		    {
			    condition.ResetSatisfied();
		    }
	    }
    }

    public abstract class AndOrBuilder : SKTriggerCondition.SKTriggerConditionBuilder
    {
	    [SerializeReference] public List<ISKTriggerConditionBuilder> ConditionBuilderList;

	    public AndOrBuilder AddCondition(ISKTriggerConditionBuilder conditionBuilder)
	    {
		    ConditionBuilderList.Add(conditionBuilder);
		    return this;
	    }
    }
    
    public class SKTriggerAndCondition : SKTriggerAndOrCondition
    {
	    private SKTriggerAndCondition(Builder conditionBuilder, SKObject skObject) : base(SKTriggerConditionType.And, conditionBuilder, skObject)
	    {
		    
	    }
	    
	    public override void Update(float deltaTime)
	    {
		    bool bAllTrue = 0 < ConditionList.Count;
		    foreach (var condition in ConditionList)
		    {
			    condition.Update(deltaTime);
			    if (false == condition.IsSatisfied)
			    {
				    bAllTrue = false;
			    }
		    }

		    if (true == bAllTrue)
		    {
			    IsSatisfied = true;
		    }
	    }

	    public class Builder : AndOrBuilder
	    {
		    protected override SKTriggerCondition BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerAndCondition(this, skObject);
		    }
	    }
    }
    
    public class SKTriggerOrCondition : SKTriggerAndOrCondition
    {
	    private SKTriggerOrCondition(Builder conditionBuilder, SKObject skObject) : base(SKTriggerConditionType.Or, conditionBuilder, skObject)
	    {
		    
	    }
	    
	    public override void Update(float deltaTime)
	    {
		    foreach (var condition in ConditionList)
		    {
			    condition.Update(deltaTime);
			    if (condition.IsSatisfied)
			    {
				    IsSatisfied = true;
			    }
		    }
	    }

	    public class Builder : AndOrBuilder
	    {
		    protected override SKTriggerCondition BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerOrCondition(this, skObject);
		    }
	    }
    }
}
