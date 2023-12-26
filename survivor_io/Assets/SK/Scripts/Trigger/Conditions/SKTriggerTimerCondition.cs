using Unity.VisualScripting;

namespace SK
{
    public class SKTriggerTimerCondition : SKTriggerCondition
    {
	    private readonly float _time;
	    private float _elapsedTime;

	    private SKTriggerTimerCondition(Builder conditionBuilder, SKObject skObject) : base(SKTriggerConditionType.Timer, conditionBuilder, skObject)
	    {
		    _time = conditionBuilder.Time;
	    }
	    
	    public override void Update(float deltaTIme)
	    {
		    UpdateWithoutSatisfied(deltaTIme);

		    if (_time <= _elapsedTime)
		    {
			    IsSatisfied = true;
		    }
	    }
	    
	    public override void UpdateWithoutSatisfied(float deltaTIme)
	    {
		    _elapsedTime += deltaTIme;
	    }
	    
	    public override void Reset()
	    {
		    _elapsedTime = 0;
	    }

	    public class Builder : SKTriggerConditionBuilder
	    {
		    public float Time;

		    public Builder SetTime(float time)
		    {
			    Time = time;
			    return this;
		    }

		    protected override SKTriggerCondition BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerTimerCondition(this, skObject);
		    }
	    }
    }
}
