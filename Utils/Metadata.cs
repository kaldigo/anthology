using Anthology.Data.Metadata;
using ReadarrBook = Anthology.Data.Readarr.Book;
using AudibleBook = Anthology.Data.Audible.Book;
using AudiobookGuildBook = Anthology.Data.AudiobookGuild.Book;
using DBBook = Anthology.Data.DB.Book;
using System.Text.RegularExpressions;

namespace Anthology.Utils
{
    public static class Metadata
    {
        public static void ConvertDBBook(Book metadata, DBBook dbBook)
        {
            metadata.ISBN = dbBook.ISBN;
            metadata.Title = dbBook.Title;
            metadata.Subtitle = dbBook.Subtitle;
            metadata.Authors = dbBook.Authors.Select(a => a.Name).ToList();
            metadata.Narrators = dbBook.Narrators.Select(n => n.Name).ToList();
            metadata.Series = dbBook.Series.Select(s => new Series() { Name = s.Name, Sequence = s.Sequence}).ToList();
            metadata.Description = dbBook.Description;
            metadata.Publisher = dbBook.Publisher;
            metadata.PublishDate = dbBook.PublishDate;
            metadata.Genres = dbBook.Genres.Select(g => g.Name).ToList();
            metadata.Tags = dbBook.Tags.Select(t => t.Name).ToList();
            metadata.Language = dbBook.Language;
            metadata.IsExplicit = dbBook.IsExplicit;
            metadata.Covers = dbBook.BookCovers.Select(c => c.GetUrl()).ToList();
    }
        public static void ConvertReadarrBook(Book metadata, ReadarrBook readarrBook)
        {
            metadata.Title = readarrBook.title;
            metadata.Authors = new List<string>() { { readarrBook.author.authorName } };
            if(!string.IsNullOrWhiteSpace(readarrBook.seriesTitle)) metadata.Series = Readarr.ExtractSeries(readarrBook.seriesTitle.Split(";").ToList());
            metadata.Description = readarrBook.overview;
            metadata.Publisher = readarrBook.editions[0].publisher;
            metadata.PublishDate = readarrBook.releaseDate;
            metadata.Genres = readarrBook.genres;
            metadata.Covers = readarrBook.editions.SelectMany(e => e.images.Select(i => i.url)).Take(10).ToList();
        }
        public static void ConvertAudibleBook(Book metadata, AudibleBook audibleBook)
        {
            metadata.ASIN = audibleBook.asin;
            metadata.Title = audibleBook.title;
            metadata.Subtitle = audibleBook.subtitle;
            metadata.Authors = audibleBook.authors.Select(a => a.name).ToList();
            metadata.Narrators = audibleBook.narrators.Select(n => n.name).ToList();
            metadata.Series = new List<Series>();
            if (audibleBook.seriesPrimary != null)
            {
                float primarySeriesIndex;
                float.TryParse(audibleBook.seriesPrimary.position, out primarySeriesIndex);
                metadata.Series.Add(new Series() { Name = audibleBook.seriesPrimary.name, Sequence = primarySeriesIndex });
            }
            if (audibleBook.seriesSecondary != null)
            {
                float secondarySeriesIndex;
                float.TryParse(audibleBook.seriesSecondary.position, out secondarySeriesIndex);
                metadata.Series.Add(new Series() { Name = audibleBook.seriesSecondary.name, Sequence = secondarySeriesIndex });
            }
            metadata.Description = audibleBook.summary;
            metadata.Publisher = audibleBook.publisherName;
            metadata.PublishDate = audibleBook.releaseDate;
            metadata.Genres = audibleBook.genres.Where(g => g.type == "genre").Select(g => g.name).ToList();
            metadata.Tags = audibleBook.genres.Where(t => t.type == "tag").Select(t => t.name).ToList();
            metadata.Language = audibleBook.language;
            metadata.Covers = new List<string>() { { audibleBook.image } };
        }
        public static void ConvertAudiobookGuildBook(Book metadata, AudiobookGuildBook audiobookGuildBook)
        {
            metadata.Title = audiobookGuildBook.title;
            metadata.Authors = audiobookGuildBook.GetTags("Author");
            metadata.Narrators = audiobookGuildBook.GetTags("Narrator");
            metadata.Series = audiobookGuildBook.GetTags("Series").Select(s => new Series() { Name = s }).ToList();
            metadata.Description = Regex.Replace(Regex.Unescape(audiobookGuildBook.body_html).Replace("<br>", "\n"), "<.*?>", String.Empty).Replace("\u00A0", " ").Replace("OVERVIEW:", "").Replace("Looking for the ebook?", "").Replace("Find it on Amazon", "").Trim().Trim( '\r', '\n', ' ' ).Trim();
            metadata.Publisher = String.Join(", ", audiobookGuildBook.GetTags("Author"));
            metadata.PublishDate = audiobookGuildBook.published_at;
            metadata.Genres = audiobookGuildBook.GetTags("Genre");
            metadata.Tags = audiobookGuildBook.GetTags("Trope");
            metadata.Covers = audiobookGuildBook.images.Select(i => Regex.Unescape(i.src)).ToList();
        }
        public static string SelectString(string field, Dictionary<string, Book> sources, bool isAudiobookField = false)
        {
            string combinedValue = null;

            if(sources["Override"][field] != null && !string.IsNullOrWhiteSpace((string)sources["Override"][field]))
            {
                combinedValue = (string)sources["Override"][field];
            }
            else if (isAudiobookField)
            {
                if (sources.ContainsKey("Audible") && sources["Audible"] != null && !string.IsNullOrWhiteSpace((string)sources["Audible"][field])) combinedValue = (string)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && !string.IsNullOrWhiteSpace((string)sources["AudiobookGuild"][field])) combinedValue = (string)sources["AudiobookGuild"][field];
                else if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && !string.IsNullOrWhiteSpace((string)sources["Goodreads"][field])) combinedValue = (string)sources["Goodreads"][field];
            }
            else
            {
                if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && !string.IsNullOrWhiteSpace((string)sources["Goodreads"][field])) combinedValue = (string)sources["Goodreads"][field];
                else if (sources.ContainsKey("Audible") && sources["Audible"] != null && !string.IsNullOrWhiteSpace((string)sources["Audible"][field])) combinedValue = (string)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && !string.IsNullOrWhiteSpace((string)sources["AudiobookGuild"][field])) combinedValue = (string)sources["AudiobookGuild"][field];
            }

            return combinedValue;
        }
        public static bool SelectBool(string field, Dictionary<string, Book> sources, bool isAudiobookField = false)
        {
            bool combinedValue = false;

            if (sources["Override"] != null && (bool)sources["Override"][field])
            {
                combinedValue = (bool)sources["Override"][field];
            }
            else if (isAudiobookField)
            {
                if (sources.ContainsKey("Audible") && sources["Audible"] != null && (bool)sources["Audible"][field]) combinedValue = (bool)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && (bool)sources["AudiobookGuild"][field]) combinedValue = (bool)sources["AudiobookGuild"][field];
                else if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && (bool)sources["Goodreads"][field]) combinedValue = (bool)sources["Goodreads"][field];
            }
            else
            {
                if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && (bool)sources["Goodreads"][field]) combinedValue = (bool)sources["Goodreads"][field];
                else if (sources.ContainsKey("Audible") && sources["Audible"] != null && (bool)sources["Audible"][field]) combinedValue = (bool)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && (bool)sources["AudiobookGuild"][field]) combinedValue = (bool)sources["AudiobookGuild"][field];
            }

            return combinedValue;
        }
        public static List<string> SelectListString(string field, Dictionary<string, Book> sources, bool isAudiobookField = false)
        {
            List<string> combinedValue = new List<string>();

            if (sources["Override"] != null && (List<string>)sources["Override"][field] != null && ((List<string>)sources["Override"][field]).Count != 0)
            {
                combinedValue = (List<string>)sources["Override"][field];
            }
            else if (isAudiobookField)
            {
                if (sources.ContainsKey("Audible") && sources["Audible"] != null && (List<string>)sources["Audible"][field] != null && ((List<string>)sources["Audible"][field]).Count != 0) combinedValue = (List<string>)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && (List<string>)sources["AudiobookGuild"][field] != null && ((List<string>)sources["AudiobookGuild"][field]).Count != 0) combinedValue = (List<string>)sources["AudiobookGuild"][field];
                else if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && (List<string>)sources["Goodreads"][field] != null && ((List<string>)sources["Goodreads"][field]).Count != 0) combinedValue = (List<string>)sources["Goodreads"][field];
            }
            else
            {
                if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && (List<string>)sources["Goodreads"][field] != null && ((List<string>)sources["Goodreads"][field]).Count != 0) combinedValue = (List<string>)sources["Goodreads"][field];
                else if (sources.ContainsKey("Audible") && sources["Audible"] != null && (List<string>)sources["Audible"][field] != null && ((List<string>)sources["Audible"][field]).Count != 0) combinedValue = (List<string>)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && (List<string>)sources["AudiobookGuild"][field] != null && ((List<string>)sources["AudiobookGuild"][field]).Count != 0) combinedValue = (List<string>)sources["AudiobookGuild"][field];
            }

            return combinedValue;
        }
        public static DateTime? SelectDateTime(string field, Dictionary<string, Book> sources, bool isAudiobookField = false)
        {
            DateTime? combinedValue = null;

            if (sources["Override"] != null && sources["Override"][field] != null)
            {
                combinedValue = (DateTime?)sources["Override"][field];
            }
            else if (isAudiobookField)
            {
                if (sources.ContainsKey("Audible") && sources["Audible"] != null && sources["Audible"][field] != null) combinedValue = (DateTime?)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && sources["AudiobookGuild"][field] != null) combinedValue = (DateTime?)sources["AudiobookGuild"][field];
                else if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && sources["Goodreads"][field] != null) combinedValue = (DateTime?)sources["Goodreads"][field];
            }
            else
            {
                if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && sources["Goodreads"][field] != null) combinedValue = (DateTime?)sources["Goodreads"][field];
                else if (sources.ContainsKey("Audible") && sources["Audible"] != null && sources["Audible"][field] != null) combinedValue = (DateTime?)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && sources["AudiobookGuild"][field] != null) combinedValue = (DateTime?)sources["AudiobookGuild"][field];
            }

            return combinedValue;
        }
        public static List<Series> SelectSeries(string field, Dictionary<string, Book> sources, bool isAudiobookField = false)
        {
            List<Series> combinedValue = new List<Series>();

            if (sources["Override"] != null && (List<Series>)sources["Override"][field] != null && ((List<Series>)sources["Override"][field]).Count != 0)
            {
                combinedValue = (List<Series>)sources["Override"][field];
            }
            else if (isAudiobookField)
            {
                if (sources.ContainsKey("Audible") && sources["Audible"] != null && (List<Series>)sources["Audible"][field] != null && ((List<Series>)sources["Audible"][field]).Count != 0) combinedValue = (List<Series>)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && (List<Series>)sources["AudiobookGuild"][field] != null && ((List<Series>)sources["AudiobookGuild"][field]).Count != 0) combinedValue = (List<Series>)sources["AudiobookGuild"][field];
                else if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && (List<Series>)sources["Goodreads"][field] != null && ((List<Series>)sources["Goodreads"][field]).Count != 0) combinedValue = (List<Series>)sources["Goodreads"][field];
            }
            else
            {
                if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && (List<Series>)sources["Goodreads"][field] != null && ((List<Series>)sources["Goodreads"][field]).Count != 0) combinedValue = (List<Series>)sources["Goodreads"][field];
                else if (sources.ContainsKey("Audible") && sources["Audible"] != null && (List<Series>)sources["Audible"][field] != null && ((List<Series>)sources["Audible"][field]).Count != 0) combinedValue = (List<Series>)sources["Audible"][field];
                else if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && (List<Series>)sources["AudiobookGuild"][field] != null && ((List<Series>)sources["AudiobookGuild"][field]).Count != 0) combinedValue = (List<Series>)sources["AudiobookGuild"][field];
            }

            return combinedValue;
        }
        public static List<string> CombineListString(string field, Dictionary<string, Book> sources)
        {
            List<string> combinedValue = new List<string>();

            if (sources["Override"] != null && (List<string>)sources["Override"][field] != null && ((List<string>)sources["Override"][field]).Count != 0) combinedValue.AddRange((List<string>)sources["Override"][field]);
            if (sources.ContainsKey("Goodreads") && sources["Goodreads"] != null && (List<string>)sources["Goodreads"][field] != null && ((List<string>)sources["Goodreads"][field]).Count != 0) combinedValue.AddRange((List<string>)sources["Goodreads"][field]);
            if (sources.ContainsKey("Audible") && sources["Audible"] != null && (List<string>)sources["Audible"][field] != null && ((List<string>)sources["Audible"][field]).Count != 0) combinedValue.AddRange((List<string>)sources["Audible"][field]);
            if (sources.ContainsKey("AudiobookGuild") && sources["AudiobookGuild"] != null && (List<string>)sources["AudiobookGuild"][field] != null && ((List<string>)sources["AudiobookGuild"][field]).Count != 0) combinedValue.AddRange((List<string>)sources["AudiobookGuild"][field]);

            return combinedValue;
        }
        
    }
}
