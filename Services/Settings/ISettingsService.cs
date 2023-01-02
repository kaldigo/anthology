using Anthology.Data;

namespace Anthology.Services
{
    public interface ISettingsService
    {
        Settings GetSettings();
        void SaveSettings(Settings settings);
        void InitializeSettings();
    }
}