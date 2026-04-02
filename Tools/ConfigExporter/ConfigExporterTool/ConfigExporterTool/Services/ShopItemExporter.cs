using ConfigExporter.Models;
using ConfigExporter.Services.Core;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services
{
    public class ShopItemExporter : ExcelExporterBase<ShopItemConfig>
    {
        protected override ShopItemConfig ParseRow(ExcelSheetReader reader, Row row)
        {
            return new ShopItemConfig
            {
                ShopId = reader.GetInt(row, "ShopId"),
                ItemId = reader.GetInt(row, "ItemId"),
                Price = reader.GetInt(row, "Price"),
                IsLimited = reader.GetInt(row, "IsLimited"),
                LimitCount = reader.GetInt(row, "LimitCount"),
                Sort = reader.GetInt(row, "Sort")
            };
        }
    }
}