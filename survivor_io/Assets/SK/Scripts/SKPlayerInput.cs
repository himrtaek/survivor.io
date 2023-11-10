using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SKPlayerInput : MonoBehaviour
{
    public float speed;
    public VariableJoystick variableJoystick;
    public SKPlayer skPlayer;

    public void Update()
    {
        Vector3 direction = Vector3.up * variableJoystick.Vertical + Vector3.right * variableJoystick.Horizontal;
        skPlayer.transform.Translate(direction * Time.deltaTime * speed);
    }
}
