using System.Collections.Generic;
using UnityEngine;

namespace SK
{
	public class SKMovementPlayerInput : SKCreatureMovementBase
	{
		private SKStatManager _playerStatManager;

		public SKStatManager PlayerStatManager
		{
			get
			{
				if (_playerStatManager == null)
				{
					_playerStatManager = SKGameManager.Instance.PlayerStatManager;
				}

				return _playerStatManager;
			}
		}
		
		protected override void ImportFieldFromData()
		{
		    
		}

		protected override List<string> ExportFieldToData()
		{
			return null;
		}
	    
		public override void OnSKObjectSpawn()
		{
			base.OnSKObjectSpawn();
		    
			SKGameManager.Instance.PlayerInput.OnInputDirection.AddListener(OnInputDirection);
		}
		public override void OnSKObjectDestroy()
		{
			SKGameManager.Instance.PlayerInput.OnInputDirection.RemoveListener(OnInputDirection);

			_playerStatManager = null;
		    
			base.OnSKObjectDestroy();
		}

		void OnInputDirection(Vector3 direction)
		{
			float moveSpeed = Time.fixedDeltaTime * PlayerStatManager.GetStatResultValue(StatType.MoveSpeed);
			
			Translate(SkObjectCreature.Rigidbody2D, direction, moveSpeed);
			SkObjectCreature.ObjectView.LookAtLeftByDirectionValue(direction.x);
		}
	}
}
