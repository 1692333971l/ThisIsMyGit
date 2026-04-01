using ConfigExporter.Models;
using ConfigExporter.Services.Core;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services
{
    public class ProfessionExporter : ExcelExporterBase<ProfessionConfig>
    {
        protected override ProfessionConfig ParseRow(ExcelSheetReader reader, Row row)
        {
            return new ProfessionConfig
            {
                ProfessionId = reader.GetInt(row, "ProfessionId"),
                ProfessionName = reader.GetString(row, "ProfessionName"),
                ModelPath = reader.GetString(row, "ModelPath"),

                Strength = reader.GetInt(row, "Strength"),
                Agility = reader.GetInt(row, "Agility"),
                Intelligence = reader.GetInt(row, "Intelligence"),

                CritRate = reader.GetDecimal(row, "CritRate"),
                CritDamage = reader.GetDecimal(row, "CritDamage"),
                Defense = reader.GetInt(row, "Defense"),

                Hp = reader.GetInt(row, "Hp"),
                Mp = reader.GetInt(row, "Mp"),
                MaxHp = reader.GetInt(row, "MaxHp"),
                MaxMp = reader.GetInt(row, "MaxMp"),

                MapId = reader.GetInt(row, "MapId"),
                PosX = reader.GetFloat(row, "PosX"),
                PosY = reader.GetFloat(row, "PosY"),
                PosZ = reader.GetFloat(row, "PosZ")
            };
        }
    }
}