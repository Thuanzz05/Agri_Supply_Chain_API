using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbHelper
{
    public interface IDbHelper
    {
        // Khởi tạo tham số 
        IDbDataParameter CreateParameter(string name, object value, DbType dbType, ParameterDirection direction = ParameterDirection.Input);

        // Thực thi NonQuery 
        Task<int> ExecuteNonQueryAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);

        // Thực thi Reader 
        Task<DbDataReader> ExecuteReaderAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);

        // Thực thi Scalar 
        Task<object> ExecuteScalarAsync(string commandText, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
    }
}