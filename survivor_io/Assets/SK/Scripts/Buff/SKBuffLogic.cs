using System;


namespace SK
{
	public enum SKBuffType
	{
		FreezeMove,
	}

	public static class SKBuffLogicFactory
	{
		public static SKBuffLogic Create(SKBuffType buffType, SKObject skObject)
		{
			return buffType switch
			{
				SKBuffType.FreezeMove => new SkBuffLogicEmpty(buffType, skObject),
				_ => throw new ArgumentOutOfRangeException(nameof(buffType), buffType, null)
			};
		}
	}
	
    public abstract class SKBuffLogic
    {
	    public SKBuffType BuffType { get; private set; }
	    public SKObject SkObject { get; private set; }
	    public float ElapsedTime { get; set; }
	    
	    protected SKBuffLogic(SKBuffType buffType, SKObject skObject)
	    {
		    BuffType = buffType;
		    SkObject = skObject;
	    }

	    public virtual void OnStart()
	    {
		    
	    }

	    public virtual void Update(float deltaTime)
	    {
		    
	    }
	    
	    public virtual void OnEnd()
	    {
		    
	    }
    }

    public class SkBuffLogicEmpty: SKBuffLogic
    {
	    public SkBuffLogicEmpty(SKBuffType buffType, SKObject skObject) : base(buffType, skObject)
	    {
		    
	    }
    }
}
