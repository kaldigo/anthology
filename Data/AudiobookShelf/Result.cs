namespace Anthology.Data.AudiobookShelf
{
    public class Result
    {
        public string id;
        public string ino;
        public string libraryId;
        public string folderId;
        public string path;
        public string relPath;
        public bool isFile;
        public object mtimeMs;
        public object ctimeMs;
        public int birthtimeMs;
        public object addedAt;
        public object updatedAt;
        public object lastScan;
        public string scanVersion;
        public bool isMissing;
        public bool isInvalid;
        public string mediaType;
        public Media media;
        public List<LibraryFile> libraryFiles;
    }
}
