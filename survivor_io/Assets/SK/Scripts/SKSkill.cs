using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SKSkill
{
    public enum SKSkillType
    {
        IncreaseMaxHp
    }

    public SKSkillType SkillType { get; }
    public long SkillValue1 { get; }
    public long SkillValue2 { get; }
    public long SkillValue3 { get; }
    public long SkillValue4 { get; }

    public SKSkill(SKSkillType skillType, long skillValue1 = 0, long skillValue2 = 0, long skillValue3 = 0,
        long skillValue4 = 0)
    {
        SkillType = skillType;
        SkillValue1 = skillValue1;
        SkillValue2 = skillValue2;
        SkillValue3 = skillValue3;
        SkillValue4 = skillValue4;
    }

    public void DoAction()
    {
        switch (SkillType)
        {
            
        }
    }

    public void UnDoAction()
    {
        switch (SkillType)
        {
            
        }
    }
}
