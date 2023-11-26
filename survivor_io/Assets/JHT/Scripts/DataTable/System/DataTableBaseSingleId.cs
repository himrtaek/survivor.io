using System.Collections.Generic;
using JHT.Scripts.Common;
using UnityEngine;

public abstract class DataTableSingleIdBase<T> : DataTableBase where T : DataRowSingleIdBase
{
    [SerializeField] private List<T> _dataTableByKeySource = new();
    private Dictionary<uint, T> _dataTableById = new();
    
    public override  bool Load()
    {
        foreach (var it in _dataTableByKeySource)
        {
            _dataTableById.Add(it.Id, it);
        }

        return true;
    }
    
    protected static T GetRowBase(DataTableManager.DataTableType dataTableType, uint id)
    {
        var dataTableBase = DataTableManager.GetDataTableBase(dataTableType);
        if (null == dataTableBase)
        {
            return null;
        }

        var dataSingleIdBase = dataTableBase as DataTableSingleIdBase<T>;
        if (null == dataSingleIdBase)
        {
            return null;
        }

        return dataSingleIdBase.GetRowGeneric(id);
    }
    
    private T GetRowGeneric(uint id)
    {
        if (_dataTableById.TryGetValue(id, out var dataTable).IsFalse())
        {
            return null;
        }

        return dataTable;
    }
}

public abstract class DataRowSingleIdBase
{
    public abstract uint Id { get; }
}
