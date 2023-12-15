using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SKMovementRotation : SKMovementBase
{
    [SerializeField] private float rotationSpeed;
    
    void Awake()
    {
        SKObject.OnGameUpdate.AddListener(GameUpdate);
    }
    
    private void GameUpdate(float deltaTime)
    {
        transform.Rotate(Vector3.forward, rotationSpeed * deltaTime);
    }
}
