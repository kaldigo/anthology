using Anthology.Plugins;
using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Plugins.MetadataSources
{
    internal class Audible : IMetadataSource
    {
        public string Name => "Audible";

        public string IdentifierKey => "ASIN";

        public List<string> Settings => new List<string>();

        public Metadata GetMetadata(string identifier, Dictionary<string, string> settings)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var url = "https://api.audnex.us/books/" + identifier;
                    var response = client.GetStringAsync(url).Result;
                    if (response != null)
                    {
                        var jsonString = JsonConvert.DeserializeObject<AudibleBook>(response);
                        return ConvertImportToMetadata(jsonString);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Metadata> Search(Dictionary<string, string> settings, string title, string author = null)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "https://api.audible.com//1.0/catalog/products?num_results=25&products_sort_by=Relevance&title=" + title;
                if (author != null) url = url + "&author=" + author;
                var response = client.GetStringAsync(url).Result;
                if (response != null)
                {
                    var jsonString = JsonConvert.DeserializeObject<dynamic>(response);

                    var searchResults = new List<Metadata>();
                    foreach (var result in jsonString.products)
                    {
                        string resultAsin = result.asin;
                        searchResults.Add(GetMetadata(resultAsin, settings));
                    }

                    return searchResults;
                }
                else
                {
                    return null;
                }
            }
        }

        private Metadata ConvertImportToMetadata(AudibleBook audibleBook)
        {
            var metadata = new Metadata();
            metadata.Title = audibleBook.title;
            metadata.Subtitle = audibleBook.subtitle;
            metadata.Authors = audibleBook.authors.Select(a => a.name).ToList();
            metadata.Narrators = audibleBook.narrators.Select(n => n.name).Where(n => n.ToLower() != "full cast").ToList();
            metadata.Series = new List<Metadata.MetadataSeries>();
            if (audibleBook.seriesPrimary != null)
            {
                metadata.Series.Add(new Metadata.MetadataSeries(audibleBook.seriesPrimary.name, audibleBook.seriesPrimary.position));
            }
            if (audibleBook.seriesSecondary != null)
            {
                metadata.Series.Add(new Metadata.MetadataSeries(audibleBook.seriesSecondary.name, audibleBook.seriesSecondary.position));
            }
            metadata.Description = audibleBook.summary;
            metadata.Publisher = audibleBook.publisherName;
            metadata.PublishDate = audibleBook.releaseDate;
            metadata.Genres = audibleBook.genres.Where(i => i.type == "genre").Select(i => i.name).ToList();
            metadata.Tags = audibleBook.genres.Where(i => i.type == "tag").Select(i => i.name).ToList();
            metadata.Language = audibleBook.language;
            metadata.Covers = new List<string>() { { audibleBook.image } };

            return metadata;
        }

        #region Import Model

        private class AudibleBook
        {
            public string asin { get; set; }
            public List<AudibleAuthor> authors { get; set; } = new List<AudibleAuthor>();
            public string? description { get; set; }
            public string? formatType { get; set; }
            public string? image { get; set; }
            public string? language { get; set; }
            public List<AudibleNarrator> narrators { get; set; } = new List<AudibleNarrator>();
            public string? publisherName { get; set; }
            public string? rating { get; set; }
            public DateTime releaseDate { get; set; }
            public int runtimeLengthMin { get; set; }
            public AudibleSeries? seriesPrimary { get; set; }
            public AudibleSeries? seriesSecondary { get; set; }
            public string? subtitle { get; set; }
            public string? summary { get; set; }
            public string? title { get; set; }
            public List<AudibleGenre> genres { get; set; } = new List<AudibleGenre>();
        }

        private class AudibleAuthor
        {
            public string asin { get; set; }
            public string name { get; set; }
        }

        private class AudibleNarrator
        {
            public string name { get; set; }
        }

        private class AudibleSeries
        {
            public string asin { get; set; }
            public string name { get; set; }
            public string position { get; set; }
        }

        private class AudibleGenre
        {
            public string asin { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        #endregion
    }
}
