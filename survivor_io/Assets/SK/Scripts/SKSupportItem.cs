using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SKSupportItem
{
    public enum SKSupportItemType
    {
        FitnessManual,
    }
    
    public int Level { get; }

    public SKSkill skill;

    public SKSupportItem(SKSupportItemType supportItemType, int level)
    {
        var data = SupportItemDataTable.GetRow((uint)supportItemType, (uint)level);
        skill = new SKSkill(data.SkillType, data.SkillValue1, data.SkillValue2, data.SkillValue3, data.SkillValue4);
    }

    public void DoAction()
    {
        skill.DoAction();
    }

    public void UnDoAction()
    {
        skill.UnDoAction();
    }
}
