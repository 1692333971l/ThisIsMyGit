namespace ConfigExporter.Services.Core
{
    public static class ExportPathHelper
    {
        public static string GetRepoRoot()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string toolProjectDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", ".."));
            return Path.GetFullPath(Path.Combine(toolProjectDir, "..", "..", ".."));
        }

        public static string GetExcelPath(string repoRootDir, string excelFileName)
        {
            return Path.Combine(repoRootDir, "ConfigExcels", excelFileName);
        }

        public static string GetClientOutputPath(string repoRootDir, string jsonFileName)
        {
            return Path.Combine(
                repoRootDir,
                "MMORPG",
                "Assets",
                "Resources",
                "Config",
                "Generated",
                jsonFileName
            );
        }

        public static string GetServerOutputPath(string repoRootDir, string jsonFileName)
        {
            return Path.Combine(
                repoRootDir,
                "MMOServerSide",
                "MMOServer",
                "MMOServer",
                "Config",
                "Generated",
                jsonFileName
            );
        }
    }
}