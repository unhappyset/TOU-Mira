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
            TouLocale.Logger.LogError($"Tou Name found for {touName}");
            
            if (TouLocale.TouLocalization.TryGetValue((SupportedLangs)CurrentLanguage, out var translations) &&
                translations.TryGetValue(touName, out var translation))
            {
                TouLocale.Logger.LogError($"Tou Name fetched for {touName}: {translation}");
                result = translation;
                return true;
            }
            
            if (TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
                     translationsEng.TryGetValue(touName, out var englishDefault))
            {
                TouLocale.Logger.LogError($"Tou Name fetched default for {touName}: {englishDefault}");
                result = englishDefault;
                return true;
            }

            result = "STRMISS_" + touName;
        }
        return false;
    }
    public bool TryGetTextFormatted(StringNames stringName, out string? result)
    {
        result = null;
        if (TouLocale.TouLocaleList.ContainsValue(stringName) && CurrentLanguage != null)
        {
            var touName = TouLocale.TouLocaleList.FirstOrDefault(x => x.Value == stringName).Key;
            TouLocale.Logger.LogError($"Tou Name found for {touName}");
            
            if (TouLocale.TouLocalization.TryGetValue((SupportedLangs)CurrentLanguage, out var translations) &&
                translations.TryGetValue(touName, out var translation))
            {
                TouLocale.Logger.LogError($"Tou Name fetched for {touName}: {translation}");
                result = translation;
                return true;
            }
            
            if (TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var translationsEng) &&
                translationsEng.TryGetValue(touName, out var englishDefault))
            {
                TouLocale.Logger.LogError($"Tou Name fetched default for {touName}: {englishDefault}");
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
        var langToUse = TouLocale.TouLocalization.ContainsKey(newLanguage) ? newLanguage : SupportedLangs.English;
        foreach (var keypair in TouLocale.TouLocalization[langToUse])
        {
            TranslationController.Instance.currentLanguage.AllStrings.TryAdd(keypair.Key, keypair.Value);
        }
        foreach (var keypair in TouLocale.TouLocalization[SupportedLangs.English])
        {
            TranslationController.Instance.fallbackLanguage.AllStrings.TryAdd(keypair.Key, keypair.Value);
        }
        // Add checks for the options menu to change all names there, april fools button on the main menu, etc etc
    }
}