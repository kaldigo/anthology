using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Anthology.Plugins.Models;

namespace Anthology.Plugins
{
    public interface IMetadataSource
    {
        string Name { get; } // Unique name of the plugin
        string IdentifierKey { get; } // Unique key for plugin identification
        List<string> Settings { get; }

        // Actual methods to be implemented
        List<MetadataSearchResult> PerformSearch(Dictionary<string, string> settings, string title, string author = null);
        Metadata PerformGetMetadata(string identifier, Dictionary<string, string> settings);

        // Caching-enabled methods with default implementation
        private static readonly MemoryCache Cache = MemoryCache.Default;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(1);

        List<MetadataSearchResult> Search(Dictionary<string, string> settings, string title, string author = null)
        {
            string cacheKey = GenerateCacheKey("Search", Name, settings, title, author);
            if (CacheContainsValidItem<List<MetadataSearchResult>>(cacheKey))
            {
                return (List<MetadataSearchResult>)Cache.Get(cacheKey);
            }

            var results = PerformSearch(settings, title, author);
            if (results != null && results.Count > 0) // Cache only if there are results
            {
                Cache.Set(cacheKey, results, DateTimeOffset.Now.Add(CacheDuration));
            }
            return results;
        }

        Metadata GetMetadata(string identifier, Dictionary<string, string> settings)
        {
            string cacheKey = GenerateCacheKey("GetMetadata", Name, settings, identifier);
            if (CacheContainsValidItem<Metadata>(cacheKey))
            {
                return (Metadata)Cache.Get(cacheKey);
            }

            var metadata = PerformGetMetadata(identifier, settings);
            if (metadata != null) // Cache only if metadata is not null
            {
                Cache.Set(cacheKey, metadata, DateTimeOffset.Now.Add(CacheDuration));
            }
            return metadata;
        }

        private static bool CacheContainsValidItem<T>(string cacheKey)
        {
            if (!Cache.Contains(cacheKey))
            {
                return false;
            }

            var cachedItem = Cache.Get(cacheKey);
            if (cachedItem is T typedItem)
            {
                // Ensure it's not empty (check for null, empty lists, etc.)
                if (typedItem is ICollection<object> collection && collection.Count == 0)
                {
                    return false;
                }
                return true; // Valid item found
            }
            return false; // Cache contains an invalid or incompatible item
        }

        private static string GenerateCacheKey(string methodName, string pluginName, Dictionary<string, string> settings, params string[] additionalParams)
        {
            var settingsKey = string.Join("&", settings.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var additionalKey = string.Join("&", additionalParams);
            return $"{pluginName}|{methodName}|{settingsKey}|{additionalKey}";
        }
    }
}
