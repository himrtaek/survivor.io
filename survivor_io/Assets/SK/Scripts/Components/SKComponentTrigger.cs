using UnityEngine;

namespace SK
{
    public class SKComponentTrigger : SKComponentBase
    {
	    [SerializeReference] private SKTrigger.SKTriggerBuilder triggerBuilder;

	    private ulong _triggerKey;

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    _triggerKey = SkObject.TriggerManager.AddTrigger(triggerBuilder);
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    SkObject.TriggerManager.RemoveTrigger(_triggerKey);
		    _triggerKey = 0;
		    
		    base.OnSKObjectDestroy();
	    }
    }
}
