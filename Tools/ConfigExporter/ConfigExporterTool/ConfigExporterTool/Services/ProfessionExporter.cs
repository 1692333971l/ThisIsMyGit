using System.Globalization;
using System.Text.Json;
using ConfigExporter.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services
{
    public class ProfessionExporter
    {
        /// <summary>
        /// 读取 Profession.xlsx 并导出到两个目标路径
        /// </summary>
        public void Export(string excelPath, string clientOutputPath, string serverOutputPath)
        {
            List<ProfessionConfig> professionConfigs = ReadProfessionConfigs(excelPath);

            string json = JsonSerializer.Serialize(professionConfigs, new JsonSerializerOptions
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

        /// <summary>
        /// 从 Excel 中读取职业配置列表
        /// </summary>
        private List<ProfessionConfig> ReadProfessionConfigs(string excelPath)
        {
            List<ProfessionConfig> result = new List<ProfessionConfig>();

            using SpreadsheetDocument document = SpreadsheetDocument.Open(excelPath, false);
            WorkbookPart workbookPart = document.WorkbookPart!;
            SharedStringTablePart? sharedStringPart = workbookPart.SharedStringTablePart;

            Sheet firstSheet = workbookPart.Workbook.Sheets!.Elements<Sheet>().First();
            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id!);
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;

            List<Row> rows = sheetData.Elements<Row>().ToList();

            if (rows.Count < 3)
            {
                throw new Exception("Profession.xlsx 至少需要 3 行：说明行、字段行、数据行。");
            }

            // 第2行是字段名行（索引1）
            Row fieldRow = rows[1];
            Dictionary<string, int> fieldIndexMap = BuildFieldIndexMap(fieldRow, sharedStringPart);

            // 第3行开始是数据行
            for (int i = 2; i < rows.Count; i++)
            {
                Row row = rows[i];

                // 跳过空行
                if (IsRowEmpty(row))
                {
                    continue;
                }

                ProfessionConfig config = new ProfessionConfig
                {
                    ProfessionId = GetIntCellValue(row, fieldIndexMap, "ProfessionId", sharedStringPart),
                    ProfessionName = GetStringCellValue(row, fieldIndexMap, "ProfessionName", sharedStringPart),
                    ModelPath = GetStringCellValue(row, fieldIndexMap, "ModelPath", sharedStringPart),

                    Strength = GetIntCellValue(row, fieldIndexMap, "Strength", sharedStringPart),
                    Agility = GetIntCellValue(row, fieldIndexMap, "Agility", sharedStringPart),
                    Intelligence = GetIntCellValue(row, fieldIndexMap, "Intelligence", sharedStringPart),

                    CritRate = GetDecimalCellValue(row, fieldIndexMap, "CritRate", sharedStringPart),
                    CritDamage = GetDecimalCellValue(row, fieldIndexMap, "CritDamage", sharedStringPart),
                    Defense = GetIntCellValue(row, fieldIndexMap, "Defense", sharedStringPart),

                    Hp = GetIntCellValue(row, fieldIndexMap, "Hp", sharedStringPart),
                    Mp = GetIntCellValue(row, fieldIndexMap, "Mp", sharedStringPart),
                    MaxHp = GetIntCellValue(row, fieldIndexMap, "MaxHp", sharedStringPart),
                    MaxMp = GetIntCellValue(row, fieldIndexMap, "MaxMp", sharedStringPart),

                    MapId = GetIntCellValue(row, fieldIndexMap, "MapId", sharedStringPart),
                    PosX = GetFloatCellValue(row, fieldIndexMap, "PosX", sharedStringPart),
                    PosY = GetFloatCellValue(row, fieldIndexMap, "PosY", sharedStringPart),
                    PosZ = GetFloatCellValue(row, fieldIndexMap, "PosZ", sharedStringPart)
                };

                result.Add(config);
            }

            return result;
        }

        /// <summary>
        /// 根据字段行建立字段名 -> 列索引映射
        /// </summary>
        private Dictionary<string, int> BuildFieldIndexMap(Row fieldRow, SharedStringTablePart? sharedStringPart)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            List<Cell> cells = GetRowCells(fieldRow);

            for (int i = 0; i < cells.Count; i++)
            {
                string fieldName = GetCellValue(cells[i], sharedStringPart).Trim();
                if (!string.IsNullOrEmpty(fieldName))
                {
                    map[fieldName] = i;
                }
            }

            return map;
        }

        /// <summary>
        /// 获取一行中按列顺序展开后的单元格列表
        /// </summary>
        private List<Cell> GetRowCells(Row row)
        {
            List<Cell> result = new List<Cell>();
            int currentColumnIndex = 0;

            foreach (Cell cell in row.Elements<Cell>())
            {
                string cellReference = cell.CellReference!.Value!;
                int cellColumnIndex = GetColumnIndexFromCellReference(cellReference);

                while (currentColumnIndex < cellColumnIndex)
                {
                    result.Add(new Cell());
                    currentColumnIndex++;
                }

                result.Add(cell);
                currentColumnIndex++;
            }

            return result;
        }

        /// <summary>
        /// 从单元格引用（如 C2）获取列索引（从0开始）
        /// </summary>
        private int GetColumnIndexFromCellReference(string cellReference)
        {
            string columnPart = new string(cellReference.Where(char.IsLetter).ToArray());
            int columnIndex = 0;

            foreach (char c in columnPart)
            {
                columnIndex *= 26;
                columnIndex += (c - 'A' + 1);
            }

            return columnIndex - 1;
        }

        /// <summary>
        /// 获取单元格字符串值
        /// </summary>
        private string GetCellValue(Cell cell, SharedStringTablePart? sharedStringPart)
        {
            if (cell == null || cell.CellValue == null)
            {
                return string.Empty;
            }

            string value = cell.CellValue.InnerText;

            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                return sharedStringPart?.SharedStringTable?.ElementAt(int.Parse(value))?.InnerText ?? string.Empty;
            }

            return value;
        }

        private string GetStringCellValue(Row row, Dictionary<string, int> fieldIndexMap, string fieldName, SharedStringTablePart? sharedStringPart)
        {
            Cell cell = GetCellByFieldName(row, fieldIndexMap, fieldName);
            return GetCellValue(cell, sharedStringPart).Trim();
        }

        private int GetIntCellValue(Row row, Dictionary<string, int> fieldIndexMap, string fieldName, SharedStringTablePart? sharedStringPart)
        {
            string value = GetStringCellValue(row, fieldIndexMap, fieldName, sharedStringPart);
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        private float GetFloatCellValue(Row row, Dictionary<string, int> fieldIndexMap, string fieldName, SharedStringTablePart? sharedStringPart)
        {
            string value = GetStringCellValue(row, fieldIndexMap, fieldName, sharedStringPart);
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private decimal GetDecimalCellValue(Row row, Dictionary<string, int> fieldIndexMap, string fieldName, SharedStringTablePart? sharedStringPart)
        {
            string value = GetStringCellValue(row, fieldIndexMap, fieldName, sharedStringPart);
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        private Cell GetCellByFieldName(Row row, Dictionary<string, int> fieldIndexMap, string fieldName)
        {
            if (!fieldIndexMap.TryGetValue(fieldName, out int index))
            {
                throw new Exception($"字段不存在：{fieldName}");
            }

            List<Cell> cells = GetRowCells(row);

            if (index >= cells.Count)
            {
                throw new Exception($"字段 {fieldName} 在当前行中没有对应单元格。");
            }

            return cells[index];
        }

        private bool IsRowEmpty(Row row)
        {
            return !row.Elements<Cell>().Any(c => !string.IsNullOrWhiteSpace(c.InnerText));
        }

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