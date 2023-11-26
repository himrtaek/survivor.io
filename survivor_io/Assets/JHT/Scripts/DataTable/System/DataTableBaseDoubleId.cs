using System;
using System.Collections;
using System.Collections.Generic;
using JHT.Scripts.Common;
using UnityEngine;

public abstract class DataTableDoubleIdBase<T> : DataTableBase where T : DataRowDoubleIdBase
{
    [SerializeField] private List<T> _dataTableByKeySource = new();
    private Dictionary<uint, Dictionary<uint, T>> _dataTableByIdKey = new();

    public override bool Load()
    {
        foreach (var it in _dataTableByKeySource)
        {
            if (false == _dataTableByIdKey.TryGetValue(it.Id, out var dataTableByIdKey))
            {
                dataTableByIdKey = new();
                _dataTableByIdKey.Add(it.Id, dataTableByIdKey);
            }
            
            dataTableByIdKey.Add(it.Key, it);
        }

        return true;
    }

    protected static T GetRowBase(Type dataTableType, uint id, uint key)
    {
        var dataTableBase = DataTableManager.GetDataTableBase(dataTableType);
        if (null == dataTableBase)
        {
            return null;
        }

        var dataTableDoubleIdBase = dataTableBase as DataTableDoubleIdBase<T>;
        if (null == dataTableDoubleIdBase)
        {
            return null;
        }

        return dataTableDoubleIdBase.GetRowGeneric(id, key);
    }
    
    private T GetRowGeneric(uint id, uint key)
    {
        if (_dataTableByIdKey.TryGetValue(id, out var dataTableByKey).IsFalse())
        {
            return null;
        }

        if (dataTableByKey.TryGetValue(key, out var dataTable).IsFalse())
        {
            return null;
        }

        return dataTable;
    }
}

public abstract class DataRowDoubleIdBase
{
    public abstract uint Id { get; }
    public abstract uint Key { get; }
}
