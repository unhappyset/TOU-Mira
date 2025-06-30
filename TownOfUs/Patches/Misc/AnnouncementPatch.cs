using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections.Immutable;
using Reactor.Utilities;
using System.Collections;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace TownOfUs.Patches.Misc;

// code credit https://github.com/Yumenopai/TownOfHost_Y
[HarmonyPatch]
public class TouMiraModNews
{
    // ReSharper disable UnassignedField.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public int Number { get; set; }
    public string Date { get; set; }
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public string ShortTitle { get; set; }

    public string Text { get; set; }
    // ReSharper restore UnassignedField.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    public Announcement ToAnnouncement()
    {
        return new()
        {
            Number = Number,
            Title = Title,
            SubTitle = SubTitle,
            ShortTitle = ShortTitle,
            Text = Text,
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Date = Date,
            Id = "TouMiraModNews"
        };
    }
    public static List<TouMiraModNews> FromJson(string json)
    {
        var result = JsonSerializer.Deserialize<List<TouMiraModNews>>(json);
        return result ?? new List<TouMiraModNews>();
    }
}
public static class ModNewsFetcher
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private static string TouMiraModNewsURL = "https://raw.githubusercontent.com/AU-Avengers/TOU-Mira/refs/heads/dev/TownOfUs/Resources/Announcements/modNews-";
#pragma warning restore S1075 // URIs should not be hardcoded

    static bool downloaded;

    public static IEnumerator FetchNews()
    {
        if (downloaded)
        {
            yield break;
        }
        downloaded = true;
        /* TouMiraModNewsURL += TranslationController.Instance.currentLanguage.languageID switch
        {
            SupportedLangs.German => "de_DE.json",
            SupportedLangs.Latam => "es_419.json",
            SupportedLangs.Spanish => "es_ES.json",
            SupportedLangs.Filipino => "fil_PH.json",
            SupportedLangs.French => "fr_FR.json",
            SupportedLangs.Italian => "it_IT.json",
            SupportedLangs.Japanese => "ja_JP.json",
            SupportedLangs.Korean => "ko_KR.json",
            SupportedLangs.Dutch => "nl_NL.json",
            SupportedLangs.Brazilian => "pt_BR.json",
            SupportedLangs.Russian => "ru_RU.json",
            SupportedLangs.SChinese => "zh_CN.json",
            SupportedLangs.TChinese => "zh_TW.json",
            _ => "en_US.json", //English and any other unsupported language
        }; */
        TouMiraModNewsURL += "en_US.json";
        var request = UnityWebRequest.Get(TouMiraModNewsURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            downloaded = false;
            Logger<TownOfUsPlugin>.Error($"Couldn't fetch mod news from github: {request.error}");
            LoadTouMiraModNewsFromResources();
            yield break;
        }

        try
        {
            List<TouMiraModNews> newsList = TouMiraModNews.FromJson(request.downloadHandler.text);
            ModNewsHistory.AllModNews = newsList.OrderByDescending(n => DateTime.Parse(n.Date, System.Globalization.CultureInfo.InvariantCulture)).ToImmutableList();
        }
        catch (Exception ex)
        {
            Logger<TownOfUsPlugin>.Error($"Couldn't fetch mod news from github, loading from resources instead: {ex.Message}");
            // Use local Mod news instead
            LoadTouMiraModNewsFromResources();
        }
    }

    private static void LoadTouMiraModNewsFromResources()
    {
        /* string filename = TranslationController.Instance.currentLanguage.languageID switch
        {
            SupportedLangs.German => "de_DE.json",
            SupportedLangs.Latam => "es_419.json",
            SupportedLangs.Spanish => "es_ES.json",
            SupportedLangs.Filipino => "fil_PH.json",
            SupportedLangs.French => "fr_FR.json",
            SupportedLangs.Italian => "it_IT.json",
            SupportedLangs.Japanese => "ja_JP.json",
            SupportedLangs.Korean => "ko_KR.json",
            SupportedLangs.Dutch => "nl_NL.json",
            SupportedLangs.Brazilian => "pt_BR.json",
            SupportedLangs.Russian => "ru_RU.json",
            SupportedLangs.SChinese => "zh_CN.json",
            SupportedLangs.TChinese => "zh_TW.json",
            _ => "en_US.json", //English and any other unsupported language
        }; */

        string filename = "en_US.json";

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using Stream resourceStream = assembly.GetManifestResourceStream("TownOfUs.Resources.Announcements.modNews-" + filename)
            ?? throw new InvalidOperationException($"Resource not found: TownOfUs.Resources.Announcements.modNews-{filename}");
        using StreamReader reader = new(resourceStream);
        List<TouMiraModNews> newsList = TouMiraModNews.FromJson(reader.ReadToEnd());
        ModNewsHistory.AllModNews = newsList.OrderByDescending(n => DateTime.Parse(n.Date, System.Globalization.CultureInfo.InvariantCulture)).ToImmutableList();
    }

    [HarmonyPatch]
    public static class ModNewsHistory
    {
        public static ImmutableList<TouMiraModNews> AllModNews = ImmutableList<TouMiraModNews>.Empty;

        [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
        [HarmonyPrefix]
        public static void SetModAnnouncements_Prefix(ref Il2CppReferenceArray<Announcement> aRange)
        {
            if (AllModNews.Count == 0)
            {
                Logger<TownOfUsPlugin>.Error($"No mod news were found.");
                return;
            }

            List<Announcement> finalAllNews = AllModNews.Select(n => n.ToAnnouncement()).ToList();
            finalAllNews.AddRange(aRange.Where(news => AllModNews.All(x => x.Number != news.Number)));
            finalAllNews.Sort((a1, a2) => DateTime.Compare(DateTime.Parse(a2.Date, System.Globalization.CultureInfo.InvariantCulture), DateTime.Parse(a1.Date, System.Globalization.CultureInfo.InvariantCulture)));

            aRange = new Il2CppReferenceArray<Announcement>(finalAllNews.Count);

            for (var i = 0; i < finalAllNews.Count; i++)
                aRange[i] = finalAllNews[i];

        }

        [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp)), HarmonyPostfix]
        public static void SetUpPanel_Postfix(AnnouncementPanel __instance, [HarmonyArgument(0)] Announcement announcement)
        {
            if (announcement.Number < 100000) return;
            var obj = new GameObject("ModLabel");
            //obj.layer = -1;
            obj.transform.SetParent(__instance.transform);
            obj.transform.localPosition = new Vector3(-0.8f, 0.13f, 0.5f);
            obj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = TouAssets.AuAvengersSprite.LoadAsset();
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }
}
