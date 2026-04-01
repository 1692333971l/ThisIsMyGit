using System.Text.Json;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services.Core
{
    public abstract class ExcelExporterBase<T>
    {
        public void Export(string excelPath, string clientOutputPath, string serverOutputPath)
        {
            List<T> configs = ReadConfigs(excelPath);

            string json = JsonSerializer.Serialize(configs, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            EnsureDirectory(clientOutputPath);
            EnsureDirectory(serverOutputPath);

            File.WriteAllText(clientOutputPath, json);
            File.WriteAllText(serverOutputPath, json);

            Console.WriteLine($"导出成功：{clientOutputPath}");
            Console.WriteLine($"导出成功：{serverOutputPath}");
        }

        private List<T> ReadConfigs(string excelPath)
        {
            List<T> result = new List<T>();

            using ExcelSheetReader reader = new ExcelSheetReader(excelPath);

            for (int i = 2; i < reader.Rows.Count; i++)
            {
                Row row = reader.Rows[i];

                if (reader.IsRowEmpty(row))
                {
                    continue;
                }

                T config = ParseRow(reader, row);
                result.Add(config);
            }

            return result;
        }

        protected abstract T ParseRow(ExcelSheetReader reader, Row row);

        private void EnsureDirectory(string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}