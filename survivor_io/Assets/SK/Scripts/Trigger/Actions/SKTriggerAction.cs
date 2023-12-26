
using System;

namespace SK
{
	public interface ISKTriggerActionBuilder
	{
		public SKTriggerAction Build(SKObject skObject);
	}
	
    public abstract class SKTriggerAction
    {
	    public enum SKTriggerActionType
	    {
		    Log,
		    Heal,
		    Spawn,
		    InitSpawner,
		    AddStat,
		    PlayAnim,
	    }

	    // 액션 타입
	    public SKTriggerActionType ActionType { get; private set; }
	    
	    // 대상 오브젝트
	    protected SKObject SkObject { get; private set; }
	    
	    // 딜레이
	    public float DelayTime { get; }

	    // 누적 시간(딜레이)
	    private float _elapsedTime;

	    // 예약 여부
	    public bool ReservedAction { get; private set; }

	    // 취소 여부
	    public bool ReservedCancel { get; private set; }
	    
	    // 죽기전에 무조건 실행되어야 하는 경우(생성은 됐지만 딜레이 사이에 클리어되는 경우 실행됨)
	    public bool ForceFireBeforeClear { get; }
	    
	    // 반복으로 인해 새 액션이 실행되기 이전에 무조건 실행되어야 하는 경우(생성은 됐지만 딜레이 사이에 또 트리거 되는 경우 실행됨)
	    public bool ForceFireBeforeRepeatFire { get; }

	    protected SKTriggerAction(SKTriggerActionType actionType, SKTriggerActionBuilder actionBuilder, SKObject skObject)
	    {
		    ActionType = actionType;
		    SkObject = skObject;
		    DelayTime = actionBuilder.Delay;
		    ForceFireBeforeClear = actionBuilder.ForceFireBeforeClear;
		    ForceFireBeforeRepeatFire = actionBuilder.ForceFireBeforeRepeatFire;
	    }

	    public void Update(float deltaTime)
	    {
		    _elapsedTime += deltaTime;
	    }

	    public virtual void Clear()
	    {
		    SkObject = null;
		    _elapsedTime = 0;
		    ReservedAction = false;
		    ReservedCancel = false;
	    }
	    
	    public abstract void DoAction();
	    
	    public bool CanDoActionToReserved() { return true == ReservedAction && DelayTime <= _elapsedTime; }

	    // 행동 예약
	    public void ReserveAction() { ReservedAction = true; }

	    [Serializable]
	    public abstract class SKTriggerActionBuilder : ISKTriggerActionBuilder
	    {
		    public float Delay;
		    public bool ForceFireBeforeClear;
		    public bool ForceFireBeforeRepeatFire;

		    public SKTriggerActionBuilder SetDelay(float delay)
		    {
			    Delay = delay;
			    return this;
		    }
		    
		    public SKTriggerActionBuilder SetForceFireBeforeClear(bool forceFireBeforeClear)
		    {
			    ForceFireBeforeClear = forceFireBeforeClear;
			    return this;
		    }
		    
		    public SKTriggerActionBuilder SetForceFireBeforeRepeatFire(bool forceFireBeforeRepeatFire)
		    {
			    ForceFireBeforeRepeatFire = forceFireBeforeRepeatFire;
			    return this;
		    }

		    protected abstract SKTriggerAction BuildImpl(SKObject skObject);

		    public SKTriggerAction Build(SKObject skObject)
		    {
			    return BuildImpl(skObject);
		    }
	    }
    }
}
