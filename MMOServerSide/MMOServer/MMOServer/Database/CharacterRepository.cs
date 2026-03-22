using Microsoft.Data.SqlClient;
using MMOServer.Models;
using System;
using System.Collections.Generic;

namespace MMOServer.Database
{
    public class CharacterRepository
    {
        /// <summary>
        /// 获取指定用户的角色列表
        /// </summary>
        public List<CharacterEntity> GetCharacterListByUserId(int userId)
        {
            List<CharacterEntity> result = new List<CharacterEntity>();

            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, UserId, Name, Profession, Level, Gold, Hp, Mp
                    FROM dbo.Characters
                    WHERE UserId = @UserId
                    ORDER BY Id ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CharacterEntity entity = new CharacterEntity
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                Name = reader.GetString(2),
                                Profession = reader.GetInt32(3),
                                Level = reader.GetInt32(4),
                                Gold = reader.GetInt32(5),
                                Hp = reader.GetInt32(6),
                                Mp = reader.GetInt32(7)
                            };

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// 根据角色Id和用户Id获取角色
        /// </summary>
        public CharacterEntity GetByCharacterIdAndUserId(int characterId, int userId)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        Id, UserId, Name, Profession, Level, Gold,
                        Strength, Agility, Intelligence,
                        CritRate, CritDamage, Defense,
                        Hp, Mp, MaxHp, MaxMp,
                        MapId, PosX, PosY, PosZ
                    FROM dbo.Characters
                    WHERE Id = @CharacterId AND UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CharacterId", characterId);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new CharacterEntity
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            Profession = reader.GetInt32(3),
                            Level = reader.GetInt32(4),
                            Gold = reader.GetInt32(5),

                            Strength = reader.GetInt32(6),
                            Agility = reader.GetInt32(7),
                            Intelligence = reader.GetInt32(8),

                            CritRate = reader.GetDecimal(9),
                            CritDamage = reader.GetDecimal(10),
                            Defense = reader.GetInt32(11),

                            Hp = reader.GetInt32(12),
                            Mp = reader.GetInt32(13),
                            MaxHp = reader.GetInt32(14),
                            MaxMp = reader.GetInt32(15),

                            MapId = reader.GetInt32(16),
                            PosX = Convert.ToSingle(reader.GetDouble(17)),
                            PosY = Convert.ToSingle(reader.GetDouble(18)),
                            PosZ = Convert.ToSingle(reader.GetDouble(19))
                        };
                    }
                }
            }
        }
        /// <summary>
        /// 判断角色名是否已存在
        /// </summary>
        public bool ExistsByName(string name)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = "SELECT COUNT(1) FROM dbo.Characters WHERE Name = @Name";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// 获取某个用户当前已有角色数量
        /// </summary>
        public int GetCharacterCountByUserId(int userId)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = "SELECT COUNT(1) FROM dbo.Characters WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 创建角色，并返回新角色ID
        /// </summary>
        public int Insert(CharacterEntity entity)
        {
            using (SqlConnection conn = DbHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    INSERT INTO dbo.Characters
                    (
                        UserId, Name, Profession, Level, Exp, Gold,
                        Strength, Agility, Intelligence,
                        CritRate, CritDamage, Defense,
                        Hp, Mp, MaxHp, MaxMp,
                        MapId, PosX, PosY, PosZ
                    )
                    VALUES
                    (
                        @UserId, @Name, @Profession, @Level, @Exp, @Gold,
                        @Strength, @Agility, @Intelligence,
                        @CritRate, @CritDamage, @Defense,
                        @Hp, @Mp, @MaxHp, @MaxMp,
                        @MapId, @PosX, @PosY, @PosZ
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", entity.UserId);
                    cmd.Parameters.AddWithValue("@Name", entity.Name);
                    cmd.Parameters.AddWithValue("@Profession", entity.Profession);
                    cmd.Parameters.AddWithValue("@Level", entity.Level);
                    cmd.Parameters.AddWithValue("@Exp", 0);
                    cmd.Parameters.AddWithValue("@Gold", entity.Gold);

                    cmd.Parameters.AddWithValue("@Strength", entity.Strength);
                    cmd.Parameters.AddWithValue("@Agility", entity.Agility);
                    cmd.Parameters.AddWithValue("@Intelligence", entity.Intelligence);

                    cmd.Parameters.AddWithValue("@CritRate", entity.CritRate);
                    cmd.Parameters.AddWithValue("@CritDamage", entity.CritDamage);
                    cmd.Parameters.AddWithValue("@Defense", entity.Defense);

                    cmd.Parameters.AddWithValue("@Hp", entity.Hp);
                    cmd.Parameters.AddWithValue("@Mp", entity.Mp);
                    cmd.Parameters.AddWithValue("@MaxHp", entity.MaxHp);
                    cmd.Parameters.AddWithValue("@MaxMp", entity.MaxMp);

                    cmd.Parameters.AddWithValue("@MapId", entity.MapId);
                    cmd.Parameters.AddWithValue("@PosX", entity.PosX);
                    cmd.Parameters.AddWithValue("@PosY", entity.PosY);
                    cmd.Parameters.AddWithValue("@PosZ", entity.PosZ);

                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}