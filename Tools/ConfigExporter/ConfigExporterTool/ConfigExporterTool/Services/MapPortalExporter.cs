using ConfigExporter.Models;
using ConfigExporter.Services.Core;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services
{
    public class MapPortalExporter : ExcelExporterBase<MapPortalConfig>
    {
        protected override MapPortalConfig ParseRow(ExcelSheetReader reader, Row row)
        {
            return new MapPortalConfig
            {
                PortalId = reader.GetInt(row, "PortalId"),
                PortalName = reader.GetString(row, "PortalName"),
                FromMapId = reader.GetInt(row, "FromMapId"),
                ToMapId = reader.GetInt(row, "ToMapId"),
                SpawnX = reader.GetFloat(row, "SpawnX"),
                SpawnY = reader.GetFloat(row, "SpawnY"),
                SpawnZ = reader.GetFloat(row, "SpawnZ")
            };
        }
    }
}