using System.Collections.Generic;

namespace SK
{
    public class SKTriggerManager
    {
	    private SKObject _skObject;
	    private ulong _lastTriggerKey;
	    private readonly Dictionary<ulong, SKTrigger> _triggerByKey = new();

	    public SKTriggerManager(SKObject skObject)
	    {
		    _skObject = skObject;
	    }

	    private ulong GenerateTriggerKey()
	    {
		    return ++_lastTriggerKey;
	    }

	    public ulong AddTrigger(ISKTriggerBuilder triggerBuilder)
	    {
		    var key = GenerateTriggerKey();
		    var trigger = triggerBuilder.Build(_skObject);
		    _triggerByKey.Add(key, trigger);
		    
		    return key;
	    }

	    public void RemoveTrigger(ulong triggerKey)
	    {
		    if (_triggerByKey.TryGetValue(triggerKey, out var trigger))
		    {
			    trigger.Clear();
		    
			    _triggerByKey.Remove(triggerKey);
		    }
	    }

	    public void Update(float deltaTime)
	    {
		    if (_triggerByKey.Count <= 0)
		    {
			    return;
		    }
		    
		    foreach (var it in _triggerByKey)
		    {
			    it.Value.Update(deltaTime);
		    }
	    }

	    public void Clear()
	    {
		    foreach (var it in _triggerByKey)
		    {
			    it.Value.Clear();
		    }
		    
		    _triggerByKey.Clear();
	    }
    }
}
