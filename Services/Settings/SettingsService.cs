using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Anthology.Data;

namespace Anthology.Services
{
    public class SettingsService : ISettingsService
    {
        DatabaseContext _context;
        IPluginsService _plugins;

        public SettingsService(DatabaseContext context, IPluginsService plugins)
        {
            _context = context;
            _plugins = plugins;
            InitializeSettings();
        }

        public Settings GetSettings()
        {
            return _context.Settings.First();
        }

        public void SaveSettings(Settings settings)
        {
            _context.SaveChanges();
        }

        public void InitializeSettings()
        {
            var settings = _context.Settings.FirstOrDefault();
            if (settings == null)
            {
                settings = new Settings();
                _context.Settings.Add(settings);
                _context.SaveChanges();
            }
            InitializeFieldSettings(settings.FieldPriorities);
            InitializePluginSettings(settings.PluginSettings);

        }

        private void InitializeFieldSettings(FieldPriorities fieldSettings)
        {
            CleanPriorities(fieldSettings.Title);
            CleanPriorities(fieldSettings.Subtitle);
            CleanPriorities(fieldSettings.Authors);
            CleanPriorities(fieldSettings.Narrators);
            CleanPriorities(fieldSettings.Series);
            CleanPriorities(fieldSettings.Description);
            CleanPriorities(fieldSettings.Publisher);
            CleanPriorities(fieldSettings.PublishDate);
            CleanPriorities(fieldSettings.Genres);
            CleanPriorities(fieldSettings.Tags);
            CleanPriorities(fieldSettings.Language);
            CleanPriorities(fieldSettings.IsExplicit);
            CleanPriorities(fieldSettings.Covers);
            _context.SaveChanges();
        }

        private void InitializePluginSettings(List<PluginSetting> pluginSettings)
        {
            var pluginNames = _plugins.GetPluginList().Select(p => p.Name).Distinct().ToList();

            var pluginsToAdd = pluginNames.Where(p => !pluginSettings.Select(s => s.PluginName).Contains(p)).Select(p => new PluginSetting() { PluginName = p }).ToList();
            var pluginsToRemove = pluginSettings.Where(s => !pluginNames.Contains(s.PluginName)).ToList();

            pluginSettings.AddRange(pluginsToAdd);
            foreach (var item in pluginsToRemove) pluginSettings.Remove(item);

            foreach (var settings in pluginSettings)
            {
                var pluginSettingKeys = _plugins.GetPluginList().Where(p => p.Name == settings.PluginName).SelectMany(p => p.SettingKeys).Distinct().ToList();

                var settingsToAdd = pluginSettingKeys.Where(p => !settings.Settings.Select(s => s.Key).Contains(p)).Select(p => new SettingKV() { Key = p, Value = "" }).ToList();
                var settingsToRemove = settings.Settings.Where(s => !pluginSettingKeys.Contains(s.Key)).ToList();

                settings.Settings.AddRange(settingsToAdd);
                foreach (var item in settingsToRemove) settings.Settings.Remove(item);
            }

            _context.SaveChanges();
        }

        private void CleanPriorities(List<SourcePriority> sourcePriorities)
        {
            var pluginNames = _plugins.GetPluginList().Where(p => p.Type == Plugin.PluginType.Metadata).Select(p => p.Name).ToList();

            var sourcesToAdd = pluginNames.Where(p => !sourcePriorities.Select(s => s.Name).Contains(p)).Select(s => new SourcePriority() { Name = s, Priority = 99 }).ToList();
            var sourcesToRemove = sourcePriorities.Where(s => !pluginNames.Contains(s.Name)).ToList();

            sourcePriorities.AddRange(sourcesToAdd);
            foreach (var item in sourcesToRemove) sourcePriorities.Remove(item);
        }
    }
}
