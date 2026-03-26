using ConfigExporter.Services;

namespace ConfigExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // 项目根目录推导
                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string toolProjectDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", ".."));
                string repoRootDir = Path.GetFullPath(Path.Combine(toolProjectDir, "..", "..", ".."));

                // ---------------------------
                // Profession.xlsx
                // ---------------------------
                string professionExcelPath = Path.Combine(repoRootDir, "ConfigExcels", "Profession.xlsx");

                string professionClientOutputPath = Path.Combine(
                    repoRootDir,
                    "MMORPG",
                    "Assets",
                    "Resources",
                    "Config",
                    "Generated",
                    "ProfessionConfig.json"
                );

                string professionServerOutputPath = Path.Combine(
                    repoRootDir,
                    "MMOServerSide",
                    "MMOServer",
                    "MMOServer",
                    "Config",
                    "Generated",
                    "ProfessionConfig.json"
                );

                ProfessionExporter professionExporter = new ProfessionExporter();
                professionExporter.Export(professionExcelPath, professionClientOutputPath, professionServerOutputPath);

                // ---------------------------
                // Item.xlsx
                // ---------------------------
                string itemExcelPath = Path.Combine(repoRootDir, "ConfigExcels", "Item.xlsx");

                string itemClientOutputPath = Path.Combine(
                    repoRootDir,
                    "MMORPG",
                    "Assets",
                    "Resources",
                    "Config",
                    "Generated",
                    "ItemConfig.json"
                );

                string itemServerOutputPath = Path.Combine(
                    repoRootDir,
                    "MMOServerSide",
                    "MMOServer",
                    "MMOServer",
                    "Config",
                    "Generated",
                    "ItemConfig.json"
                );

                ItemExporter itemExporter = new ItemExporter();
                itemExporter.Export(itemExcelPath, itemClientOutputPath, itemServerOutputPath);

                Console.WriteLine("全部导出完成。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导表失败：{ex.Message}");
                Console.WriteLine(ex);
            }

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}