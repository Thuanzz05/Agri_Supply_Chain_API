using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbHelper
{
    public class SqlServerHelper : IDbHelper
    {
        private readonly string _connectionString;
        private const string ConnectionStringKey = "AgriSupplyChainDB";

        // --- Constructor: Nhận IConfiguration qua DI ---
        public SqlServerHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(ConnectionStringKey)
                                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringKey}' not found.");
        }

        // --- Phương thức Khởi tạo Tham số ---
        public IDbDataParameter CreateParameter(string name, object value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            return new SqlParameter
            {
                ParameterName = name,
                Value = value ?? DBNull.Value, // Xử lý giá trị null
                DbType = dbType,
                Direction = direction
            };
        }

        // --- Thiết lập Command chung ---
        private SqlCommand CreateCommand(SqlConnection connection, string commandText, IEnumerable<IDbDataParameter> parameters, CommandType commandType)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    // Chuyển IDbDataParameter về SqlParameter
                    command.Parameters.Add((SqlParameter)param);
                }
            }
            return command;
        }

        // --- 1. Thực thi NonQuery (Async) ---
        public async Task<int> ExecuteNonQueryAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text)
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

        // --- 2. Thực thi Reader (Async) ---
        public async Task<DbDataReader> ExecuteReaderAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.OpenAsync();
                using (var command = CreateCommand(connection, commandText, parameters, commandType))
                {
                    // CommandBehavior.CloseConnection đảm bảo Reader đóng kết nối khi nó được dispose
                    return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                }
            }
            catch
            {
                // Đảm bảo đóng kết nối nếu có lỗi xảy ra
                if (connection.State == ConnectionState.Open) connection.Close();
                throw;
            }
        }

        // --- 3. Thực thi Scalar (Async) ---
        public async Task<object> ExecuteScalarAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text)
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
    }
}