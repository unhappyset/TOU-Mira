using Reactor.Localization;

namespace TownOfUs.Modules.Localization;

public class TouLocalizationProvider : LocalizationProvider
{
    public override bool TryGetText(StringNames stringName, out string? result)
    {
        result = null;
        if (TouLocale.TouLocaleList.ContainsValue(stringName) && CurrentLanguage != null)
        {
            var touName = TouLocale.TouLocaleList.FirstOrDefault(x => x.Value == stringName).Key;
            
            if (TouLocale.TouLocalization.TryGetValue((SupportedLangs)CurrentLanguage, out var translations) &&
                translations.TryGetValue(touName, out var translation))
            {
                result = translation;
                return true;
            }
            
            if (TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
                     translationsEng.TryGetValue(touName, out var englishDefault))
            {
                result = englishDefault;
                return true;
            }

            result = "STRMISS_" + touName;
            return true;
        }
        return false;
    }
    public override void OnLanguageChanged(SupportedLangs newLanguage)
    {
        // Add checks for the options menu to change all names there, april fools button on the main menu, etc etc
    }
}