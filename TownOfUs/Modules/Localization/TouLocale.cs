using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Localization;

public static class TouLocale
{
    public static string LocaleDirectory => Path.Combine(Application.persistentDataPath, "TownOfUs", "Locales");

    public static Dictionary<SupportedLangs, Dictionary<TouNames, string>> TouLocalization { get; } = [];

    public static string Get(TouNames name, string? defaultValue = null)
    {
        var currentLanguage = TranslationController.Instance.currentLanguage.languageID;
        if (!TouLocalization.TryGetValue(currentLanguage, out var translations) ||
            !translations.TryGetValue(name, out var translation))
        {
            return defaultValue ?? "STRMISS_" + name;
        }

        return translation;
    }

    public static void Initialize()
    {
        var translations = Directory.GetFiles(LocaleDirectory, "*.txt");
        foreach (var language in Enum.GetValues<SupportedLangs>())
        {
            TouLocalization.TryAdd(language, []);
        }

        foreach (var file in translations)
        {
            var localeName = Path.GetFileNameWithoutExtension(file);
            if (!Enum.TryParse<SupportedLangs>(localeName, out var language))
            {
                Logger<TownOfUsPlugin>.Warning($"Invalid locale name: {localeName}");
                continue;
            }

            foreach (var translation in File.ReadAllLines(file))
            {
                var parts = translation.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0];
                    var value = parts[1];

                    if (!int.TryParse(key, out var touName))
                    {
                        Logger<TownOfUsPlugin>.Warning("Invalid key value in translation: " + translation);
                    }

                    TouLocalization[language].TryAdd((TouNames)touName, value);
                }
                else
                {
                    Logger<TownOfUsPlugin>.Warning("Invalid translation format: " + translation);
                }
            }
        }
    }
}