namespace Anthology.Utils
{
    public static partial class FileUtils
    {
        public static string GetConfigPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Config/");
#else
            var rootPath = "/anthology/config/";
#endif

            Directory.CreateDirectory(rootPath);

            return rootPath;
        }
        public static string GetMediaPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Media/");
#else
            var rootPath = "/anthology/media/";
#endif

            Directory.CreateDirectory(rootPath);

            return rootPath;
        }
        public static string GetDownloadPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Downloads/");
#else
            var rootPath = "/data/";
#endif

            Directory.CreateDirectory(rootPath);

            return rootPath;
        }
        public static string GetTempPath()
        {

#if DEBUG
            var rootPath = Path.GetFullPath("AppData/Temp/");
#else
            var rootPath = "/anthology/temp/";
#endif

            Directory.CreateDirectory(rootPath);

            return rootPath;
        }
    }
}
