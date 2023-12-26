using JHT.Scripts.Common;
using Unity.VisualScripting;

namespace SK
{
    public class SKTriggerPlayAnimAction : SKTriggerAction
    {
	    private SKViewAnimNameType _animName;
	    private SKViewAnimNameType _animNameNext;
	    
	    private SKTriggerPlayAnimAction(Builder actionBuilder, SKObject skObject) : base(SKTriggerActionType.PlayAnim, actionBuilder, skObject)
	    {
		    _animName = actionBuilder.PlayAnimName;
		    _animNameNext = actionBuilder.PlayAnimNameNext;
	    }
	    
	    public override void DoAction()
	    {
		    if (SkObject.TryGetSKComponent(out SKComponentObjectView objectView).IsFalse())
		    {
			    return;    
		    }
		    
		    objectView.PlayAnim(_animName, _animNameNext);
	    }

	    public class Builder : SKTriggerActionBuilder
	    {
		    public SKViewAnimNameType PlayAnimName;
		    public SKViewAnimNameType PlayAnimNameNext;

		    public Builder SetAnimName(SKViewAnimNameType playAnimName)
		    {
			    PlayAnimName = playAnimName;
			    return this;
		    }
		    
		    public Builder SetAnimNameNext(SKViewAnimNameType playAnimNameNext)
		    {
			    PlayAnimNameNext = playAnimNameNext;
			    return this;
		    }

		    protected override SKTriggerAction BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerPlayAnimAction(this, skObject);
		    }
	    }
    }
}
