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

        /// <summary>
        /// 根据角色ID和格子索引获取单个背包物品
        /// </summary>
        public InventoryItemEntity GetByCharacterIdAndSlotIndex(int characterId, int slotIndex)
        {
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
                    WHERE CharacterId = @CharacterId AND SlotIndex = @SlotIndex";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@SlotIndex", slotIndex);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new InventoryItemEntity
                        {
                            Id = reader.GetInt32(0),
                            CharacterId = reader.GetInt32(1),
                            ItemId = reader.GetInt32(2),
                            SlotIndex = reader.GetInt32(3),
                            Count = reader.GetInt32(4),
                            IsBound = reader.GetBoolean(5)
                        };
                    }
                }
            }
        }

        /// <summary>
        /// 更新某个背包物品数量
        /// </summary>
        public void UpdateItemCount(int inventoryItemId, int newCount)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    UPDATE dbo.InventoryItems
                    SET Count = @Count
                    WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", inventoryItemId);
                    cmd.Parameters.AddWithValue("@Count", newCount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 删除某个背包物品记录
        /// </summary>
        public void DeleteById(int inventoryItemId)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"DELETE FROM dbo.InventoryItems WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", inventoryItemId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 删除莫格个角色的所有背包物品记录
        /// </summary>
        public void DeleteAllByCharacterId(int characterId)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                DELETE FROM InventoryItems
                WHERE CharacterId = @CharacterId";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CharacterId", characterId);
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 更新背包物品所在格子索引
        /// </summary>
        public void UpdateItemSlotIndex(int inventoryItemId, int slotIndex)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                UPDATE InventoryItems
                SET SlotIndex = @SlotIndex
                WHERE Id = @Id";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@SlotIndex", slotIndex);
                cmd.Parameters.AddWithValue("@Id", inventoryItemId);
                cmd.ExecuteNonQuery();
            }
        }
        public void Insert(InventoryItemEntity entity)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    INSERT INTO InventoryItems (CharacterId, SlotIndex, ItemId, Count)
                    VALUES (@CharacterId, @SlotIndex, @ItemId, @Count)";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CharacterId", entity.CharacterId);
                cmd.Parameters.AddWithValue("@SlotIndex", entity.SlotIndex);
                cmd.Parameters.AddWithValue("@ItemId", entity.ItemId);
                cmd.Parameters.AddWithValue("@Count", entity.Count);
                cmd.ExecuteNonQuery();
            }
        }
    }
}