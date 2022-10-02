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

            return rootPath;
        }
        public static string GetMediaPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Media/");
#else
            var rootPath = "/data/";
#endif

            return rootPath;
        }
        public static string GetDownloadPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Downloads/");
#else
            var rootPath = "/downloads/";
#endif

            return rootPath;
        }
    }
}
