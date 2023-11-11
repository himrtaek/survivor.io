using System.Collections;
using System.Collections.Generic;
using JHT.Scripts.Common;
using UnityEngine;

public class SKObjectComponentBase : MonoBehaviour
{
    [SerializeField] private SKObject _skObject;

    public SKObject SKObject
    {
        get
        {
            if (false == _skObject)
            {
                if (TryGetComponent(out _skObject).IsFalse())
                {
                    return null;
                }
            }

            return _skObject;
        }
    }
}
