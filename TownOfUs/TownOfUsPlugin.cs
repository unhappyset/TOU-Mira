using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace TownOfUs;

/// <summary>
/// Plugin class for Town of Us.
/// </summary>
[BepInAutoPlugin("auavengers.tou.mira", "Town of Us Mira")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class TownOfUsPlugin : BasePlugin, IMiraPlugin
{
    /// <summary>
    /// Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture { get; } = new("en-US");

    /// <summary>
    /// Gets the Harmony instance for patching.
    /// </summary>
    public Harmony Harmony { get; } = new(Id);

    /// <inheritdoc/>
    public string OptionsTitleText => "Town of Us\nMira";

    /// <inheritdoc/>
    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public static ConfigEntry<bool> DeadSeeGhosts { get; set; }
    public static ConfigEntry<bool> ShowShieldHud { get; set; }
    public static ConfigEntry<bool> ShowSummaryMessage { get; set; }
    public static ConfigEntry<bool> ShowWelcomeMessage { get; set; }
    public static ConfigEntry<int> GameSummaryMode { get; set; }

    /// <summary>
    /// The Load method for the plugin.
    /// </summary>
    public override void Load()
    {
        Reactor.Utilities.ReactorCredits.Register<TownOfUsPlugin>(Reactor.Utilities.ReactorCredits.AlwaysShow);

        TouAssets.Initialize();

        IL2CPPChainloader.Instance.Finished += Modules.ModCompatibility.Initialize; // Initialise AFTER the mods are loaded to ensure maximum parity (no need for the soft dependency either then)

        var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(TownOfUsPlugin))!.Location) + "\\touhats.catalog";
        AddressablesLoader.RegisterCatalog(path);
        AddressablesLoader.RegisterHats("touhats");

        DeadSeeGhosts = Config.Bind("LocalSettings", "DeadSeeGhosts", true, "If you see other ghosts when dead");
        ShowShieldHud = Config.Bind("LocalSettings", "ShowShieldHud", true, "If you see shield modifiers with a description, turn this off if it gets in your way.");
        ShowSummaryMessage = Config.Bind("LocalSettings", "ShowSummaryMessage", true, "If you see the game summary message when you join the lobby again.");
        ShowWelcomeMessage = Config.Bind("LocalSettings", "ShowWelcomeMessage", true, "If you see the welcome message when you first join a game.");
        GameSummaryMode = Config.Bind("LocalSettings", "GameSummaryMode", 1, "How the Game Summary appears in the Win Screen. 0 is to the left, 1 is split, and 2 is hidden.");

        // do not touch this warning fixing it broke the function of the code
#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable CA1825 // Avoid zero-length array allocations
        ServerManager.DefaultRegions = new Il2CppReferenceArray<IRegionInfo>(new IRegionInfo[0]);
#pragma warning restore CA1825 // Avoid zero-length array allocations
#pragma warning restore IDE0300 // Simplify collection initialization

        Harmony.PatchAll();
    }
}
