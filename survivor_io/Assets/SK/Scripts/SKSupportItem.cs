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
        
    }
}
