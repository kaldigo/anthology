namespace Anthology.Data.AudiobookShelf
{
    public class Media
    {
        public string libraryItemId;
        public Metadata metadata;
        public string coverPath;
        public List<string> tags;
        public List<AudioFile> audioFiles;
        public List<Chapter> chapters;
        public List<object> missingParts;
        public object ebookFile;
    }
}
