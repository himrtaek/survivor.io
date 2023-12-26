using System;
using System.Collections.Generic;
using JHT.Scripts.Common;


namespace SK
{
	public class SKBuffOption
	{
		public float Duration { get; set; }
		public uint Priority { get; private set; }
		public bool AddRemainDuration { get; private set; }

		public SKBuffOption(float duration)
		{
			Duration = duration;
		}

		public SKBuffOption SetPriority(uint priority)
		{
			Priority = priority;
			return this;
		}

		public SKBuffOption SetAddRemainDuration(bool addRemainDuration)
		{
			AddRemainDuration = addRemainDuration;
			return this;
		}
	}
	
	public class SKBuffManager
	{
		private SKObject _skObject;

		public SKBuffManager(SKObject skObject)
		{
			_skObject = skObject;
		}

		private Dictionary<SKBuffType, (SKBuffLogic buffLogic, SKBuffOption buffOption)> _buffInfoByBuffType = new();
		private List<SKBuffType> _removeGrouIdTempList = new();

		public bool AddBuff(SKBuffType buffType, SKBuffOption buffOption)
		{
			if (_buffInfoByBuffType.TryGetValue(buffType, out var buffInfo))
			{
				if (buffOption.Priority < buffInfo.buffOption.Priority)
				{
					// 이미 더 우선순위 높은게 걸려있음
					return false;
				}
				
				if (buffInfo.buffOption.Priority == buffOption.Priority)
				{
					if (buffOption.AddRemainDuration)
					{
						// 시간 연장
						buffOption.Duration += Math.Max(buffInfo.buffOption.Duration - buffInfo.buffLogic.ElapsedTime, 0);
					}
				}
				
				buffInfo.buffLogic.OnEnd();
				_buffInfoByBuffType.Remove(buffType);
			}
			
			var buffBase = SKBuffLogicFactory.Create(buffType, _skObject);
			if (buffBase.IsNull())
			{
				return false;
			}

			buffBase.OnStart();
			_buffInfoByBuffType.Add(buffType, (buffBase, buffOption));

			return true;
		}

		public void Update(float deltaTime)
		{
			_removeGrouIdTempList.Clear();

			foreach (var it in _buffInfoByBuffType)
			{
				it.Value.buffLogic.Update(deltaTime);
				it.Value.buffLogic.ElapsedTime += deltaTime;

				if (it.Value.buffOption.Duration <= it.Value.buffLogic.ElapsedTime)
				{
					_removeGrouIdTempList.Add(it.Key);
				}
			}

			for (int i = _removeGrouIdTempList.Count - 1; i >= 0; i--)
			{
				var removeBuffType = _removeGrouIdTempList[i];
				var buffInfo = _buffInfoByBuffType[removeBuffType];
				buffInfo.buffLogic.OnEnd();
				
				_buffInfoByBuffType.Remove(removeBuffType);
			}
		}

		public bool ExistBuffByType(SKBuffType buffType)
		{
			return _buffInfoByBuffType.ContainsKey(buffType);
		}
	}	
}
