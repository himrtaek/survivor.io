using UnityEngine;
using UnityEngine.Events;

namespace SK
{
    public class SKPlayerInput : MonoBehaviour
    {
	    [SerializeField] private VariableJoystick variableJoystick;

	    public UnityEvent<Vector3> OnInputDirection { get; private set; } = new();
	    
	    private bool _init;
	    
        public void Init()
        {
	        _init = true;
	        
	        SKGameManager.Instance.OnGameFixedUpdate -= (OnGameFixedUpdate);
	        SKGameManager.Instance.OnGameFixedUpdate += (OnGameFixedUpdate);
        }

        void OnGameFixedUpdate(float deltaTime)
        {
	        if (false == _init)
	        {
		        return;
	        }
	        
	        if (Mathf.Abs(variableJoystick.Vertical) + Mathf.Abs(variableJoystick.Horizontal) < SKConstants.PlayerInputThreshold)
	        {
		        return;
	        }
	        
	        Vector3 direction = (Vector3.up * variableJoystick.Vertical + Vector3.right * variableJoystick.Horizontal).normalized;
	        if (direction == Vector3.zero)
	        {
		        return;
	        }
	        
	        OnInputDirection.Invoke(direction);
        }
    }
}
