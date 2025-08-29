using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using BepInEx.Logging;
using Reactor.Localization;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Localization;

public static class TouLocale
{
    public static string LocaleDirectory => Path.Combine(Application.persistentDataPath, "TownOfUs", "Locales");

    public static Dictionary<SupportedLangs, string> LangList { get; } = new()
    {
        { SupportedLangs.English, "en_US.xml" },
        { SupportedLangs.German, "de_DE.xml" },
        { SupportedLangs.Latam, "es_419.xml" },
        { SupportedLangs.Spanish, "es_ES.xml" },
        { SupportedLangs.Filipino, "fil_PH.xml" },
        { SupportedLangs.French, "fr_FR.xml" },
        { SupportedLangs.Italian, "it_IT.xml" },
        { SupportedLangs.Japanese, "ja_JP.xml" },
        { SupportedLangs.Korean, "ko_KR.xml" },
        { SupportedLangs.Dutch, "nl_NL.xml" },
        { SupportedLangs.Brazilian, "pt_BR.xml" },
        { SupportedLangs.Russian, "ru_RU.xml" },
        { SupportedLangs.SChinese, "zh_CN.xml" },
        { SupportedLangs.TChinese, "zh_TW.xml" }
    };

    public static string BepinexLocaleDirectory =>
        Path.Combine(BepInEx.Paths.BepInExRootPath, "MiraLocales", "TownOfUs");

    public static Dictionary<string, StringNames> TouLocaleList { get; } = [];

    public static Dictionary<string, string> TmpTextList { get; } = new()
    {
        { "<nl>", "\n" },
        { "<and>", "&" },
    };
    // Language, Xml Name, then Value
    public static Dictionary<SupportedLangs, Dictionary<string, string>> TouLocalization { get; } = [];

    private static ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource("TouLocale");

    public static string Get(string name, string? defaultValue = null)
    {
        if (TranslationController.InstanceExists && TouLocaleList.TryGetValue(name, out var value))
        {
            return TranslationController.Instance.GetString(value) ?? defaultValue ?? "STRMISS3_" + name;
        }

        var currentLanguage =
            TranslationController.InstanceExists
                ? TranslationController.Instance.currentLanguage.languageID
                : SupportedLangs.English;

        if (!TouLocalization.TryGetValue(currentLanguage, out var translations) ||
            !translations.TryGetValue(name, out var translation))
        {
            return defaultValue ?? "STRMISS2_" + name;
        }

        return translation;
    }

    public static string GetParsed(string name, string? defaultValue = null, Dictionary<string, string>? parseList = null)
    {
        var text = string.Empty;
        if (TranslationController.InstanceExists && TouLocaleList.TryGetValue(name, out var value))
        {
            text = TranslationController.Instance.GetString(value) ?? defaultValue ?? "STRMISS3_" + name;
        }

        var currentLanguage =
            TranslationController.InstanceExists
                ? TranslationController.Instance.currentLanguage.languageID
                : SupportedLangs.English;

        if ((!TouLocalization.TryGetValue(currentLanguage, out var translations) ||
             !translations.ContainsValue(name)) && text == string.Empty)
        {
            text = defaultValue ?? "STRMISS2_" + name;
        }

        if (translations != null && translations.TryGetValue(name, out var translation2) && text == string.Empty)
        {
            text = translation2;
        }
        
        text = Regex.Replace(text, @"\%([^%]+)\%", @"<$1>");
        if (text.Contains("\\<"))
        {
            text = text.Replace("\\<", "<");
        }
        if (text.Contains("\\>"))
        {
            text = text.Replace("\\>", ">");
        }

        foreach (var tmpText in TmpTextList.Where(x => text.Contains(x.Key)))
        {
            text = text.Replace(tmpText.Key, tmpText.Value);
        }

        if (parseList != null)
        {
            foreach (var tmpText in parseList.Where(x => text.Contains(x.Key)))
            {
                text = text.Replace(tmpText.Key, tmpText.Value);
            }
        }
        
        return text;
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
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var locale in LangList)
        {
            using var resourceStream =
                assembly.GetManifestResourceStream("TownOfUs.Resources.Locale." + locale.Value);
            if (resourceStream == null)
            {
                continue;
            }
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

        var xmlTranslations = Directory.GetFiles(directory, "*.xml");
        foreach (var file in xmlTranslations)
        {
            var localeName = Path.GetFileNameWithoutExtension(file);
            if (!LangList.ContainsValue(localeName + ".xml"))
            {
                Logger.LogWarning($"Invalid locale iso name: {localeName}");
                continue;
            }

            var language = LangList.FirstOrDefault(x => x.Value == localeName + ".xml").Key;
            TouLocalization.TryAdd(language, []);
            ParseXmlFile(file, language);
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
            XmlNodeList? stringNodes = xmlDoc.SelectNodes("/resources/string");

            if (stringNodes != null)
            {
                foreach (XmlNode node in stringNodes)
                {
                    if (node.Attributes?["name"] != null)
                    {
                        string name = node.Attributes["name"]!.Value;
                        string value = node.InnerText;
                        
                        if (TouLocalization[language].ContainsKey(name))
                        {
                            var ogValuePair = TouLocalization[language].FirstOrDefault(x => x.Key == name);
                            TouLocalization[language].Remove(ogValuePair.Key);
                        }
                        
                        TouLocalization[language].TryAdd(name, value);
                        
                        if (language is SupportedLangs.English && !TouLocalization[language].ContainsKey(name))
                        {
                            var stringName = CustomStringName.CreateAndRegister(name);
                            TouLocaleList.TryAdd(name, stringName);
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