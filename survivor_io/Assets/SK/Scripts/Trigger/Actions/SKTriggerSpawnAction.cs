using Unity.VisualScripting;

namespace SK
{
    public class SKTriggerSpawnAction : SKTriggerAction
    {
	    private SKSpawnInfo _spawnInfo;
	    
	    private SKTriggerSpawnAction(Builder actionBuilder, SKObject skObject) : base(SKTriggerActionType.Spawn, actionBuilder, skObject)
	    {
		    _spawnInfo = actionBuilder.SpawnInfo;
	    }
	    
	    public override void DoAction()
	    {
		    SKSpawner.Spawn(_spawnInfo, SkObject);
	    }

	    public class Builder : SKTriggerActionBuilder
	    {
		    public SKSpawnInfo SpawnInfo;

		    public Builder SetSpawnInfo(SKSpawnInfo spawnInfo)
		    {
			    SpawnInfo = spawnInfo;
			    return this;
		    }

		    protected override SKTriggerAction BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerSpawnAction(this, skObject);
		    }
	    }
    }
}
