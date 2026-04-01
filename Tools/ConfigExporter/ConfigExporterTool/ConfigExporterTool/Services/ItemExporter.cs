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
                CanUse = reader.GetInt(row, "CanUse"),
                UseEffectType = reader.GetInt(row, "UseEffectType"),
                UseEffectValue = reader.GetInt(row, "UseEffectValue")
            };
        }
    }
}