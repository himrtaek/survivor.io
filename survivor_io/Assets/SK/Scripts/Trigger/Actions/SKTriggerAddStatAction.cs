using Unity.VisualScripting;

namespace SK
{
    public class SKTriggerAddStatAction : SKTriggerAction
    {
	    private StatType _statType;
	    private StatExprType _exprType;
	    private StatSourceType _sourceType;
	    private long _key;
	    private long _value;
	    
	    private SKTriggerAddStatAction(Builder actionBuilder, SKObject skObject) : base(SKTriggerActionType.AddStat, actionBuilder, skObject)
	    {
		    _statType = actionBuilder.StatType;
		    _exprType = actionBuilder.ExprType;
		    _sourceType = actionBuilder.SourceType;
		    _key = actionBuilder.StatKey;
		    _value = actionBuilder.StatValue;
	    }
	    
	    public override void DoAction()
	    {
		    SkObject.StatManager.AddStatData(_statType, _exprType, _sourceType, _key, _value);
	    }

	    public class Builder : SKTriggerActionBuilder
	    {
		    public StatType StatType;
		    public StatExprType ExprType;
		    public StatSourceType SourceType;
		    public long StatKey;
		    public long StatValue;

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
		    
		    public Builder SetStatValue(long statValue)
		    {
			    StatValue = statValue;
			    return this;
		    }

		    protected override SKTriggerAction BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerAddStatAction(this, skObject);
		    }
	    }
    }
}
