﻿using Anthology.Data;
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
        IBookService _bookService;
        IPluginsService _pluginsService;
        ISettingsService _settingsService;
        IClassificationService _classificationService;
        ISeriesService _seriesService;

        public MetadataService(IBookService bookService, IPluginsService pluginsService, ISettingsService settingsService, IClassificationService classificationService, ISeriesService seriesService)
        {
            _bookService = bookService;
            _pluginsService = pluginsService;
            _settingsService = settingsService;
            _classificationService = classificationService;
            _seriesService = seriesService;
        }

        public Task<List<dynamic>> Search(Dictionary<string, string> searchQuery)
        {
            if (searchQuery["title"].Length > 4 && searchQuery["title"].Substring(0, 4) == "ANTH") searchQuery["isbn"] = searchQuery["title"];

            if (searchQuery["isbn"] != null && searchQuery["isbn"].Substring(0, 4) == "ANTH")
            {
                var book = _bookService.GetBookByISBN(searchQuery["isbn"]);
                if (book != null)
                {
                    var searchISBN = GetMetadata(book).Result;

                    var searchISBNList = new List<dynamic>();
                    searchISBNList.Add(searchISBN);
                    return Task.FromResult(searchISBNList);

                }
            }

            var books = _bookService.GetBooks(searchQuery["title"]).Select(b => (dynamic)GetApiMetadata(b)).ToList();
            return Task.FromResult(books);
        }
        public Task<dynamic> GetApiMetadata(Book book)
        {
            dynamic mergedMetadata = GetMetadata(book, false);
            mergedMetadata.ISBN = book.ISBN;
            foreach (var identifier in book.Identifiers)
            {
                mergedMetadata.Add(identifier.Key, identifier.Value);
            }

            return Task.FromResult(mergedMetadata);
        }

        public Task<Metadata> GetMetadata(Book book, bool forceRefresh = false)
        {
            if (book.DateFetchedMetadata == null || book.DateFetchedMetadata > DateTime.Now.AddDays(30) || forceRefresh)
                RefreshBookMetadata(book);

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

        public Task RefreshBookMetadata(Book book)
        {
            Dictionary<string, Metadata> dataToMerge = new Dictionary<string, Metadata>();

            var sourcePlugins = _pluginsService.GetPluginList().Where(p => p.Type == Plugin.PluginType.Metadata).ToList();
            foreach (var plugin in sourcePlugins)
            {
                var sourceIdentifier = book.Identifiers.FirstOrDefault(i => i.Key == plugin.Identifier);
                if (sourceIdentifier != null)
                {
                    var sourceInstance = Activator.CreateInstance(plugin.ClassType) as IMetadataSource;
                    var sourceSettings = _settingsService.GetSettings().PluginSettings.Where(s => s.PluginName == plugin.Name).SelectMany(s => s.Settings).ToDictionary(s => s.Key, s => s.Value);
                    dataToMerge.Add(sourceIdentifier.Key, sourceInstance.GetMetadata(sourceIdentifier.Value, sourceSettings));
                }
            }

            book.BookMetadata = MergeMetadata(dataToMerge);
            _bookService.SaveBook(book);
            return Task.CompletedTask;
        }

        private Metadata MergeMetadata(Dictionary<string, Metadata> sources)
        {
            return new Metadata()
            {
                Title = SelectMetadataString(sources, "Title"),
                Subtitle = SelectMetadataString(sources, "Subtitle"),
                Authors = SelectMetadataList<string>(sources, "Authors"),
                Narrators = SelectMetadataList<string>(sources, "Narrators"),
                Series = SelectMetadataList<MetadataSeries>(sources, "Series"),
                Description = SelectMetadataString(sources, "Description"),
                Publisher = SelectMetadataString(sources, "Publisher"),
                PublishDate = SelectMetadataDateTime(sources, "PublishDate"),
                Genres = SelectMetadataList<string>(sources, "Genres", true),
                Tags = SelectMetadataList<string>(sources, "Tags", true),
                Language = SelectMetadataString(sources, "Language"),
                IsExplicit = SelectMetadataBool(sources, "IsExplicit"),
                Covers = SelectMetadataList<string>(sources, "Covers")
            };
        }

        private string SelectMetadataString(Dictionary<string, Metadata> sources, string fieldName)
        {
            string selectedValue = null;

            if (sources.ContainsKey("Override") && sources["Override"][fieldName] != null && !string.IsNullOrWhiteSpace((string)sources["Override"][fieldName])) selectedValue = (string)sources["Override"][fieldName];
            if (sources.ContainsKey("Metadata") && sources["Metadata"][fieldName] != null && !string.IsNullOrWhiteSpace((string)sources["Metadata"][fieldName])) selectedValue = (string)sources["Metadata"][fieldName];

            foreach (var source in ((List<SourcePriority>)_settingsService.GetSettings().FieldPriorities[fieldName]).OrderBy(s => s.Priority))
            {
                var sourceIdentifier = _pluginsService.GetPluginList().First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null && !string.IsNullOrWhiteSpace((string)sources[sourceIdentifier][fieldName]))
                {
                    selectedValue = (string)sources[sourceIdentifier][fieldName];
                    break;
                }
            }

            return selectedValue;
        }

        private DateTime? SelectMetadataDateTime(Dictionary<string, Metadata> sources, string fieldName)
        {
            DateTime? selectedValue = null;

            if (sources.ContainsKey("Override") && sources["Override"] != null && sources["Override"][fieldName] != null) selectedValue = (DateTime)sources["Override"][fieldName];
            if (sources.ContainsKey("Metadata") && sources["Metadata"] != null && sources["Metadata"][fieldName] != null) selectedValue = (DateTime)sources["Metadata"][fieldName];

            foreach (var source in ((List<SourcePriority>)_settingsService.GetSettings().FieldPriorities[fieldName]).OrderBy(s => s.Priority))
            {
                var sourceIdentifier = _pluginsService.GetPluginList().First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null && sources[sourceIdentifier][fieldName] != null)
                {
                    selectedValue = (DateTime)sources[sourceIdentifier][fieldName];
                    break;
                }
            }

            return selectedValue;
        }

        private bool SelectMetadataBool(Dictionary<string, Metadata> sources, string fieldName)
        {
            bool selectedValue = false;

            if (sources.ContainsKey("Override") && sources["Override"] != null && (bool)sources["Override"][fieldName]) selectedValue = (bool)sources["Override"][fieldName];
            if (sources.ContainsKey("Metadata") && sources["Metadata"] != null && (bool)sources["Metadata"][fieldName]) selectedValue = (bool)sources["Metadata"][fieldName];

            foreach (var source in ((List<SourcePriority>)_settingsService.GetSettings().FieldPriorities[fieldName]).OrderBy(s => s.Priority))
            {
                var sourceIdentifier = _pluginsService.GetPluginList().First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null && (bool)sources[sourceIdentifier][fieldName])
                {
                    selectedValue = (bool)sources[sourceIdentifier][fieldName];
                    break;
                }
            }

            return selectedValue;
        }

        private List<T> SelectMetadataList<T>(Dictionary<string, Metadata> sources, string fieldName, bool mergeData = false)
        {
            List<T> selectedValue = new List<T>();

            if (sources.ContainsKey("Override") && sources["Override"] != null && (List<T>)sources["Override"][fieldName] != null && ((List<T>)sources["Override"][fieldName]).Count != 0)
            {
                selectedValue.AddRange((List<T>)sources["Override"][fieldName]);
                if (!mergeData) return selectedValue;
            }
            if (sources.ContainsKey("Metadata") && sources["Metadata"] != null && (List<T>)sources["Metadata"][fieldName] != null && ((List<T>)sources["Metadata"][fieldName]).Count != 0)
            {
                selectedValue.AddRange((List<T>)sources["Metadata"][fieldName]);
                if (!mergeData) return selectedValue;
            }

            foreach (var source in ((List<SourcePriority>)_settingsService.GetSettings().FieldPriorities[fieldName]).OrderBy(s => s.Priority))
            {
                var sourceIdentifier = _pluginsService.GetPluginList().First(s => s.Type == Plugin.PluginType.Metadata && s.Name == source.Name).Identifier;
                if (sources.ContainsKey(sourceIdentifier) && sources[sourceIdentifier] != null && (List<T>)sources[sourceIdentifier][fieldName] != null && ((List<T>)sources[sourceIdentifier][fieldName]).Count != 0)
                {
                    selectedValue.AddRange((List<T>)sources[sourceIdentifier][fieldName]);
                    if (!mergeData) break;
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
                IsExplicit = book.IsExplicit,
                Covers = covers
            };
        }
    }
}
