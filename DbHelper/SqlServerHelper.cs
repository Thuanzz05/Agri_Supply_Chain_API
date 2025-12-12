using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace DbHelper
{
    // SỬA LỖI: Implement cả IDbHelper và ILegacyDbHelper
    public class SqlServerHelper : IDbHelper, ILegacyDbHelper
    {
        private readonly string _connectionString;
        // Đảm bảo ConnectionStringKey khớp với appsettings.json của bạn
        private const string ConnectionStringKey = "DefaultConnection";

        // --- Constructor: Nhận IConfiguration qua DI ---
        public SqlServerHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(ConnectionStringKey)
                                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringKey}' not found.");
        }

        // --- Helper chung để tạo SqlCommand ---
        private SqlCommand CreateCommand(SqlConnection connection, string commandText, IEnumerable<IDbDataParameter>? parameters, CommandType commandType)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add((SqlParameter)param);
                }
            }
            return command;
        }

        // --- TRIỂN KHAI IDbHelper (Async) ---

        public IDbDataParameter CreateParameter(string name, object value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            return new SqlParameter { ParameterName = name, Value = value ?? DBNull.Value, DbType = dbType, Direction = direction };
        }

        public async Task<int> ExecuteNonQueryAsync(string commandText, IEnumerable<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = CreateCommand(connection, commandText, parameters, commandType))
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string commandText, IEnumerable<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text)
        {
            var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.OpenAsync();
                using (var command = CreateCommand(connection, commandText, parameters, commandType))
                {
                    return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                }
            }
            catch
            {
                if (connection.State == ConnectionState.Open) connection.Close();
                throw;
            }
        }

        public async Task<object> ExecuteScalarAsync(string commandText, IEnumerable<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = CreateCommand(connection, commandText, parameters, commandType))
                {
                    return await command.ExecuteScalarAsync();
                }
            }
        }

        // --- TRIỂN KHAI ILegacyDbHelper (Đồng bộ/Fix lỗi triển khai) ---

        // SỬA LỖI: Triển khai hàm này với chữ ký chính xác
        public DataTable ExecuteSProcedureReturnDataTable(out string msgError, string sprocedureName, params object[] paramObjects)
        {
            msgError = "";
            DataTable tb = new DataTable();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand { CommandType = CommandType.StoredProcedure, CommandText = sprocedureName };
                    connection.Open();
                    cmd.Connection = connection;

                    // Logic xử lý tham số (params object[])
                    int parameterInput = (paramObjects.Length) / 2;
                    int j = 0;
                    for (int i = 0; i < parameterInput; i++)
                    {
                        string paramName = Convert.ToString(paramObjects[j++])?.Trim() ?? string.Empty;
                        object? value = paramObjects[j++]; // Có thể là null

                        if (paramName.ToLower().Contains("json"))
                        {
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = paramName,
                                Value = value ?? DBNull.Value,
                                SqlDbType = SqlDbType.NVarChar
                            });
                        }
                        else
                        {
                            // SỬA: Thêm tham số bằng cú pháp SqlParameter
                            cmd.Parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
                        }
                    }

                    // Thực thi và Fill DataTable
                    using (SqlDataAdapter ad = new SqlDataAdapter(cmd))
                    {
                        ad.Fill(tb);
                    }
                    cmd.Dispose();
                }
                catch (Exception exception)
                {
                    tb = null;
                    msgError = exception.ToString();
                }
            }
            return tb;
        }
    }
}