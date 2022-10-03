namespace Anthology.Data.AudiobookShelf
{
    public class AudioFile
    {
        public int index;
        public string ino;
        public Metadata metadata;
        public object addedAt;
        public object updatedAt;
        public int? trackNumFromMeta;
        public object discNumFromMeta;
        public int? trackNumFromFilename;
        public object discNumFromFilename;
        public bool manuallyVerified;
        public bool invalid;
        public bool exclude;
        public object error;
        public string format;
        public double duration;
        public int bitRate;
        public string language;
        public string codec;
        public string timeBase;
        public int channels;
        public string channelLayout;
        public List<Chapter> chapters;
        public string embeddedCoverArt;
        public MetaTags metaTags;
        public string mimeType;
    }
}
