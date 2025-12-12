using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbHelper
{
    // Interface cho các phương thức Async/chuẩn mực
    public interface IDbHelper
    {
        IDbDataParameter CreateParameter(string name, object value, DbType dbType, ParameterDirection direction = ParameterDirection.Input);
        Task<int> ExecuteNonQueryAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
        Task<DbDataReader> ExecuteReaderAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
        Task<object> ExecuteScalarAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
    }

    // Interface chứa phương thức đồng bộ/legacy (dùng out string msgError)
    public interface ILegacyDbHelper
    {
        // Đây là hàm mà DaiLyRepository đang gọi
        DataTable ExecuteSProcedureReturnDataTable(out string msgError, string sprocedureName, params object[] paramObjects);
        // Có thể thêm các hàm đồng bộ khác (ExecuteSProcedure, ExecuteScalarSProcedure...)
    }
}