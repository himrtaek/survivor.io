using System;
using System.Collections.Generic;
using JHT.Scripts.Common.PerformanceExtension;
using UnityEngine;

namespace SK
{
    public class SKTargetHelper
    {
	    public enum SKTargetType
	    {
		    None = -1,
		    Player,
		    NearestMonster
	    }
	    
	    public enum SKObjectUpdateType
	    {
		    Once,
		    Realtime,
	    }
	    
	    public enum SKPositionUpdateType
	    {
		    Once,
		    Realtime,
	    }
	    
	    [Flags]
	    public enum SKConstraintFlag
	    {
		    None = 0,
		    
		    Angle45 = 1 << 0,
	    }

	    public SKTargetType TargetType
	    {
		    get;
		    private set;
	    }
	    
	    public SKObjectUpdateType ObjectUpdateType
	    {
		    get;
		    private set;
	    }
	    
	    public SKPositionUpdateType PositionUpdateType
	    {
		    get;
		    private set;
	    }

	    public SKConstraintFlag ConstraintFlag
	    {
		    get;
		    private set;
	    }

	    public GameObject TargetObject
	    {
		    get;
		    private set;
	    }

	    public GameObject MyObject
	    {
		    get;
		    private set;
	    }

	    private Vector3 _prevTargetPosition;
	    private List<GameObject> _targetListTemp = new();

	    public void SetData(GameObject myObject, SKTargetType targetType, SKObjectUpdateType objectUpdateType, SKPositionUpdateType positionUpdateType, SKConstraintFlag constraint = SKConstraintFlag.None)
	    {
		    MyObject = myObject;
		    TargetType = targetType;
		    ObjectUpdateType = objectUpdateType;
		    PositionUpdateType = positionUpdateType;
		    ConstraintFlag = constraint;
		    UpdateTargetObject();
		    
		    if (TargetObject)
		    {
			    _prevTargetPosition = TargetObject.transform.position;   
		    }
	    }

	    private void UpdateTargetObject()
	    {
		    TargetObject = SearchTargetObject(TargetType);
	    }
	    
	    private GameObject SearchTargetObject(SKTargetType targetType)
	    {
		    switch (targetType)
		    {
			    case SKTargetType.None:
				    break;
			    case SKTargetType.Player:
				    _targetListTemp.Add(SKGameManager.Instance.ObjectManager.ObjectPlayer.gameObject);
				    break;
			    case SKTargetType.NearestMonster:
				    if (ConstraintFlag == SKConstraintFlag.None)
				    {
					    _targetListTemp.Add(SKGameManager.Instance.ObjectManager.GetNearestMonster());
				    }
				    else
				    {
					    SKGameManager.Instance.ObjectManager.GetNearestMonster(ref _targetListTemp);   
				    }
				    break;
			    default:
				    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
		    }

		    GameObject result = null;
		    if (ConstraintFlag == SKConstraintFlag.None && 0 < _targetListTemp.Count)
		    {
			    result = _targetListTemp[0];
			    _targetListTemp.Clear();
			    return result;
		    }
		    
		    foreach (var targetObject in _targetListTemp)
		    {
			    if (ConstraintFlag.HasFlagNonAlloc(SKConstraintFlag.Angle45))
			    {
				    var direction = (targetObject.transform.position - MyObject.transform.position).normalized;
				    var rotation = Quaternion.FromToRotation(Vector3.up, direction);
				    var rotationGap = Mathf.Abs(rotation.eulerAngles.z - MyObject.transform.rotation.eulerAngles.z);
				    if (22.5f < rotationGap)
				    {
					    continue;
				    }
			    }

			    result = targetObject;
			    break;
		    }

		    _targetListTemp.Clear();
		    return result;
	    }

	    public Vector3 GetDirection(Vector3 myPosition)
	    {
		    var targetPosition = GetTargetPosition();
		    return (targetPosition - myPosition).normalized;
	    }
	    
	    public Vector3 GetDirectionWithNearCheck(Vector3 myPosition, float nearDistance = 0.1f)
	    {
		    var targetPosition = GetTargetPosition();
		    if (Vector3.Distance(targetPosition, myPosition) <= nearDistance)
		    {
			    return Vector3.zero;
		    }
		    
		    return (targetPosition - myPosition).normalized;
	    }

	    public Vector3 GetTargetPosition()
	    {
		    switch (ObjectUpdateType)
		    {
			    case SKObjectUpdateType.Once:
				    break;
			    case SKObjectUpdateType.Realtime:
				    UpdateTargetObject();
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }

		    switch (PositionUpdateType)
		    {
			    case SKPositionUpdateType.Once:
				    return _prevTargetPosition;
			    case SKPositionUpdateType.Realtime:
				    if (TargetObject)
				    {
					    return TargetObject.transform.position;
				    }
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }

		    return _prevTargetPosition;
	    }
    }
}
