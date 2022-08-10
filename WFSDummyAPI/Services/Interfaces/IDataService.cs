using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WFS_UI_API.Data;

namespace WFS_UI_API.Service
{
    public interface IDataService
    {
        string GetConnectionString(string Type);

        Task<long> CountAsync<T>(string Table, string Type) where T : class;

        Task<bool> InsertSingle<T, U>(string Query, U Input, string Type) where T : class;
        Task<bool> InsertMany<T, U>(string Query, List<U> InputList, string Type) where T : class;

        Task<T> SelectSingle<T, U>(string Query, U Input, string Type) where T : class, new();
        Task<List<T>> SelectMany<T, U>(string Query, U Input, string Type) where T : class, new();

        Task<bool> UpdateSingle<T, U>(string Query, U Input, string Type) where T : class;
        Task<bool> DeleteSingle<T, U>(string Query, U Input, string Type) where T : class;

        Task<bool> StoredProcLong<T, U>(string Query, U Input, string Type) where T : class;

        Task<bool> DeleteCustom(string Query, string Connection, string Type);

        bool Truncate(string Table, string Type);

        bool BulkUpload(DataTable model, string Table, string Type);
        Task<bool> CopyTable(string InputTableName, string OutputTableName, string DBConnInput, string DBConnOutput, bool mapUnmapped, List<ColumnMap> columnMaps);

    }
}
