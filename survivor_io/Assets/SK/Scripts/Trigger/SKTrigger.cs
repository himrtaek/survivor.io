using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SK
{
	public interface ISKTriggerBuilder
	{
		public SKTrigger Build(SKObject skObject);
	}
	
    public class SKTrigger
    {
	    // 대상 오브젝트
	    private readonly SKObject _skObject;
	    
	    // 조건 트리의 최상단에 위치한 조건
	    private readonly SKTriggerEvent _rootEvent;
	    private readonly SKTriggerCondition _rootCondition;

	    // 액션 관련 리스트
	    private readonly List<SKTriggerAction.SKTriggerActionBuilder> _actionBuilderList;
	    private readonly List<SKTriggerAction> _actionList = new();
	    private readonly List<int> _removeActionIndexTempForUpdate = new();
	    private readonly List<int> _removeActionIndexTempForSatisfied = new();

	    // 이벤트 통과 시 콜백
	    private UnityEvent _onEventSatisfied = new();

	    // 반복 여부
	    readonly bool _isRepeat;

	    // 반복 최대 횟수
	    readonly int _repeatMaxCount;

	    // 반복 대기 시간
	    readonly float _repeatCoolTime;

	    // 발동 처리 했는지 여부
	    bool _isFired;

	    // 반복 누적 횟수
	    int _repeatCount;

	    // 반복 대기 누적 시간
	    float _elapsedTimeForRepeat;

	    private SKTrigger(SKObject skObject, SKTriggerBuilder triggerBuilder)
	    {
		    _onEventSatisfied.AddListener(UpdateForEvent);
		    
		    _skObject = skObject;
		    _isRepeat = triggerBuilder.IsRepeat;
		    _repeatMaxCount = triggerBuilder.RepeatMaxCount;
		    _repeatCoolTime = triggerBuilder.RepeatCoolTime;
		    _rootEvent = triggerBuilder.EventBuilder?.Build(skObject, _onEventSatisfied);
		    _rootCondition = triggerBuilder.ConditionBuilder?.Build(skObject);
		    _actionBuilderList = new(triggerBuilder.ActionBuilderList);
	    }

	    // 삭제 시
	    public void Clear()
	    {
		    foreach (var action in _actionList)
		    {
			    if (action.ReservedCancel)
			    {
				    continue;
			    }

			    if (false == action.ForceFireBeforeClear)
			    {
				    continue;
			    }
			    
			    action.DoAction();
		    }
		    
		    _onEventSatisfied.RemoveListener(UpdateForEvent);
		    
		    _rootEvent?.Clear();
		    _rootCondition?.Clear();
		    _actionBuilderList?.Clear();

		    foreach (var action in _actionList)
		    {
			    action.Clear();
		    }

		    _actionList.Clear();
		    _removeActionIndexTempForUpdate.Clear();
		    _removeActionIndexTempForSatisfied.Clear();
	    }

	    // 통과 후 초기화 시
	    private void Reset()
	    {
		    _rootEvent?.Reset();
		    _rootCondition?.Reset();

		    _isFired = false;
		    _elapsedTimeForRepeat = 0;
	    }

	    private void UpdateForEvent()
	    {
		    // 업데이트 확인
		    Update(.0f, true);
	    }

	    public void Update(float deltaTime, bool bOnReceiveEvent = false)
	    {
		    // 1. 예약된 액션중에 수행 가능한게 있는지 확인
		    {
			    for (var index = 0; index < _actionList.Count; index++)
			    {
				    var action = _actionList[index];
				    if (true == action.ReservedCancel)
				    {
					    action.Clear();
				    
					    _removeActionIndexTempForUpdate.Add(index);
				    }
				    else
				    {
					    action.Update(deltaTime);

					    if (true == action.CanDoActionToReserved())
					    {
						    action.DoAction();
						    action.Clear();
					    
						    _removeActionIndexTempForUpdate.Add(index);
					    }
				    }
			    }

			    for (var index = _removeActionIndexTempForUpdate.Count - 1; index >= 0; index--)
			    {
				    _actionList.RemoveAt(index);
			    }
		    
			    _removeActionIndexTempForUpdate.Clear();
		    }

		    // 2. 반복 여부 확인
			if (true == _isFired)
			{
				_elapsedTimeForRepeat += deltaTime;
				
				if ((true == _isRepeat && 0 <= _repeatCoolTime && _elapsedTimeForRepeat < _repeatCoolTime) ||
					(true == _isRepeat && 0 <= _repeatMaxCount && _repeatMaxCount < _repeatCount) ||
					(false == _isRepeat))
				{
					return;
				}
				else
				{
					Reset();
				}
			}

			// 3. 조건 상태 갱신
			var satisfyCondition = true;
			if (null != _rootEvent)
			{
				// 조건 통과 여부 확인
				satisfyCondition = _rootEvent.IsSatisfied;
				_rootEvent.ResetSatisfied();
			}

			if (null != _rootCondition)
			{
				if (true == satisfyCondition)
				{
					// 전체 조건 업데이트
					_rootCondition.Update(deltaTime);

					// 조건 통과 여부 확인
					satisfyCondition = _rootCondition.IsSatisfied;
					_rootCondition.ResetSatisfied();
				}
				else
				{
					// 무조건 업데이트해야하는 조건만 업데이트
					// 어차피 이벤트에서 실패했으므로 조건 통과 여부는 체크하지 않는다
					_rootCondition.UpdateWithoutSatisfied(deltaTime);
					_rootCondition.ResetSatisfied();
				}
			}

			// 4. 조건 통과 여부 전달
			if (true == satisfyCondition)
			{
				CheckForceFireBeforeRepeatFire();
				NotifyConditionSatisfied();
			}
	    }

	    private void CheckForceFireBeforeRepeatFire()
	    {
		    for (var i = 0; i < _actionList.Count; i++)
		    {
			    var action = _actionList[i];
			    if (action.ReservedCancel)
			    {
				    continue;
			    }

			    if (action.ForceFireBeforeRepeatFire)
			    {
				    action.DoAction();
				    action.Clear();
				    _removeActionIndexTempForSatisfied.Add(i);
			    }
		    }

		    for (var i = _removeActionIndexTempForSatisfied.Count - 1; i >= 0; i--)
		    {
			    _actionList.RemoveAt(i);
		    }

		    _removeActionIndexTempForSatisfied.Clear();
	    }

	    private void NotifyConditionSatisfied()
		{	
			// TriggerAction 리스트 
			CreateActionList();

			for (var index = 0; index < _actionList.Count; index++)
			{
				var action = _actionList[index];
				if (true == action.ReservedCancel)
				{
					action.Clear();

					_removeActionIndexTempForSatisfied.Add(index);
					continue;
				}

				if (true == action.ReservedAction)
				{
					continue;
				}

				var iDelayTime = action.DelayTime;
				if (iDelayTime > 0)
				{
					action.ReserveAction();
				}
				else
				{
					action.DoAction();
					action.Clear();
					
					_removeActionIndexTempForSatisfied.Add(index);
				}
			}

			for (var i = _removeActionIndexTempForSatisfied.Count - 1; i >= 0; i--)
			{
				_actionList.RemoveAt(i);
			}
			
			_removeActionIndexTempForSatisfied.Clear();

			_isFired = true;
		}

	    private bool CreateActionList()
	    {
		    var success = true;
		    foreach (var actionBuilder in _actionBuilderList)
		    {
			    var action = actionBuilder.Build(_skObject);
				if (null == action)
				{
					success = false;
				}
				else
				{
					_actionList.Add(action);
				}
			}

			if (false == success)
			{
				foreach (var action in _actionList)
				{
					action.Clear();
				}
				
				_actionList.Clear();
			}

			return true == success && 0 < _actionList.Count;
	    }

	    [Serializable]
	    public class SKTriggerBuilder : ISKTriggerBuilder
	    {
		    public bool IsRepeat;
		    public int RepeatMaxCount;
		    public float RepeatCoolTime;
		    [SerializeReference] public SKTriggerEvent.SKTriggerEventBuilder EventBuilder;
		    [SerializeReference] public SKTriggerCondition.SKTriggerConditionBuilder ConditionBuilder;
		    [SerializeReference] public List<SKTriggerAction.SKTriggerActionBuilder> ActionBuilderList = new();

		    public SKTriggerBuilder SetIsRepeat(bool isRepeat)
		    {
			    IsRepeat = isRepeat;
			    return this;
		    }

		    public SKTriggerBuilder SetRepeatMaxCount(int repeatMaxCount)
		    {
			    RepeatMaxCount = repeatMaxCount;
			    return this;
		    }

		    public SKTriggerBuilder SetRepeatCoolTime(int repeatCoolTime)
		    {
			    RepeatCoolTime = repeatCoolTime;
			    return this;
		    }
		    
		    public SKTriggerBuilder SetEvent(SKTriggerEvent.SKTriggerEventBuilder eventBuilder)
		    {
			    EventBuilder = eventBuilder;
			    return this;
		    }
		    
		    public SKTriggerBuilder SetCondition(SKTriggerCondition.SKTriggerConditionBuilder conditionBuilder)
		    {
			    ConditionBuilder = conditionBuilder;
			    return this;
		    }
		    
		    public SKTriggerBuilder AddAction(SKTriggerAction.SKTriggerActionBuilder actionBuilder)
		    {
			    ActionBuilderList.Add(actionBuilder);
			    return this;
		    }

		    public SKTrigger Build(SKObject skObject)
		    {
			    return new SKTrigger(skObject, this);
		    }
	    }
    }
}
