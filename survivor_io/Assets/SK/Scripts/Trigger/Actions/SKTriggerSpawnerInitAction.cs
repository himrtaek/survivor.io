using Unity.VisualScripting;

namespace SK
{
    public class SKTriggerSpawnerInitAction : SKTriggerAction
    {
	    private SKComponentSpawnerCommon _spawner;
	    
	    private SKTriggerSpawnerInitAction(Builder actionBuilder, SKObject skObject) : base(SKTriggerActionType.InitSpawner, actionBuilder, skObject)
	    {
		    _spawner = actionBuilder.Spawner;
	    }
	    
	    public override void DoAction()
	    {
		    _spawner.Init();
		    _spawner.SpawnerUpdate(0);
	    }

	    public class Builder : SKTriggerActionBuilder
	    {
		    public SKComponentSpawnerCommon Spawner;

		    public Builder SetSpawner(SKComponentSpawnerCommon spawner)
		    {
			    Spawner = spawner;
			    return this;
		    }

		    protected override SKTriggerAction BuildImpl(SKObject skObject)
		    {
			    return new SKTriggerSpawnerInitAction(this, skObject);
		    }
	    }
    }
}
