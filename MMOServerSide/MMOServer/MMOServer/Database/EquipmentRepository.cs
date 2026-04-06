using Microsoft.Data.SqlClient;
using MMOServer.Models;

namespace MMOServer.Database
{
    public class EquipmentRepository
    {
        /// <summary>
        /// 获取角色全部装备
        /// </summary>
        public List<CharacterEquipmentEntity> GetEquipmentListByCharacterId(int characterId)
        {
            List<CharacterEquipmentEntity> result = new List<CharacterEquipmentEntity>();

            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, CharacterId, EquipSlotType, ItemId
                    FROM dbo.CharacterEquipments
                    WHERE CharacterId = @CharacterId
                    ORDER BY EquipSlotType ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CharacterEquipmentEntity entity = new CharacterEquipmentEntity
                            {
                                Id = reader.GetInt32(0),
                                CharacterId = reader.GetInt32(1),
                                EquipSlotType = reader.GetInt32(2),
                                ItemId = reader.GetInt32(3)
                            };

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据角色ID和装备槽位获取装备
        /// </summary>
        public CharacterEquipmentEntity GetByCharacterIdAndSlotType(int characterId, int equipSlotType)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, CharacterId, EquipSlotType, ItemId
                    FROM dbo.CharacterEquipments
                    WHERE CharacterId = @CharacterId AND EquipSlotType = @EquipSlotType";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@EquipSlotType", equipSlotType);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new CharacterEquipmentEntity
                        {
                            Id = reader.GetInt32(0),
                            CharacterId = reader.GetInt32(1),
                            EquipSlotType = reader.GetInt32(2),
                            ItemId = reader.GetInt32(3)
                        };
                    }
                }
            }
        }

        /// <summary>
        /// 插入一条装备记录
        /// </summary>
        public int Insert(CharacterEquipmentEntity entity)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    INSERT INTO dbo.CharacterEquipments
                    (
                        CharacterId,
                        EquipSlotType,
                        ItemId
                    )
                    VALUES
                    (
                        @CharacterId,
                        @EquipSlotType,
                        @ItemId
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", entity.CharacterId);
                    cmd.Parameters.AddWithValue("@EquipSlotType", entity.EquipSlotType);
                    cmd.Parameters.AddWithValue("@ItemId", entity.ItemId);

                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 更新某条装备记录的 ItemId
        /// </summary>
        public void UpdateItemId(int id, int itemId)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    UPDATE dbo.CharacterEquipments
                    SET ItemId = @ItemId
                    WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 根据角色ID和装备槽位更新装备
        /// </summary>
        public void UpdateByCharacterIdAndSlotType(int characterId, int equipSlotType, int itemId)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    UPDATE dbo.CharacterEquipments
                    SET ItemId = @ItemId
                    WHERE CharacterId = @CharacterId AND EquipSlotType = @EquipSlotType";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@EquipSlotType", equipSlotType);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 删除一条装备记录
        /// </summary>
        public void DeleteById(int id)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"DELETE FROM dbo.CharacterEquipments WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 根据角色ID和装备槽位删除装备
        /// </summary>
        public void DeleteByCharacterIdAndSlotType(int characterId, int equipSlotType)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    DELETE FROM dbo.CharacterEquipments
                    WHERE CharacterId = @CharacterId AND EquipSlotType = @EquipSlotType";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@EquipSlotType", equipSlotType);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 判断指定角色槽位是否已有装备
        /// </summary>
        public bool ExistsByCharacterIdAndSlotType(int characterId, int equipSlotType)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT COUNT(1)
                    FROM dbo.CharacterEquipments
                    WHERE CharacterId = @CharacterId AND EquipSlotType = @EquipSlotType";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@EquipSlotType", equipSlotType);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}