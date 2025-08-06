using System.Reflection;
using System.Text.Json;
using System.Xml;
using BepInEx.Logging;
using Reactor.Localization;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Localization;

public static class TouLocale
{
    // C# classes for deserialization
    public class RootObject
    {
        public List<Dictionary<string, string>> RawTouNames { get; set; }
    }

    public static string LocaleDirectory => Path.Combine(Application.persistentDataPath, "TownOfUs", "Locales");

    public static string BepinexLocaleDirectory =>
        Path.Combine(BepInEx.Paths.BepInExRootPath, "MiraLocales", "TownOfUs");
    public static Dictionary<string, int> TouLocaleList { get; } = [];
    public static Dictionary<SupportedLangs, Dictionary<string, string>> TouLocalization { get; } = [];
    public static int VanillaEnumAmounts;

    private static ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource("TouLocale");

    public static string Get(string name, string? defaultValue = null)
    {
        if (TranslationController.InstanceExists && TouLocaleList.TryGetValue(name, out var value))
        {
            var id = value + VanillaEnumAmounts;
            return TranslationController.Instance.GetString((StringNames)id) ?? defaultValue ?? "STRMISS_" + name;
        }

        var currentLanguage =
            TranslationController.InstanceExists
                ? TranslationController.Instance.currentLanguage.languageID
                : SupportedLangs.English;

        if (!TouLocalization.TryGetValue(currentLanguage, out var translations) ||
            !translations.TryGetValue(name, out var translation))
        {
            return defaultValue ?? "STRMISS_" + name;
        }

        return translation;
    }

    public static void Initialize()
    {
        SearchInteralLocale();
        SearchDirectory(BepInEx.Paths.PluginPath);
        SearchDirectory(BepInEx.Paths.BepInExRootPath);
        SearchDirectory(BepinexLocaleDirectory);
        SearchDirectory(BepInEx.Paths.GameRootPath);
        SearchDirectory(LocaleDirectory);
        LocalizationManager.Register(new TouLocalizationProvider());
    }

    public static void SearchInteralLocale()
    {
        List<KeyValuePair<SupportedLangs, string>> list =
        [
            new(SupportedLangs.English, "en_US.xml"),
            //new(SupportedLangs.German, "de_DE.xml"),
            //new(SupportedLangs.Latam, "es_419.xml"),
            new(SupportedLangs.Spanish, "es_ES.xml"),
            //new(SupportedLangs.Filipino, "fil_PH.xml"),
            //new(SupportedLangs.French, "fr_FR.xml"),
            //new(SupportedLangs.Italian, "it_IT.xml"),
            //new(SupportedLangs.Japanese, "ja_JP.xml"),
            //new(SupportedLangs.Korean, "ko_KR.xml"),
            //new(SupportedLangs.Dutch, "nl_NL.xml"),
            //new(SupportedLangs.Brazilian, "pt_BR.xml"),
            //new(SupportedLangs.Russian, "ru_RU.xml"),
            //new(SupportedLangs.SChinese, "zh_CN.xml"),
            //new(SupportedLangs.TChinese, "zh_TW.xml")
        ];

        var assembly = Assembly.GetExecutingAssembly();
        foreach (var locale in list)
        {
            using var resourceStream =
                assembly.GetManifestResourceStream("TownOfUs.Resources.Locale." + locale.Value)
                ?? throw new InvalidOperationException(
                    $"Resource not found: TownOfUs.Resources.Locale.{locale.Value}");
            using StreamReader reader = new(resourceStream);
            string xmlContent = reader.ReadToEnd();
        
            TouLocalization.TryAdd(locale.Key, []);
            ParseXmlFile(xmlContent, locale.Key);
        }
    }

    public static void SearchDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Logger.LogWarning($"Directory does not exist: {directory}");
            return;
        }

        var translations = Directory.GetFiles(directory, "*.txt");
        foreach (var file in translations)
        {
            var localeName = Path.GetFileNameWithoutExtension(file);
            if (!Enum.TryParse<SupportedLangs>(localeName, out var language))
            {
                Logger.LogWarning($"Invalid locale name: {localeName}");
                continue;
            }

            TouLocalization.TryAdd(language, []);
            ParseFile(file, language);
        }
    }

    public static void ParseFile(string file, SupportedLangs language)
    {
        foreach (var translation in File.ReadAllLines(file))
        {
            var parts = translation.Split('=');
            if (parts.Length >= 2)
            {
                var key = parts[0];
                var value = string.Join("=", parts.Skip(1));

                if (TouLocalization[language].ContainsKey(key))
                {
                    var ogValuePair = TouLocalization[language].FirstOrDefault(x => x.Key == key);
                    TouLocalization[language].Remove(ogValuePair.Key);
                }

                TouLocalization[language].TryAdd(key, value);
            }
            else
            {
                Logger.LogWarning("Invalid translation format: " + translation);
            }
        }
    }
    
    public static void ParseXmlFile(string xmlContent, SupportedLangs language)
    {
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            xmlDoc.LoadXml(xmlContent);
            XmlNodeList? stringNodes = xmlDoc.SelectNodes("/resources/TouNames/string");

            if (stringNodes != null)
            {
                foreach (XmlNode node in stringNodes)
                {
                    if (node.Attributes?["name"] != null)
                    {
                        string name = node.Attributes["name"]!.Value;
                        string value = node.InnerText;
                        
                        TouLocalization[language].TryAdd(name, value);
                        if (language is SupportedLangs.English)
                        {
                            var stringName = CustomStringName.CreateAndRegister(name);
                            if (VanillaEnumAmounts == 0)
                            {
                                VanillaEnumAmounts = (int)stringName;
                            }
                            TouLocaleList.TryAdd(name, (int)stringName);
                        }
                    }
                }
            }
        }
        catch (XmlException ex)
        {
            Logger.LogError($"XML parsing error: {ex.Message}");
        }
    }
}