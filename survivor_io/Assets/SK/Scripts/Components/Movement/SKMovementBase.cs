using UnityEngine;

namespace SK
{
    public abstract class SKMovementBase : SKComponentFromDataBase
    {
	    [SerializeField] private bool useLocalPosition;
	    
	    protected void SetRotation(Vector3 direction)
	    {
		    if (useLocalPosition)
		    {
			    transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
		    }
		    else
		    {
			    transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
		    }
	    }
	    
	    protected void Rotate(float speed)
	    {
		    if (useLocalPosition)
		    {
			    transform.Rotate(Vector3.back * speed, Space.Self);
		    }
		    else
		    {
			    transform.Rotate(Vector3.back * speed, Space.World);
		    }
	    }
	    
	    protected void SetPosition(Vector3 position)
	    {
		    if (useLocalPosition)
		    {
			    transform.localPosition = position;   
		    }
		    else
		    {
			    transform.position = position;   
		    }
	    }

	    protected bool CanMove()
	    {
		    if (SkObject.BuffManager.ExistBuffByType(SKBuffType.FreezeMove))
		    {
			    return false;
		    }

		    return true;
	    }
	    
	    protected void Translate(Vector3 moveVector, float moveSpeed)
	    {
		    if (useLocalPosition)
		    {
			    var angleVector = Quaternion.AngleAxis(transform.localRotation.eulerAngles.z, Vector3.back);
			    transform.localPosition += angleVector * (moveVector * moveSpeed);
		    }
		    else
		    {
			    transform.position += transform.TransformDirection(moveVector * moveSpeed);
		    }
	    }
    }
}
