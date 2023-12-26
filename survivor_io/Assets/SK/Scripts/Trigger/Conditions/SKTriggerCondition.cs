using System;

namespace SK
{
	public interface ISKTriggerConditionBuilder
	{
		public SKTriggerCondition Build(SKObject skObject);
	}
	
	public abstract class SKTriggerCondition
	{
		public enum SKTriggerConditionType
		{
			And,
			Or,
			Timer,
		};

		public SKTriggerConditionType ConditionType { get; private set; }
	    
		protected SKObject SKObject { get; private set; }

		protected SKTriggerCondition(SKTriggerConditionType conditionType, SKTriggerConditionBuilder conditionBuilder, SKObject skObject)
		{
			ConditionType = conditionType;
			SKObject = skObject;
			IsNotCondition = conditionBuilder.IsNotCondition;
		}

		// 초기화
		public virtual void Reset()
		{
			
		}

		// 삭제
		public virtual void Clear()
		{
			SKObject = null;
		}

		// 업데이트(폴링)
		public abstract void Update(float deltaTime);

		public virtual void UpdateWithoutSatisfied(float deltaTime)
		{
			
		}

		// 통과 여부
		private bool _isSatisfied;
		public bool IsSatisfied {
			get
			{
				if (true == IsNotCondition)
				{
					return !_isSatisfied;
				}

				return _isSatisfied;
			}
			protected set => _isSatisfied = value;
		}

		// 통과 여부 초기화
		public virtual void ResetSatisfied() { _isSatisfied = false; }

		// Not 조건 여부
		public bool IsNotCondition { get; private set; }

		[Serializable]
		public abstract class SKTriggerConditionBuilder : ISKTriggerConditionBuilder
		{
			public bool IsNotCondition;

			public SKTriggerConditionBuilder SetIsNotCondition(bool isNotCondition)
			{
				IsNotCondition = isNotCondition;
				return this;
			}
			
			protected abstract SKTriggerCondition BuildImpl(SKObject skObject);

			public SKTriggerCondition Build(SKObject skObject)
			{
				return BuildImpl(skObject);
			}
		}
	}
}
