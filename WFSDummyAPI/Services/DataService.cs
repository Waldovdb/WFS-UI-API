using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using WFS_UI_API.Data;
using Microsoft.Extensions.Configuration;
using System.Dynamic;

namespace WFS_UI_API.Service
{
    public class DataService : IDataService
    {
        private readonly string _dbConnectionUI;
        private readonly string _dbConnectionPres;
        private readonly string _dbConnectionTAD;
        private readonly string _dbConnectionCIM;

        #region [ Default Constructor ]
        public DataService()
        {
            _dbConnectionUI = $"Data Source=10.21.160.104;Initial Catalog=AGPUI;User Id=InovoCIM;Password=g8rF1eld;";
            _dbConnectionCIM = $"Data Source=10.21.160.104;Initial Catalog=InovoCIM;User Id=InovoCIM;Password=g8rF1eld;";
            _dbConnectionPres = $"Data Source=10.21.160.134\\INST01;Initial Catalog=sqlpr1;User Id=PTOOLS;Password=G00dluck2314;";
            _dbConnectionTAD = $"Data Source=10.21.160.134\\INST01;Initial Catalog=sqlpr1;User Id=PTOOLS;Password=G00dluck2314;";
        }
        #endregion

        //-----------------------------//

        #region [ Get Connection String ]
        public string GetConnectionString(string Type)
        {
           switch(Type)
            {
                case "Pres":
                    return _dbConnectionPres;
                case "UI":
                    return _dbConnectionUI;
                case "TAD":
                    return _dbConnectionTAD;
                case "CIM":
                    return _dbConnectionCIM;
                default:
                    return "";
            }
        }
        #endregion

        #region [ Count Async ]
        public async Task<long> CountAsync<T>(string Table, string Type) where T : class
        {
            try
            {
                using var conn = new SqlConnection(GetConnectionString(Type));
                long total = await conn.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM {Table}");
                return total;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        //------------------//

        #region [ Insert Single ]
        public async Task<bool> InsertSingle<T, U>(string Query, U Input, string Type) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(GetConnectionString(Type));
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 1500);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region [ Insert Many ]
        public async Task<bool> InsertMany<T, U>(string Query, List<U> InputList, string Type) where T : class
        {
            if (InputList.Count > 0)
            {
                foreach (var item in InputList)
                {
                    try
                    {
                        using var conn = new SqlConnection(GetConnectionString(Type));
                        await conn.ExecuteAsync(Query, item, commandTimeout: 1500);
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        continue;
                    }
                }
                return true;
            }
            return false;
        }
        #endregion

        //------------------//

        #region [ Select Single ]
        public async Task<T> SelectSingle<T, U>(string Query, U Input, string Type) where T : class, new()
        {
            try
            {
                using var conn = new SqlConnection(GetConnectionString(Type));
                var data = await conn.QueryAsync<T>(Query, Input, commandTimeout: 1500);
                return data.FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            return new T();
        }
        #endregion

        #region [ Copy Table ]
        public async Task<bool> CopyTable(string InputTableName, string OutputTableName, string DBConnInput, string DBConnOutput)
        {
            try
            {
                List<string> columnList = new();
                string TruncateCommand = $"DELETE FROM {OutputTableName} WHERE 1=1";

                using (var conn = new SqlConnection(DBConnInput))
                {
                    var dataColumns = await conn.QueryAsync<DataSchema>("SELECT [COLUMN_NAME],[ORDINAL_POSITION],[DATA_TYPE] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName ORDER BY [ORDINAL_POSITION]", new { TableName = InputTableName.Replace("[dbo].[","").Replace("]","") });
                    List<DataSchema> schemaList = dataColumns.ToList();
                    foreach (var row in schemaList.OrderBy(x => x.ORDINAL_POSITION)) { columnList.Add(row.COLUMN_NAME); }
                    conn.Close();
                }

                using (var conn = new SqlConnection(DBConnOutput))
                {
                    conn.Open();
                    using SqlCommand cmd = new SqlCommand();
                    cmd.CommandTimeout = 600;
                    cmd.Connection = conn;
                    cmd.Parameters.Clear();
                    cmd.CommandText = TruncateCommand;
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }

                SqlBulkCopyOptions sqlOptions = SqlBulkCopyOptions.Default;
                sqlOptions |= SqlBulkCopyOptions.KeepIdentity;
                sqlOptions |= SqlBulkCopyOptions.KeepNulls;
                sqlOptions |= SqlBulkCopyOptions.TableLock;

                using (SqlBulkCopy bcp = new(DBConnOutput, sqlOptions))
                {
                    bcp.BulkCopyTimeout = 0;
                    bcp.BatchSize = 10000;
                    bcp.DestinationTableName = OutputTableName;

                    foreach (string column in columnList.ToArray())
                    {
                        SqlBulkCopyColumnMapping mapping = new() { DestinationColumn = column, SourceColumn = column };
                        bcp.ColumnMappings.Add(mapping);
                    }

                    string SelectCommand = $"SELECT * FROM {InputTableName}";

                    using (var conn = new SqlConnection(DBConnInput))
                    {
                        conn.Open();
                        using SqlCommand cmd = new SqlCommand();
                        cmd.CommandTimeout = 600;
                        cmd.Connection = conn;
                        cmd.Parameters.Clear();
                        cmd.CommandText = SelectCommand;
                        cmd.CommandType = CommandType.Text;

                        using SqlDataReader Reader = cmd.ExecuteReader();
                        bcp.WriteToServer(Reader);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region [ Copy Table ]
        public async Task<bool> CopyTable(string InputTableName, string OutputTableName, string DBConnInput, string DBConnOutput, bool mapUnmapped, List<ColumnMap> columnMaps)
        {
            try
            {
                List<string> columnList = new();
                string TruncateCommand = $"DELETE FROM {OutputTableName} WHERE 1=1";

                using (var conn = new SqlConnection(DBConnInput))
                {
                    var dataColumns = await conn.QueryAsync<DataSchema>("SELECT [COLUMN_NAME],[ORDINAL_POSITION],[DATA_TYPE] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName ORDER BY [ORDINAL_POSITION]", new { TableName = InputTableName.Replace("[dbo].[", "").Replace("]", "") });
                    List<DataSchema> schemaList = dataColumns.ToList();
                    foreach (var row in schemaList.OrderBy(x => x.ORDINAL_POSITION)) { columnList.Add(row.COLUMN_NAME); }
                    conn.Close();
                }

                using (var conn = new SqlConnection(DBConnOutput))
                {
                    conn.Open();
                    using SqlCommand cmd = new SqlCommand();
                    cmd.CommandTimeout = 600;
                    cmd.Connection = conn;
                    cmd.Parameters.Clear();
                    cmd.CommandText = TruncateCommand;
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }

                SqlBulkCopyOptions sqlOptions = SqlBulkCopyOptions.Default;
                sqlOptions |= SqlBulkCopyOptions.KeepIdentity;
                sqlOptions |= SqlBulkCopyOptions.KeepNulls;
                sqlOptions |= SqlBulkCopyOptions.TableLock;

                List<string> mappedColumns = new();
                List<string> unmappedColumns = new();

                using (SqlBulkCopy bcp = new(DBConnOutput, sqlOptions))
                {
                    bcp.BulkCopyTimeout = 0;
                    bcp.BatchSize = 10000;
                    bcp.DestinationTableName = OutputTableName;

                    foreach (string column in columnList.ToArray())
                    {
                        if(mapUnmapped && columnMaps.Count > 0)
                        {
                            if (columnMaps.Exists(o => o.target == column))
                            {
                                SqlBulkCopyColumnMapping mapping = new() { DestinationColumn = column, SourceColumn = columnMaps.Where(o => o.target == column).First().source };
                                mappedColumns.Add(column);
                                bcp.ColumnMappings.Add(mapping);
                            }
                            else if (mapUnmapped)
                            {
                                SqlBulkCopyColumnMapping mapping = new() { DestinationColumn = column, SourceColumn = column };
                                mappedColumns.Add(column);
                                bcp.ColumnMappings.Add(mapping);
                            }
                        }
                        else
                        {
                            SqlBulkCopyColumnMapping mapping = new() { DestinationColumn = column, SourceColumn = column };
                            mappedColumns.Add(column);
                            bcp.ColumnMappings.Add(mapping);
                        }             
                    }

                    string SelectCommand = $"SELECT * FROM {InputTableName}";

                    using (var conn = new SqlConnection(DBConnInput))
                    {
                        conn.Open();
                        using SqlCommand cmd = new SqlCommand();
                        cmd.CommandTimeout = 0;
                        cmd.Connection = conn;
                        cmd.Parameters.Clear();
                        cmd.CommandText = SelectCommand;
                        cmd.CommandType = CommandType.Text;

                        using SqlDataReader Reader = cmd.ExecuteReader();
                        bcp.WriteToServer(Reader);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region [ Select Many ]
        public async Task<List<T>> SelectMany<T, U>(string Query, U Input, string Type) where T : class, new()
        {
            try
            {
                using var conn = new SqlConnection(GetConnectionString(Type));
                var data = await conn.QueryAsync<T>(Query, Input, commandTimeout: 1500);
                return data.ToList();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return null;
        }
        #endregion

        #region [ Dynamic Thing ]
        public ExpandoObject CreateDynamic(string propertyName, string PropertyValue)
        {
            dynamic cust = new ExpandoObject();
            ((IDictionary<string, object>)cust)[propertyName] = PropertyValue;
            return cust;
        }
        #endregion

        //------------------//

        #region [ Update Single ]
        public async Task<bool> UpdateSingle<T, U>(string Query, U Input, string Type) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(GetConnectionString(Type));
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 1500);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Update Single ]
        public async Task<bool> StoredProcLong<T, U>(string Query, U Input, string Type) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(GetConnectionString(Type));
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 3600);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        //------------------//

        #region [ Delete Single ]
        public async Task<bool> DeleteSingle<T, U>(string Query, U Input, string Type) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(GetConnectionString(Type));
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 1500);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Delete Custom ]
        public async Task<bool> DeleteCustom(string Query, string Connection, string Type)
        {
            try
            {
                using (var conn = new SqlConnection(Connection))
                {
                    await conn.ExecuteAsync(Query, new { }, commandTimeout: 1500);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        //------------------//

        #region [ Truncate ]
        public bool Truncate(string Table, string Type)
        {
            try
            {
                Table = Table.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
                Table = string.Format("[dbo].[{0}]", Table);

                using (var conn = new SqlConnection(GetConnectionString(Type)))
                {
                    conn.Execute($"TRUNCATE TABLE {Table}", new { });
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Bulk Upload ]
        public bool BulkUpload(DataTable model, string Table, string Type)
        {
            try
            {
                Table = Table.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
                Table = string.Format("[dbo].[{0}]", Table);

                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(GetConnectionString(Type)))
                {
                    SqlBulk.DestinationTableName = Table;
                    SqlBulk.BatchSize = 9500;
                    SqlBulk.BulkCopyTimeout = 1500;
                    SqlBulk.WriteToServer(model);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
