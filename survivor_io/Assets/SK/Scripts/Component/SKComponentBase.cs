using System.Collections;
using System.Collections.Generic;
using JHT.Scripts.Common;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SKObjectBase))]
public class SKComponentBase : MonoBehaviour
{
    [SerializeField] private SKObjectBase skObject;

    public SKObjectBase SKObject
    {
        get
        {
            if (false == skObject)
            {
                if (TryGetComponent(out skObject).IsFalse())
                {
                    return null;
                }
            }

            return skObject;
        }
    }
}
