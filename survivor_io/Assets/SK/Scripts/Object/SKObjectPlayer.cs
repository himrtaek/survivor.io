using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class SKObjectPlayer : SKObjectBase
{
    public override SKObjectType ObjectType => SKObjectType.Player;
    private Dictionary<SKSupportItem.SKSupportItemType, SKSupportItem> _supportItemsByType = new();

    public void AddSupportItemInfo(SKSupportItem.SKSupportItemType supportItemType, int level)
    {
        _supportItemsByType.Add(supportItemType, new SKSupportItem(supportItemType, level));
    }

    public void RemoveSupportItem(SKSupportItem.SKSupportItemType supportItemType)
    {
        _supportItemsByType.Remove(supportItemType);
    }
}
