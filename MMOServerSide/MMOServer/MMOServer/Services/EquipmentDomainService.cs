using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using Protocol;

namespace MMOServer.Services.Common
{
    /// <summary>
    /// 装备领域公共服务
    /// 负责：
    /// 1. 获取角色当前装备列表
    /// 2. 构建协议层装备列表 EquipmentItemInfo
    /// 3. 计算装备总属性加成
    /// 4. 根据角色基础属性 + 装备加成，生成最终 CharacterInfo
    /// </summary>
    public class EquipmentDomainService
    {
        private readonly EquipmentRepository _equipmentRepository;

        public EquipmentDomainService()
        {
            _equipmentRepository = new EquipmentRepository();
        }

        /// <summary>
        /// 获取角色当前全部装备记录
        /// </summary>
        public List<CharacterEquipmentEntity> GetEquipmentList(int characterId)
        {
            return _equipmentRepository.GetEquipmentListByCharacterId(characterId);
        }

        /// <summary>
        /// 获取角色指定槽位装备
        /// </summary>
        public CharacterEquipmentEntity GetEquipmentBySlotType(int characterId, int equipSlotType)
        {
            return _equipmentRepository.GetByCharacterIdAndSlotType(characterId, equipSlotType);
        }

        /// <summary>
        /// 构建协议层装备列表
        /// 
        /// 说明：
        /// 当前先固定返回 1~6 六个槽位：
        /// 1 = 武器
        /// 2 = 头部
        /// 3 = 衣服
        /// 4 = 手部
        /// 5 = 腿部
        /// 6 = 饰品
        /// 
        /// 如果某个槽位没有装备，则 ItemId 返回 0
        /// </summary>
        public List<EquipmentItemInfo> BuildEquipmentItemInfoList(int characterId)
        {
            List<CharacterEquipmentEntity> entityList = _equipmentRepository.GetEquipmentListByCharacterId(characterId);

            Dictionary<int, int> equipDict = new Dictionary<int, int>();
            foreach (CharacterEquipmentEntity entity in entityList)
            {
                equipDict[entity.EquipSlotType] = entity.ItemId;
            }

            List<EquipmentItemInfo> result = new List<EquipmentItemInfo>();

            for (int slotType = 1; slotType <= 6; slotType++)
            {
                equipDict.TryGetValue(slotType, out int itemId);

                result.Add(new EquipmentItemInfo
                {
                    EquipSlotType = slotType,
                    ItemId = itemId
                });
            }

            return result;
        }

        /// <summary>
        /// 计算角色当前装备总加成
        /// </summary>
        public EquipmentBonusInfo CalculateEquipmentBonus(int characterId)
        {
            List<CharacterEquipmentEntity> entityList = _equipmentRepository.GetEquipmentListByCharacterId(characterId);

            EquipmentBonusInfo bonusInfo = new EquipmentBonusInfo();

            foreach (CharacterEquipmentEntity entity in entityList)
            {
                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(entity.ItemId);
                if (itemConfig == null)
                {
                    throw new Exception($"装备道具配置不存在，ItemId = {entity.ItemId}");
                }

                bonusInfo.AddStrength += itemConfig.AddStrength;
                bonusInfo.AddAgility += itemConfig.AddAgility;
                bonusInfo.AddIntelligence += itemConfig.AddIntelligence;
                bonusInfo.AddDefense += itemConfig.AddDefense;
                bonusInfo.AddMaxHp += itemConfig.AddMaxHp;
                bonusInfo.AddMaxMp += itemConfig.AddMaxMp;
                bonusInfo.AddCritRate += itemConfig.AddCritRate;
                bonusInfo.AddCritDamage += itemConfig.AddCritDamage;
            }

            return bonusInfo;
        }

        /// <summary>
        /// 根据角色基础属性 + 当前装备总加成，构建最终角色信息
        /// </summary>
        public CharacterInfo BuildFinalCharacterInfo(CharacterEntity character)
        {
            if (character == null)
            {
                return null;
            }

            EquipmentBonusInfo bonusInfo = CalculateEquipmentBonus(character.Id);

            CharacterInfo result = new CharacterInfo
            {
                CharacterId = character.Id,
                UserId = character.UserId,
                Name = character.Name,
                Profession = character.Profession,
                Level = character.Level,
                Exp = character.Exp,
                Gold = character.Gold,

                Strength = character.Strength + bonusInfo.AddStrength,
                Agility = character.Agility + bonusInfo.AddAgility,
                Intelligence = character.Intelligence + bonusInfo.AddIntelligence,

                Defense = character.Defense + bonusInfo.AddDefense,

                // 你当前协议里是 decimal，这里先直接按“整数增量”叠加
                CritRate = character.CritRate + bonusInfo.AddCritRate,
                CritDamage = character.CritDamage + bonusInfo.AddCritDamage,

                MaxHp = character.MaxHp + bonusInfo.AddMaxHp,
                MaxMp = character.MaxMp + bonusInfo.AddMaxMp,

                // 当前 HP / MP 先沿用数据库当前值
                // 如果以后想做“装备后超过上限自动截断”逻辑，可以在这里补
                Hp = character.Hp,
                Mp = character.Mp,

                MapId = character.MapId,
                PosX = character.PosX,
                PosY = character.PosY,
                PosZ = character.PosZ
            };

            // 保护：当前 HP / MP 不允许超过最终上限
            if (result.Hp > result.MaxHp)
            {
                result.Hp = result.MaxHp;
            }

            if (result.Mp > result.MaxMp)
            {
                result.Mp = result.MaxMp;
            }

            return result;
        }

        /// <summary>
        /// 判断某个道具是否是可装备道具
        /// </summary>
        public bool IsEquipItem(int itemId)
        {
            ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(itemId);
            if (itemConfig == null)
            {
                return false;
            }

            return itemConfig.CanEquip == 1;
        }

        /// <summary>
        /// 获取某个道具对应的装备槽位类型
        /// </summary>
        public int GetEquipSlotType(int itemId)
        {
            ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(itemId);
            if (itemConfig == null)
            {
                throw new Exception($"道具配置不存在，ItemId = {itemId}");
            }

            return itemConfig.EquipSlotType;
        }
    }

    /// <summary>
    /// 装备加成汇总信息
    /// </summary>
    public class EquipmentBonusInfo
    {
        public int AddStrength { get; set; }
        public int AddAgility { get; set; }
        public int AddIntelligence { get; set; }
        public int AddDefense { get; set; }
        public int AddMaxHp { get; set; }
        public int AddMaxMp { get; set; }
        public int AddCritRate { get; set; }
        public int AddCritDamage { get; set; }
    }
}