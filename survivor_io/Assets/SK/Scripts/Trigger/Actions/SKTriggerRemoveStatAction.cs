using Unity.VisualScripting;

namespace SK
{
    public class SKTriggerRemoveStatAction : SKTriggerAction
    {
	    private StatType _statType;
	    private StatExprType _exprType;
	    private StatSourceType _sourceType;
	    private long _key;
	    
	    private SKTriggerRemoveStatAction(Builder actionBuilder, SKObject skObject) : base(SKTriggerActionType.AddStat, actionBuilder, skObject)
	    {
		    _statType = actionBuilder.StatType;
		    _exprType = actionBuilder.ExprType;
		    _sourceType = actionBuilder.SourceType;
		    _key = actionBuilder.StatKey;
	    }
	    
	    public override void DoAction()
	    {
		    SkObject.StatManager.RemoveStatData(_statType, _exprType, _sourceType, _key);
	    }

	    public class Builder : SKTriggerActionBuilder
	    {
		    public StatType StatType;
		    public StatExprType ExprType;
		    public StatSourceType SourceType;
		    public long StatKey;

		    public Builder SetStatType(StatType statType)
		    {
			    StatType = statType;
			    return this;
		    }
		    
		    public Builder SetExprType(StatExprType exprType)
		    {
			    ExprType = exprType;
			    return this;
		    }
		    
		    public Builder SetSourceType(StatSourceType sourceType)
		    {
			    SourceType = sourceType;
			    return this;
		    }
		    
		    public Builder SetStatKey(long key)
		    {
			    StatKey = key;
			    return this;
		    }

		    protected override SKTriggerAction BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerRemoveStatAction(this, skObject);
		    }
	    }
    }
}
