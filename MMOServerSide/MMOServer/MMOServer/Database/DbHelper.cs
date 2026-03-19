using Microsoft.Data.SqlClient;

namespace MMOServer.Database
{
    public static class DbHelper
    {
        // 使用你本机实际的 LocalDB 实例名
        private static readonly string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=MMOGame;Integrated Security=True;TrustServerCertificate=True;";

        /// <summary>
        /// 获取一个新的数据库连接
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}