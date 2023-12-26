using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(SKObjectCreature))]
    public abstract class SKCreatureMovementBase : SKMovementBase
    {
	    #region Cache
	    
	    private SKObjectCreature _skObjectCreature;
	    public SKObjectCreature SkObjectCreature
	    {
		    get
		    {
			    if (false == _skObjectCreature)
			    {
				    _skObjectCreature = SkObject as SKObjectCreature;
			    }
			    
			    return _skObjectCreature;
		    }
	    }

	    #endregion

	    protected void Translate(Rigidbody2D rigidBody2d, Vector3 moveVector, float moveSpeed)
	    {
		    var newPosition = rigidBody2d.position + (Vector2)moveVector * moveSpeed;
		    rigidBody2d.MovePosition(newPosition);
	    }
    }
}
