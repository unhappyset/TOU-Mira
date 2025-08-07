using Reactor.Localization;

namespace TownOfUs.Modules.Localization;

public class TouLocalizationProvider : LocalizationProvider
{
    public override bool TryGetText(StringNames stringName, out string? result)
    {
        var id = (int)stringName;
        if (TouLocale.TouLocaleList.ContainsValue(id) && CurrentLanguage != null)
        {
            var touName = TouLocale.TouLocaleList.FirstOrDefault(x => x.Value == id).Key;
            
            if (TouLocale.TouLocalization.TryGetValue((SupportedLangs)CurrentLanguage, out var translations) &&
                translations.TryGetValue(touName, out var translation))
            {
                result = translation;
                return true;
            }
            else if (TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
                     translationsEng.TryGetValue(touName, out var englishDefault))
            {
                result = englishDefault;
                return true;
            }

            result = "STRMISS_" + touName;
            return true;
        }
        
        result = null;
        return false;
    }
    public override void OnLanguageChanged(SupportedLangs newLanguage)
    {
        // Add checks for the options menu to change all names there, april fools button on the main menu, etc etc
    }
}