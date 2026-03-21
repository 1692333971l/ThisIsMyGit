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

                string excelPath = Path.Combine(repoRootDir, "ConfigExcels", "Profession.xlsx");

                string clientOutputPath = Path.Combine(
                    repoRootDir,
                    "MMORPG",
                    "Assets",
                    "Resources",
                    "Config",
                    "Generated",
                    "ProfessionConfig.json"
                );

                string serverOutputPath = Path.Combine(
                    repoRootDir,
                    "MMOServerSide",
                    "MMOServer",
                    "MMOServer",
                    "Config",
                    "Generated",
                    "ProfessionConfig.json"
                );

                ProfessionExporter exporter = new ProfessionExporter();
                exporter.Export(excelPath, clientOutputPath, serverOutputPath);

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