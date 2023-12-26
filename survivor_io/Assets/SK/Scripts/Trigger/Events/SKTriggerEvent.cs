using System;
using UnityEngine.Events;

namespace SK
{
	public interface ISKTriggerEventBuilder
	{
		public SKTriggerEvent Build(SKObject skObject, UnityEvent onSatisfied);
	}
	
    public class SKTriggerEvent
    {
	    public enum SKTriggerEventType
	    {
		    SKEvent,
	    };
	    
	    public SKTriggerEventType EventType { get; private set; }
	    
	    // 대상 오브젝트
	    protected SKObject SKObject { get; private set; }

	    // 통과 콜백
	    private UnityEvent _onSatisfied;
	    
	    // 통과 여부
	    private bool _isSatisfied;
	    
	    public bool IsSatisfied
	    {
		    get => _isSatisfied;
		    protected set
		    {
			    _isSatisfied = value;
			    if (_isSatisfied)
			    {
				    _onSatisfied.Invoke();   
			    } 
		    }
	    }
	    
	    // 통과 여부 초기화
	    public void ResetSatisfied() { IsSatisfied = false; }

	    protected SKTriggerEvent(SKTriggerEventType eventType, SKTriggerEventBuilder eventBuilder, SKObject skObject, UnityEvent onSatisfied)
	    {
		    EventType = eventType;
		    SKObject = skObject;
		    _onSatisfied = onSatisfied;
	    }
	    
	    // 통과 후 초기화 시
	    public virtual void Reset()
	    {
			
	    }

	    // 삭제 시
	    public virtual void Clear()
	    {
		    SKObject = null;
		    _onSatisfied = null;
	    }

	    [Serializable]
	    public abstract class SKTriggerEventBuilder : ISKTriggerEventBuilder
	    {
		    protected abstract SKTriggerEvent BuildImpl(SKObject skObject, UnityEvent onSatisfied);

		    public SKTriggerEvent Build(SKObject skObject, UnityEvent onSatisfied)
		    {
			    return BuildImpl(skObject, onSatisfied);
		    }
	    }
    }
}
