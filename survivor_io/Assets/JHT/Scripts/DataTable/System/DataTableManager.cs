using System;
using System.Collections.Generic;
using JHT.Scripts.Common;
using JHT.Scripts.ResourceManager;

public static class DataTableManager
{
    private static HashSet<Type> _dataTypes = new();
    private static Dictionary<Type, DataTableBase> _dataTableByType = new();

    public static bool Load()
    {
        foreach (Type dataTableType in _dataTypes)
        {
            var dataTableBase = ResourceManager.Instance.LoadOriginalAsset<DataTableBase>($"DataTable/{dataTableType.Name}");
            if (dataTableBase.IsNull())
            {
                continue;
            }

            dataTableBase.Load();

            _dataTableByType.Add(dataTableType, dataTableBase);
        }
        
        return true;
    }

    public static void AddDataTableType(Type dataTableType)
    {
        _dataTypes.Add(dataTableType);
    } 

    public static DataTableBase GetDataTableBase(Type dataTableType)
    {
        if (false == _dataTableByType.TryGetValue(dataTableType, out var dataTableBase))
        {
            return null;
        }

        return dataTableBase;
    }
}
