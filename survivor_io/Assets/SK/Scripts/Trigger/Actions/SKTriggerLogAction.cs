using Unity.VisualScripting;
using UnityEngine;

namespace SK
{
    public class SKTriggerLogAction : SKTriggerAction
    {
	    private readonly string _message;
	    
	    private SKTriggerLogAction(Builder actionBuilder, SKObject skObject) : base(SKTriggerActionType.Log, actionBuilder, skObject)
	    {
		    _message = actionBuilder.Message;
	    }
	    
	    public override void DoAction()
	    {
		    Debug.Log(_message);
	    }

	    public class Builder : SKTriggerActionBuilder
	    {
		    public string Message;

		    public Builder SetMessage(string message)
		    {
			    Message = message;
			    return this;
		    }

		    protected override SKTriggerAction BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerLogAction(this, skObject);
		    }
	    }
    }
}
