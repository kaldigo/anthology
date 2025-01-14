using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
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
            if (Cache.Contains(cacheKey))
            {
                return (List<MetadataSearchResult>)Cache.Get(cacheKey);
            }

            var results = PerformSearch(settings, title, author);
            Cache.Set(cacheKey, results, DateTimeOffset.Now.Add(CacheDuration));
            return results;
        }

        Metadata GetMetadata(string identifier, Dictionary<string, string> settings)
        {
            string cacheKey = GenerateCacheKey("GetMetadata", Name, settings, identifier);
            if (Cache.Contains(cacheKey))
            {
                return (Metadata)Cache.Get(cacheKey);
            }

            var metadata = PerformGetMetadata(identifier, settings);
            Cache.Set(cacheKey, metadata, DateTimeOffset.Now.Add(CacheDuration));
            return metadata;
        }

        private static string GenerateCacheKey(string methodName, string pluginName, Dictionary<string, string> settings, params string[] additionalParams)
        {
            var settingsKey = string.Join("&", settings.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var additionalKey = string.Join("&", additionalParams);
            return $"{pluginName}|{methodName}|{settingsKey}|{additionalKey}";
        }
    }

}
