using ConfigExporter.Services;
using ConfigExporter.Services.Core;

namespace ConfigExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string repoRootDir = ExportPathHelper.GetRepoRoot();

                ExportProfession(repoRootDir);
                ExportItem(repoRootDir);
                ExportNpc(repoRootDir);
                ExportShopItem(repoRootDir);
                ExportMapPortal(repoRootDir);

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

        private static void ExportProfession(string repoRootDir)
        {
            string excelPath = ExportPathHelper.GetExcelPath(repoRootDir, "Profession.xlsx");
            string clientPath = ExportPathHelper.GetClientOutputPath(repoRootDir, "ProfessionConfig.json");
            string serverPath = ExportPathHelper.GetServerOutputPath(repoRootDir, "ProfessionConfig.json");

            ProfessionExporter exporter = new ProfessionExporter();
            exporter.Export(excelPath, clientPath, serverPath);
        }

        private static void ExportItem(string repoRootDir)
        {
            string excelPath = ExportPathHelper.GetExcelPath(repoRootDir, "Item.xlsx");
            string clientPath = ExportPathHelper.GetClientOutputPath(repoRootDir, "ItemConfig.json");
            string serverPath = ExportPathHelper.GetServerOutputPath(repoRootDir, "ItemConfig.json");

            ItemExporter exporter = new ItemExporter();
            exporter.Export(excelPath, clientPath, serverPath);
        }
        private static void ExportNpc(string repoRootDir)
        {
            string excelPath = ExportPathHelper.GetExcelPath(repoRootDir, "Npc.xlsx");
            string clientPath = ExportPathHelper.GetClientOutputPath(repoRootDir, "NpcConfig.json");
            string serverPath = ExportPathHelper.GetServerOutputPath(repoRootDir, "NpcConfig.json");

            NpcExporter exporter = new NpcExporter();
            exporter.Export(excelPath, clientPath, serverPath);
        }
        private static void ExportShopItem(string repoRootDir)
        {
            string excelPath = ExportPathHelper.GetExcelPath(repoRootDir, "ShopItem.xlsx");
            string clientPath = ExportPathHelper.GetClientOutputPath(repoRootDir, "ShopItemConfig.json");
            string serverPath = ExportPathHelper.GetServerOutputPath(repoRootDir, "ShopItemConfig.json");

            ShopItemExporter exporter = new ShopItemExporter();
            exporter.Export(excelPath, clientPath, serverPath);
        }
        private static void ExportMapPortal(string repoRootDir)
        {
            string excelPath = ExportPathHelper.GetExcelPath(repoRootDir, "MapPortal.xlsx");
            string clientPath = ExportPathHelper.GetClientOutputPath(repoRootDir, "MapPortalConfig.json");
            string serverPath = ExportPathHelper.GetServerOutputPath(repoRootDir, "MapPortalConfig.json");

            MapPortalExporter exporter = new MapPortalExporter();
            exporter.Export(excelPath, clientPath, serverPath);
        }
    }
}