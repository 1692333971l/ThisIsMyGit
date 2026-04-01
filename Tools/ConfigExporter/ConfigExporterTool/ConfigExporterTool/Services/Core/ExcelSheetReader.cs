using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ConfigExporter.Services.Core
{
    public class ExcelSheetReader : IDisposable
    {
        private readonly SpreadsheetDocument _document;
        private readonly SharedStringTablePart? _sharedStringPart;
        private readonly List<Row> _rows;
        private readonly Dictionary<string, int> _fieldIndexMap;

        public IReadOnlyList<Row> Rows => _rows;

        public ExcelSheetReader(string excelPath)
        {
            _document = SpreadsheetDocument.Open(excelPath, false);

            WorkbookPart workbookPart = _document.WorkbookPart!;
            _sharedStringPart = workbookPart.SharedStringTablePart;

            Sheet firstSheet = workbookPart.Workbook.Sheets!.Elements<Sheet>().First();
            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id!);
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;

            _rows = sheetData.Elements<Row>().ToList();

            if (_rows.Count < 3)
            {
                throw new Exception($"{Path.GetFileName(excelPath)} 至少需要 3 行：说明行、字段行、数据行。");
            }

            Row fieldRow = _rows[1];
            _fieldIndexMap = BuildFieldIndexMap(fieldRow);
        }

        public bool IsRowEmpty(Row row)
        {
            return !row.Elements<Cell>().Any(c => !string.IsNullOrWhiteSpace(c.InnerText));
        }

        public string GetString(Row row, string fieldName)
        {
            Cell cell = GetCellByFieldName(row, fieldName);
            return GetCellValue(cell).Trim();
        }

        public int GetInt(Row row, string fieldName)
        {
            return int.Parse(GetString(row, fieldName), CultureInfo.InvariantCulture);
        }

        public float GetFloat(Row row, string fieldName)
        {
            return float.Parse(GetString(row, fieldName), CultureInfo.InvariantCulture);
        }

        public decimal GetDecimal(Row row, string fieldName)
        {
            return decimal.Parse(GetString(row, fieldName), CultureInfo.InvariantCulture);
        }

        private Dictionary<string, int> BuildFieldIndexMap(Row fieldRow)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            List<Cell> cells = GetRowCells(fieldRow);

            for (int i = 0; i < cells.Count; i++)
            {
                string fieldName = GetCellValue(cells[i]).Trim();
                if (!string.IsNullOrEmpty(fieldName))
                {
                    map[fieldName] = i;
                }
            }

            return map;
        }

        private List<Cell> GetRowCells(Row row)
        {
            List<Cell> result = new List<Cell>();
            int currentColumnIndex = 0;

            foreach (Cell cell in row.Elements<Cell>())
            {
                string cellReference = cell.CellReference?.Value ?? string.Empty;
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

        private string GetCellValue(Cell cell)
        {
            if (cell == null || cell.CellValue == null)
            {
                return string.Empty;
            }

            string value = cell.CellValue.InnerText;

            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                return _sharedStringPart?.SharedStringTable?.ElementAt(int.Parse(value))?.InnerText ?? string.Empty;
            }

            return value;
        }

        private Cell GetCellByFieldName(Row row, string fieldName)
        {
            if (!_fieldIndexMap.TryGetValue(fieldName, out int index))
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

        public void Dispose()
        {
            _document.Dispose();
        }
    }
}