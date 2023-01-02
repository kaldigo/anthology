namespace Anthology.Utils
{
    public static class FileUtils
    {
        public static string GetConfigPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Config/");
#else
            var rootPath = "/config/";
#endif

            Directory.CreateDirectory(rootPath);

            return rootPath;
        }
        public static string GetMediaPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Media/");
#else
            var rootPath = "/data/";
#endif

            Directory.CreateDirectory(rootPath);

            return rootPath;
        }
        public static string GetDownloadPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Downloads/");
#else
            var rootPath = "/downloads/";
#endif

            Directory.CreateDirectory(rootPath);

            return rootPath;
        }
    }
}
