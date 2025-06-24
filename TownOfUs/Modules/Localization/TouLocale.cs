using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Localization;

public static class TouLocale
{
    public static string LocaleDirectory => Path.Combine(Application.persistentDataPath, "TownOfUs", "Locales");

    public static Dictionary<SupportedLangs, Dictionary<TouNames, string>> TouLocalization { get; } = [];

    public static string Get(TouNames name)
    {
        var currentLanguage = TranslationController.Instance.currentLanguage.languageID;
        if (!TouLocalization.TryGetValue(currentLanguage, out var translations) ||
            !translations.TryGetValue(name, out var translation))
        {
            return Default.GetValueOrDefault(name, "STRMISS_" + name);
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

    private static Dictionary<TouNames, string> Default { get;} = new()
    {
        { TouNames.Astral, "Astral" },
        { TouNames.Bomber, "Bomber" },
        { TouNames.BomberCanVent, "Bomber Can Vent" },
        { TouNames.Swooper, "Swooper" },
        { TouNames.PuppetMaster, "Puppet Master" },
        { TouNames.Mage, "Mage" },
        { TouNames.Sandword, "Sandword" },
        { TouNames.RCXD, "RCXD" },
        { TouNames.Glitch, "Glitch" },
        { TouNames.Morphling, "Morphling" },
        { TouNames.Poisoner, "Poisoner" },
        { TouNames.Sniper, "Sniper" },
        { TouNames.Veteran, "Veteran" },
        { TouNames.Sheriff, "Sheriff" },
        { TouNames.Transporter, "Transporter" },
        { TouNames.Trapper, "Trapper" },
        { TouNames.Medic, "Medic" },
        { TouNames.Mystic, "Mystic" },
        { TouNames.Cannibal, "Cannibal" },
        { TouNames.Amnesiac, "Amnesiac" },
        { TouNames.Arsonist, "Arsonist" },
        { TouNames.Jester, "Jester" },
        { TouNames.Vigilante, "Vigilante" },
        { TouNames.Politician, "Politician" },
        { TouNames.Mayor, "Mayor" },
        { TouNames.Swapper, "Swapper" },
        { TouNames.Engineer, "Engineer" },
        { TouNames.Timelord, "Timelord" }
    };
}