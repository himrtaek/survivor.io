using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SupportItemDataTable", menuName = "ScriptableObjects/DataTable/SupportItemDataTable", order = 1)]
public class SupportItemDataTable : DataTableDoubleIdBase<SupportItemData>
{
    public static SupportItemData GetRow(uint id, uint key)
    {
        return GetRowBase(DataTableManager.DataTableType.SupportItem, id, key);
    }
}

[Serializable]
public class SupportItemData : DataRowDoubleIdBase
{
    [SerializeField] private uint id;
    [SerializeField] private uint key;
    [SerializeField] private SKSkill.SKSkillType skillType;
    [SerializeField] private long skillValue1;
    [SerializeField] private long skillValue2;
    [SerializeField] private long skillValue3;
    [SerializeField] private long skillValue4;

    public override uint Id => id;
    public override uint Key => key;
    public SKSkill.SKSkillType SkillType => skillType;
    public long SkillValue1 => skillValue1;
    public long SkillValue2 => skillValue2;
    public long SkillValue3 => skillValue3;
    public long SkillValue4 => skillValue4;
}
