using Anthology.Data;
using Anthology.Plugins;
using Anthology.Plugins.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Anthology.Plugins.Models.Metadata;

namespace Anthology.Services
{
    public class MetadataService : IMetadataService
    {
        IPluginsService _pluginsService;
        ISettingsService _settingsService;
        IClassificationService _classificationService;
        ISeriesService _seriesService;
        IPersonService _personService;
        private bool _isPendingRefresh;

        public MetadataService(IPluginsService pluginsService, ISettingsService settingsService, IClassificationService classificationService, ISeriesService seriesService, IPersonService personService)
        {
            _pluginsService = pluginsService;
            _settingsService = settingsService;
            _classificationService = classificationService;
            _seriesService = seriesService;
            _personService = personService;
            _isPendingRefresh = false;
        }

        public bool IsPendingRefresh()
        {
            return this._isPendingRefresh;
        }
        public Task<ApiMetadata> GetApiMetadata(Book book, string host)
        {
            ApiMetadata mergedMetadata = new ApiMetadata(GetMetadata(book, false).Result);
            mergedMetadata.ISBN = book.ISBN;
            foreach (var identifier in book.Identifiers.Where(i => i.Exists && !string.IsNullOrWhiteSpace(i.Value)))
            {
                mergedMetadata.Identifiers.Add(identifier.Key.ToLower(), identifier.Value);
            }

            mergedMetadata.Covers = mergedMetadata.Covers.Select(c => host + c).ToList();

            return Task.FromResult(mergedMetadata);
        }

        public async Task<List<MetadataSearchResult>> SearchMetadata(Plugin plugin, string title, string author)
        {
            var pluginInstance = Activator.CreateInstance(plugin.ClassType) as IMetadataSource;
            var pluginSettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);

            var results = pluginInstance.Search(pluginSettings, title, author);

            return results;
        }

        public Task<Metadata> GetMetadata(Book book, bool forceRefresh = false)
        {
            if (book.DateFetchedMetadata == null || book.DateFetchedMetadata > DateTime.Now.AddDays(30) || forceRefresh)
            {
                RefreshBookMetadata(book);
                this._isPendingRefresh = true;
            }

            Dictionary<string, Metadata> dataToMerge = new Dictionary<string, Metadata>();
            dataToMerge.Add("Override", ConvertBookToMetadata(book));
            dataToMerge.Add("Metadata", book.BookMetadata);

            Metadata mergedMetadata = MergeMetadata(dataToMerge);

            var classifications = mergedMetadata.Genres.Select(c => new Classification() { Name = c, Type = Classification.ClassificationType.Genre }).ToList();
            classifications.AddRange(mergedMetadata.Tags.Select(c => new Classification() { Name = c, Type = Classification.ClassificationType.Tag }).ToList());

            foreach (var series in mergedMetadata.Series)
            {
                var dbSeries = _seriesService.GetSeries(series.Name);
                if(dbSeries != null)
                {
                    classifications.AddRange(dbSeries.Classifications);
                    classifications.AddRange(dbSeries.BookClassifications);
                }
            }

            var cleanedClassifications = _classificationService.CleanClassification(classifications);

            mergedMetadata.Genres = cleanedClassifications.Where(c => c.Type == Classification.ClassificationType.Genre).Select(c => c.Name).ToList();
            mergedMetadata.Tags = cleanedClassifications.Where(c => c.Type == Classification.ClassificationType.Tag).Select(c => c.Name).ToList();

            return Task.FromResult(mergedMetadata);
        }

        public Task RefreshMetadataCache()
        {
            _classificationService.RefreshMetadataClassifications();
            _seriesService.RefreshMetadataSeries();
            _personService.RefreshMetadataPeople();
            this._isPendingRefresh = false;
            return Task.CompletedTask;
        }

        public Task RefreshBookMetadata(Book book)
        {
            Dictionary<string, Metadata> dataToMerge = new Dictionary<string, Metadata>();

            var sourcePlugins = _pluginsService.GetPluginList().Where(p => p.Type == Plugin.PluginType.Metadata).ToList();
            foreach (var plugin in sourcePlugins)
            {
                var sourceIdentifier = book.Identifiers.FirstOrDefault(i => i.Key == plugin.Identifier);
                if (sourceIdentifier != null && !string.IsNullOrWhiteSpace(sourceIdentifier.Value) && sourceIdentifier.Exists)
                {
                    var sourceInstance = Activator.CreateInstance(plugin.ClassType) as IMetadataSource;
                    var sourceSettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);
                    dataToMerge.Add(sourceIdentifier.Key, sourceInstance.GetMetadata(sourceIdentifier.Value, sourceSettings));
                }
            }

            book.BookMetadata = MergeMetadata(dataToMerge);
            return Task.CompletedTask;
        }

        private Metadata MergeMetadata(Dictionary<string, Metadata> sources)
        {
            var fieldPriorities = _settingsService.GetSettings().FieldPriorities;
            var pluginList = _pluginsService.GetPluginList();
            return new Metadata()
            {
                Title = SelectMetadataString(sources, fieldPriorities, pluginList, "Title"),
                Subtitle = SelectMetadataString(sources, fieldPriorities, pluginList, "Subtitle"),
                Authors = SelectMetadataList<string>(sources, fieldPriorities, pluginList, "Authors"),
                Narrators = SelectMetadataList<string>(sources, fieldPriorities, pluginList, "Narrators"),
                Series = SelectMetadataList<MetadataSeries>(sources, fieldPriorities, pluginList, "Series"),
                Description = SelectMetadataString(sources, fieldPriorities, pluginList, "Description"),
                Publisher = SelectMetadataString(sources, fieldPriorities, pluginList, "Publisher"),
                PublishDate = SelectMetadataDateTime(sources, fieldPriorities, pluginList, "PublishDate"),
                Genres = SelectMetadataList<string>(sources, fieldPriorities, pluginList, "Genres", fieldPriorities.MergeGenres),
                Tags = SelectMetadataList<string>(sources, fieldPriorities, pluginList, "Tags", fieldPriorities.MergeTags),
                Language = SelectMetadataString(sources, fieldPriorities, pluginList, "Language"),
                IsExplicit = SelectMetadataBool(sources, fieldPriorities, pluginList, "IsExplicit"),
                Covers = SelectMetadataList<string>(sources, fieldPriorities, pluginList, "Covers", fieldPriorities.MergeCovers)
            };
        }

        private string SelectMetadataString(Dictionary<string, Metadata> sources, FieldPriorities fieldPriorities, List<Plugin> pluginList, string fieldName)
        {
            string selectedValue = null;

            if (sources.ContainsKey("Override") && sources["Override"][fieldName] != null && !string.IsNullOrWhiteSpace((string)sources["Override"][fieldName])) selectedValue = (string)sources["Override"][fieldName];
            else if (sources.ContainsKey("Metadata") && sources["Metadata"][fieldName] != null && !string.IsNullOrWhiteSpace((string)sources["Metadata"][fieldName])) selectedValue = (string)sources["Metadata"][fieldName];
            else
            {
                foreach (var source in ((List<SourcePriority>)fieldPriorities[fieldName]).OrderBy(s => s.Priority))
                {
                    var sourceIdentifier = pluginList
                        .First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                    if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null &&
                        !string.IsNullOrWhiteSpace((string)sources[sourceIdentifier][fieldName]))
                    {
                        selectedValue = (string)sources[sourceIdentifier][fieldName];
                        break;
                    }
                }
            }

            return selectedValue;
        }

        private DateTime? SelectMetadataDateTime(Dictionary<string, Metadata> sources, FieldPriorities fieldPriorities, List<Plugin> pluginList, string fieldName)
        {
            DateTime? selectedValue = null;

            if (sources.ContainsKey("Override") && sources["Override"] != null && sources["Override"][fieldName] != null) selectedValue = (DateTime)sources["Override"][fieldName];
            else if (sources.ContainsKey("Metadata") && sources["Metadata"] != null && sources["Metadata"][fieldName] != null) selectedValue = (DateTime)sources["Metadata"][fieldName];
            else
            {
                foreach (var source in ((List<SourcePriority>)fieldPriorities[fieldName]).OrderBy(s => s.Priority))
                {
                    var sourceIdentifier = pluginList
                        .First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                    if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null &&
                        sources[sourceIdentifier][fieldName] != null)
                    {
                        selectedValue = (DateTime)sources[sourceIdentifier][fieldName];
                        break;
                    }
                }
            }

            return selectedValue;
        }

        private bool SelectMetadataBool(Dictionary<string, Metadata> sources, FieldPriorities fieldPriorities, List<Plugin> pluginList, string fieldName)
        {
            bool selectedValue = false;

            if (sources.ContainsKey("Override") && sources["Override"] != null && (bool)sources["Override"][fieldName]) selectedValue = (bool)sources["Override"][fieldName];
            else if (sources.ContainsKey("Metadata") && sources["Metadata"] != null && (bool)sources["Metadata"][fieldName]) selectedValue = (bool)sources["Metadata"][fieldName];
            else
            {
                foreach (var source in ((List<SourcePriority>)fieldPriorities[fieldName]).OrderBy(s => s.Priority))
                {
                    var sourceIdentifier = pluginList
                        .First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                    if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null &&
                        (bool)sources[sourceIdentifier][fieldName])
                    {
                        selectedValue = (bool)sources[sourceIdentifier][fieldName];
                        break;
                    }
                }
            }

            return selectedValue;
        }

        private List<T> SelectMetadataList<T>(Dictionary<string, Metadata> sources, FieldPriorities fieldPriorities, List<Plugin> pluginList, string fieldName, bool mergeData = false)
        {
            List<T> selectedValue = new List<T>();

            if (sources.ContainsKey("Override") && sources["Override"] != null && (List<T>)sources["Override"][fieldName] != null && ((List<T>)sources["Override"][fieldName]).Count != 0)
            {
                selectedValue.AddRange((List<T>)sources["Override"][fieldName]);
                if (!mergeData) return selectedValue;
            }
            else if (sources.ContainsKey("Metadata") && sources["Metadata"] != null && (List<T>)sources["Metadata"][fieldName] != null && ((List<T>)sources["Metadata"][fieldName]).Count != 0)
            {
                selectedValue.AddRange((List<T>)sources["Metadata"][fieldName]);
                if (!mergeData) return selectedValue;
            }
            else
            {
                foreach (var source in ((List<SourcePriority>)fieldPriorities[fieldName]).OrderBy(s => s.Priority))
                {
                    var sourceIdentifier = pluginList
                        .First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                    if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null &&
                        (List<T>)sources[sourceIdentifier][fieldName] != null &&
                        ((List<T>)sources[sourceIdentifier][fieldName]).Count != 0)
                    {
                        selectedValue.AddRange((List<T>)sources[sourceIdentifier][fieldName]);
                        if (!mergeData) break;
                    }
                }
            }

            return selectedValue.Distinct().ToList();
        }

        private Metadata ConvertBookToMetadata(Book book)
        {
            var covers = new List<string>();
            covers.AddRange(book.BookCovers.Select(i => i.GetUrl()));
            covers.AddRange(book.AudiobookCovers.Select(i => i.GetUrl()));
            return new Metadata()
            {
                Title = book.Title,
                Subtitle = book.Subtitle,
                Authors = book.Authors.Select(a => a.Name).ToList(),
                Narrators = book.Narrators.Select(n => n.Name).ToList(),
                Series = book.Series.Select(s => new Metadata.MetadataSeries(s.Series.Name, s.Sequence)).ToList(),
                Description = book.Description,
                Publisher = book.Publisher,
                PublishDate = book.PublishDate,
                Genres = book.Classifications.Where(c => c.Type == Classification.ClassificationType.Genre).Select(c => c.Name).ToList(),
                Tags = book.Classifications.Where(c => c.Type == Classification.ClassificationType.Tag).Select(c => c.Name).ToList(),
                Language = book.Language,
                IsExplicit = book.IsExplicit.HasValue ? book.IsExplicit.Value : false,
                Covers = covers
            };
        }

        public class ApiMetadata : Metadata
        {
            public string? ISBN { get; set; }
            public Dictionary<string,string> Identifiers { get; set; } = new Dictionary<string, string>();

            public ApiMetadata(Metadata metadata)
            {
                Title = metadata.Title;
                Subtitle = metadata.Subtitle;
                Authors = metadata.Authors;
                Narrators = metadata.Narrators;
                Series = metadata.Series;
                Description = metadata.Description;
                Publisher = metadata.Publisher;
                PublishDate = metadata.PublishDate;
                Genres = metadata.Genres;
                Tags = metadata.Tags;
                Language = metadata.Language;
                IsExplicit = metadata.IsExplicit;
                Covers = metadata.Covers;
            }    

        }
    }
}
