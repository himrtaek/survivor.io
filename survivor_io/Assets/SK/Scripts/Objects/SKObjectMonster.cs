using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(SKComponentAttacker))]
    public class SKObjectMonster : SKObjectCreature
    {
	    #region Cache
	    
	    [SerializeField] private SKComponentAttacker componentAttacker;
	    public SKComponentAttacker ComponentAttacker
	    {
		    get
		    {
			    if (false == componentAttacker)
			    {
				    TryGetComponent(out componentAttacker);
			    }
			    
			    return componentAttacker;
		    }
	    }

	    #endregion
	    
	    public override SKObjectType ObjectType { get; } = SKObjectType.Monster;
	    
	    public enum SKMonsterGradeType
	    {
		    Normal,
		    Rare,
		    Elite,
		    Boss,
	    }
	    
	    [SerializeField] private SKMonsterGradeType _monsterGrade;
	    public SKMonsterGradeType MonsterGrade => _monsterGrade;

	    public override void OnSKObjectSpawn()
	    {
		    base.OnSKObjectSpawn();
		    
		    ObjectView.PlayAnim(SKViewAnimNameType.Appear, SKViewAnimNameType.Move);
		    
		    if (MonsterGrade == SKMonsterGradeType.Boss)
		    {
			    SKGameManager.Instance.BossBattleStart(this);
		    }
	    }
	    
	    public override void OnSKObjectDestroy()
	    {
		    if (MonsterGrade == SKMonsterGradeType.Boss)
		    {
			    SKGameManager.Instance.BossBattleEnd(this);
		    }
		    
		    base.OnSKObjectDestroy();
	    }
    }
}
