using System;
using System.Collections.Generic;
using JHT.Scripts.Common;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.ResourceManager;

public static class DataTableManager
{
    public enum DataTableType
    {
        SupportItem
    }

    private static Dictionary<DataTableType, DataTableBase> _dataTableByType = new();

    public static bool Load()
    {
        foreach (DataTableType dataTableType in Enum.GetValues(typeof(DataTableType)))
        {
            var dataTableBase = ResourceManager.Instance.LoadOriginalAsset<DataTableBase>($"DataTable/{dataTableType.ToStringCached()}DataTable");
            if (dataTableBase.IsNull())
            {
                continue;
            }

            dataTableBase.Load();

            _dataTableByType.Add(dataTableType, dataTableBase);
        }
        
        return true;
    }

    public static DataTableBase GetDataTableBase(DataTableType dataTableType)
    {
        if (false == _dataTableByType.TryGetValue(dataTableType, out var dataTableBase))
        {
            return null;
        }

        return dataTableBase;
    }
}
