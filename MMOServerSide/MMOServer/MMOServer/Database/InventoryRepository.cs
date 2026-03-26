using Microsoft.Data.SqlClient;
using MMOServer.Models;

namespace MMOServer.Database
{
    public class InventoryRepository
    {
        /// <summary>
        /// 根据角色ID获取背包物品列表
        /// </summary>
        public List<InventoryItemEntity> GetInventoryListByCharacterId(int characterId)
        {
            List<InventoryItemEntity> result = new List<InventoryItemEntity>();

            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT
                        Id,
                        CharacterId,
                        ItemId,
                        SlotIndex,
                        Count,
                        IsBound
                    FROM dbo.InventoryItems
                    WHERE CharacterId = @CharacterId
                    ORDER BY SlotIndex ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InventoryItemEntity entity = new InventoryItemEntity
                            {
                                Id = reader.GetInt32(0),
                                CharacterId = reader.GetInt32(1),
                                ItemId = reader.GetInt32(2),
                                SlotIndex = reader.GetInt32(3),
                                Count = reader.GetInt32(4),
                                IsBound = reader.GetBoolean(5)
                            };

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }
    }
}