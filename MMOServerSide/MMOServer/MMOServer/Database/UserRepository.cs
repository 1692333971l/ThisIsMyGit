using Microsoft.Data.SqlClient;
using MMOServer.Models;

namespace MMOServer.Database
{
    public class UserRepository
    {
        /// <summary>
        /// 根据账号查询用户
        /// </summary>
        public UserEntity GetByAccount(string account)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = "SELECT Id, Account, Password FROM dbo.Users WHERE Account = @Account";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Account", account);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new UserEntity
                        {
                            Id = reader.GetInt32(0),
                            Account = reader.GetString(1),
                            Password = reader.GetString(2)
                        };
                    }
                }
            }
        }

        /// <summary>
        /// 判断账号是否已存在
        /// </summary>
        public bool Exists(string account)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = "SELECT COUNT(1) FROM dbo.Users WHERE Account = @Account";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Account", account);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// 插入新用户
        /// </summary>
        public bool Insert(string account, string password)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = "INSERT INTO dbo.Users (Account, Password) VALUES (@Account, @Password)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Account", account);
                    cmd.Parameters.AddWithValue("@Password", password);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }
    }
}