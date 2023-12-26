using System;
using System.Collections.Generic;
using UnityEngine;


namespace SK
{
	public abstract class SKEventParam
	{
		protected SKEventParam(SKEventManager.SKEventType eventType)
		{
			EventType = eventType;
		}
		
		public SKEventManager.SKEventType EventType { get; private set; }
	    private static Dictionary<Type, Queue<SKEventParam>> _eventParamPoolByType = new();
	    
	    public static T GetOrNewParam<T>() where T : SKEventParam
	    {
	        var paramType = typeof(T);
	        if (false == _eventParamPoolByType.TryGetValue(paramType, out var eventParamPool))
	        {
	            eventParamPool = new();
	            _eventParamPoolByType.Add(paramType, eventParamPool);
	        }
	        
	        if (eventParamPool.TryDequeue(out var result))
	        {
	            return result as T;
	        }

	        return Activator.CreateInstance(paramType) as T;
	    }

	    public static void Return(SKEventParam eventParam)
	    {
	        var paramType = eventParam.GetType();
	        if (false == _eventParamPoolByType.TryGetValue(paramType, out var eventParamPool))
	        {
	            eventParamPool = new();
	            _eventParamPoolByType.Add(paramType, eventParamPool);
	        }
	        
	        eventParam.Reset();
	        eventParamPool.Enqueue(eventParam);
	    }

	    public abstract void Reset();
	}

	public class SKReadyForSpawnEventParam : SKEventParam
	{
		public SKReadyForSpawnEventParam() : base(SKEventManager.SKEventType.ReadyForSpawn)
		{
			
		}
		
		public ulong SpawnId;
	    
		public override void Reset()
		{
			SpawnId = 0;
		}
	}

	public class SKSpawnEventParam : SKEventParam
	{
		public SKSpawnEventParam() : base(SKEventManager.SKEventType.Spawn)
		{
			
		}
		
	    public ulong SpawnId;
	    
	    public override void Reset()
	    {
	        SpawnId = 0;
	    }
	}

	public class SKDestroyEventParam : SKEventParam
	{
		public SKDestroyEventParam() : base(SKEventManager.SKEventType.Destroy)
		{
			
		}
		
		public override void Reset()
		{
			
		}
	}

	public class SKObjectStateChangeEventParam : SKEventParam
	{
		public SKObjectStateChangeEventParam() : base(SKEventManager.SKEventType.StateChange)
		{
			
		}
		
	    public SKObject.SKObjectStateType BeforeType;
	    public SKObject.SKObjectStateType AfterType;
	    
	    public override void Reset()
	    {
	        BeforeType = SKObject.SKObjectStateType.ReadyForSpawn;
	        AfterType = SKObject.SKObjectStateType.ReadyForSpawn;
	    }
	}	

	public class SKDamageEventParam : SKEventParam
	{
		public SKDamageEventParam() : base(SKEventManager.SKEventType.Damage)
		{
			
		}
		
		public SKComponentAttacker Attacker;
		public float DamageOfHp;
		public float DamageOfShield;
		public uint DamageOfShieldCount;
	    
		public override void Reset()
		{
			Attacker = null;
			DamageOfHp = 0;
			DamageOfShield = 0;
			DamageOfShieldCount = 0;
		}
	}	

	public class SKAttackEventParam : SKEventParam
	{
		public SKAttackEventParam() : base(SKEventManager.SKEventType.Attack)
		{
			
		}
		
		public SKComponentAttackee Attackee;
		public ulong Damage;
	    
		public override void Reset()
		{
			Attackee = null;
			Damage = 0;
		}
	}	

	public class SKDropItemCaptureEventParam : SKEventParam
	{
		public SKDropItemCaptureEventParam() : base(SKEventManager.SKEventType.DropItemCapture)
		{
			
		}
		
		public SKObjectDropItem ObjectDropItem;
	    
		public override void Reset()
		{
			ObjectDropItem = null;
		}
	}	

	public class SKChangeMoveDirectionEventParam : SKEventParam
	{
		public SKChangeMoveDirectionEventParam() : base(SKEventManager.SKEventType.ChangeMoveDirection)
		{
			
		}
		
		public Vector3 MoveVector;
	    
		public override void Reset()
		{
			MoveVector = Vector3.zero;
		}
	}

	public class SKKillEventParam : SKEventParam
	{
		public SKKillEventParam() : base(SKEventManager.SKEventType.Kill)
		{
			
		}
		
		public SKComponentAttackee Attackee;
		public float DamageOfHp;
		public float DamageOfShield;
		public uint DamageOfShieldCount;
	    
		public override void Reset()
		{
			Attackee = null;
			DamageOfHp = 0;
			DamageOfShield = 0;
			DamageOfShieldCount = 0;
		}
	}	

	public class SKHPZeroEventParam : SKEventParam
	{
		public SKHPZeroEventParam() : base(SKEventManager.SKEventType.HPZero)
		{
			
		}
		
		public SKComponentAttackee Attackee;
	    
		public override void Reset()
		{
			Attackee = null;
		}
	}	

	public class SKSpawnChildObjectEventParam : SKEventParam
	{
		public SKSpawnChildObjectEventParam() : base(SKEventManager.SKEventType.SpawnChildObject)
		{
			
		}
		
		public SKObject SpawnObject;
	    
		public override void Reset()
		{
			SpawnObject = null;
		}
	}	

	public class SKHealEventParam : SKEventParam
	{
		public SKHealEventParam() : base(SKEventManager.SKEventType.Heal)
		{
			
		}
		
		public ulong HealValue;
	    
		public override void Reset()
		{
			HealValue = 0;
		}
	}	
}

