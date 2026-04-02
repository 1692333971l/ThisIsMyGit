using ConfigExporter.Models;
using ConfigExporter.Services.Core;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services
{
    public class NpcExporter : ExcelExporterBase<NpcConfig>
    {
        protected override NpcConfig ParseRow(ExcelSheetReader reader, Row row)
        {
            return new NpcConfig
            {
                NpcId = reader.GetInt(row, "NpcId"),
                NpcName = reader.GetString(row, "NpcName"),
                HasTask = reader.GetInt(row, "HasTask"),
                TaskId = reader.GetInt(row, "TaskId"),
                HasShop = reader.GetInt(row, "HasShop"),
                ShopId = reader.GetInt(row, "ShopId")
            };
        }
    }
}