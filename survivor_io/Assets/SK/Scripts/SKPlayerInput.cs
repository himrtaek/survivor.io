using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SKPlayerInput : MonoBehaviour
{
    public float speed;
    public VariableJoystick variableJoystick;
    private SKObjectPlayer _objectPlayer;

    public void Init(SKObjectPlayer objectPlayer)
    {
        this._objectPlayer = objectPlayer;
    }

    public void Update()
    {
        Vector3 direction = Vector3.up * variableJoystick.Vertical + Vector3.right * variableJoystick.Horizontal;
        _objectPlayer.transform.Translate(direction * Time.deltaTime * speed);
    }
}
