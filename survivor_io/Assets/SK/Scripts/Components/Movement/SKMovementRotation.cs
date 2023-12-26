using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SKMovementRotation : SKMovementBase
    {
	    [SerializeField] private float rotationSpeed;
	    
	    protected override void ImportFieldFromData()
	    {
		    
	    }

	    protected override List<string> ExportFieldToData()
	    {
		    return null;
	    }

        public override void GameUpdate(float deltaTime)
        {
	        base.GameUpdate(deltaTime);
	        
	        float speed = deltaTime * rotationSpeed;
	        Rotate(speed);
        }
    }
}
