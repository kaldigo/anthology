using Anthology.Data;

namespace Anthology.Services
{
    public interface ISettingsService
    {
        Settings GetSettings();
        Task<Settings> GetSettingsAsync();
        void SaveSettings(Settings settings);
        void InitializeSettings();
    }
}