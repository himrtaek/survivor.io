using UnityEngine.Events;

namespace SK
{
    public class SKTriggerSKEventReceiveEvent : SKTriggerEvent
    {
	    private SKEventManager.SKEventType _skEventType;
	    private readonly long _count;
	    private long _elapsedCount;

	    private SKTriggerSKEventReceiveEvent(Builder conditionBuilder, SKObject skObject, UnityEvent onSatisfied) : base(SKTriggerEventType.SKEvent, conditionBuilder, skObject, onSatisfied)
	    {
		    _skEventType = conditionBuilder.EventType;
		    _count = conditionBuilder.Count;
		    
		    SKObject.EventManager.AddListener(_skEventType, OnReceiveEvent);
	    }

	    public override void Reset()
	    {
		    _elapsedCount = 0;
		    
		    base.Reset();
	    }

	    public override void Clear()
	    {
		    SKObject.EventManager.RemoveListener(_skEventType, OnReceiveEvent);
		    
		    base.Clear();
	    }
	    
	    public void OnReceiveEvent(SKObject skObject, SKEventParam eventParam)
	    {
		    if (eventParam.EventType == _skEventType)
		    {
			    _elapsedCount++;

			    if (_count <= _elapsedCount)
			    {
				    IsSatisfied = true;
			    }
		    }
	    }

	    public class Builder : SKTriggerEventBuilder
	    {
		    public SKEventManager.SKEventType EventType;
		    public long Count;

		    public Builder SetSKEvent(SKEventManager.SKEventType eventType)
		    {
			    EventType = eventType;
			    return this;
		    }

		    public Builder SetCount(long count)
		    {
			    Count = count;
			    return this;
		    }

		    protected override SKTriggerEvent BuildImpl(SKObject skObject, UnityEvent onSatisfied)
		    {
			    return new SKTriggerSKEventReceiveEvent(this, skObject, onSatisfied);
		    }
	    }
    }
}
