using ConfigExporter.Models;
using ConfigExporter.Services.Core;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services
{
    public class ItemExporter : ExcelExporterBase<ItemConfig>
    {
        protected override ItemConfig ParseRow(ExcelSheetReader reader, Row row)
        {
            return new ItemConfig
            {
                ItemId = reader.GetInt(row, "ItemId"),
                ItemName = reader.GetString(row, "ItemName"),
                ItemType = reader.GetInt(row, "ItemType"),
                MaxStackCount = reader.GetInt(row, "MaxStackCount"),
                SellPrice = reader.GetInt(row, "SellPrice"),
                Quality = reader.GetInt(row, "Quality"),
                IconPath = reader.GetString(row, "IconPath"),
                Description = reader.GetString(row, "Description"),

                // 使用相关
                CanUse = reader.GetInt(row, "CanUse"),
                UseEffectType = reader.GetInt(row, "UseEffectType"),
                UseEffectValue = reader.GetInt(row, "UseEffectValue"),

                // 装备相关
                CanEquip = reader.GetInt(row, "CanEquip"),
                EquipSlotType = reader.GetInt(row, "EquipSlotType"),

                // 装备加成属性
                AddStrength = reader.GetInt(row, "AddStrength"),
                AddAgility = reader.GetInt(row, "AddAgility"),
                AddIntelligence = reader.GetInt(row, "AddIntelligence"),
                AddDefense = reader.GetInt(row, "AddDefense"),
                AddMaxHp = reader.GetInt(row, "AddMaxHp"),
                AddMaxMp = reader.GetInt(row, "AddMaxMp"),
                AddCritRate = reader.GetInt(row, "AddCritRate"),
                AddCritDamage = reader.GetInt(row, "AddCritDamage")
            };
        }
    }
}