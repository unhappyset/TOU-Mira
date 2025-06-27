using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Utilities;

public static class MiscUtils
{
    public static int KillersAliveCount => Helpers.GetAlivePlayers().Count(x => x.IsImpostor() || x.Is(RoleAlignment.NeutralKilling) || (x.Data.Role is InquisitorRole inquis && OptionGroupSingleton<InquisitorOptions>.Instance.StallGame && inquis.CanVanquish) || (x.Data.Role is ITouCrewRole { IsPowerCrew: true } && !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
        OptionGroupSingleton<GeneralOptions>.Instance.CrewKillersContinue));

    public static int RealKillersAliveCount => Helpers.GetAlivePlayers().Count(x => x.IsImpostor() || x.Is(RoleAlignment.NeutralKilling) || (x.Data.Role is InquisitorRole inquis && OptionGroupSingleton<InquisitorOptions>.Instance.StallGame && inquis.CanVanquish));

    public static int NKillersAliveCount => Helpers.GetAlivePlayers().Count(x => x.Is(RoleAlignment.NeutralKilling) || (x.Data.Role is InquisitorRole inquis && OptionGroupSingleton<InquisitorOptions>.Instance.StallGame && inquis.CanVanquish));

    public static int NonImpKillersAliveCount => Helpers.GetAlivePlayers().Count(x => x.Is(RoleAlignment.NeutralKilling) || (x.Data.Role is InquisitorRole inquis && OptionGroupSingleton<InquisitorOptions>.Instance.StallGame && inquis.CanVanquish) || (x.Data.Role is ITouCrewRole { IsPowerCrew: true } && !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
        OptionGroupSingleton<GeneralOptions>.Instance.CrewKillersContinue));

    public static int ImpAliveCount => Helpers.GetAlivePlayers().Count(x => x.IsImpostor());

    public static int CrewKillersAliveCount => Helpers.GetAlivePlayers().Count(x => x.Data.Role is ITouCrewRole { IsPowerCrew: true } && !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
        OptionGroupSingleton<GeneralOptions>.Instance.CrewKillersContinue);

    public static IEnumerable<BaseModifier> AllModifiers => MiraPluginManager.GetPluginByGuid(TownOfUsPlugin.Id)!.Modifiers;

    public static IEnumerable<RoleBehaviour> AllRoles => MiraPluginManager.GetPluginByGuid(TownOfUsPlugin.Id)!.Roles.Values;
    // public static IEnumerable<RoleBehaviour> AllRoles => RoleManager.Instance.AllRoles;

    public static ReadOnlyCollection<IModdedOption>? GetModdedOptionsForRole(Type classType)
    {
        var plugin = MiraPluginManager.GetPluginByGuid(TownOfUsPlugin.Id);
        var optionGroup = plugin!.OptionGroups.FirstOrDefault(g => classType.IsAssignableFrom(g.OptionableType));

        return optionGroup?.Children;
    }

    public static string AppendOptionsText(Type classType)
    {
        var options = GetModdedOptionsForRole(classType);
        if (options == null) return string.Empty;

        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture, $"\n<size=50%> \n</size><b>{TownOfUsColors.Vigilante.ToTextColor()}Options</color></b>");

        foreach (var option in options)
        {
            switch (option)
            {
                case ModdedToggleOption toggleOption:
                    if (!toggleOption.Visible()) continue;
                    builder.AppendLine(option.Title + ": " + toggleOption.Value);
                    break;
                case ModdedEnumOption enumOption:
                    if (!enumOption.Visible()) continue;
                    builder.AppendLine(enumOption.Title + ": " + enumOption.Values[enumOption.Value]);
                    break;
                case ModdedNumberOption numberOption:
                    if (!numberOption.Visible()) continue;
                    var optionStr = numberOption.Data.GetValueString(numberOption.Value);
                    if (optionStr.Contains(".000")) optionStr = optionStr.Replace(".000", "");
                    else if (optionStr.Contains(".00")) optionStr = optionStr.Replace(".00", "");
                    else if (optionStr.Contains(".0")) optionStr = optionStr.Replace(".0", "");

                    if (numberOption.ZeroInfinity && numberOption.Value == 0) builder.AppendLine(numberOption.Title + ": ∞");
                    else builder.AppendLine(numberOption.Title + ": " + optionStr);
                    break;
            }
        }

        return builder.ToString();
    }

    public static IEnumerable<RoleBehaviour> GetRegisteredRoles(RoleAlignment alignment)
    {
        var roles = AllRoles.Where(x => x is ITownOfUsRole role && role.RoleAlignment == alignment);
        var registeredRoles = roles.ToList();

        switch (alignment)
        {
            case RoleAlignment.CrewmateInvestigative:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Tracker));
                break;
            case RoleAlignment.CrewmateSupport:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Crewmate));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Scientist));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Noisemaker));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Engineer));
                break;
            case RoleAlignment.ImpostorSupport:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Impostor));
                break;
            case RoleAlignment.ImpostorConcealing:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Shapeshifter));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Phantom));
                break;
        }

        return registeredRoles;
    }

    public static IEnumerable<RoleBehaviour> GetRegisteredRoles(ModdedRoleTeams team)
    {
        var roles = AllRoles.Where(x => x is ITownOfUsRole role && role.Team == team);
        var registeredRoles = roles.ToList();

        switch (team)
        {
            case ModdedRoleTeams.Crewmate:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Crewmate));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Scientist));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Noisemaker));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Engineer));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Tracker));
                break;
            case ModdedRoleTeams.Impostor:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Impostor));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Shapeshifter));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Phantom));
                break;
        }

        return registeredRoles;
    }

    public static IEnumerable<RoleBehaviour> GetRegisteredGhostRoles()
    {
        var baseGhostRoles = RoleManager.Instance.AllRoles.Where(x => x.IsDead && AllRoles.All(y => y.Role != x.Role));
        var ghostRoles = AllRoles.Where(x => x.IsDead).Union(baseGhostRoles);

        return ghostRoles;
    }

    public static RoleBehaviour? GetRegisteredRole(RoleTypes roleType)
    {
        // we want to prioritise the custom roles because the role has the right RoleColour/TeamColor
        var role = AllRoles.FirstOrDefault(x => x.Role == roleType) ?? RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == roleType);

        return role;
    }

    public static T? GetRole<T>() where T : RoleBehaviour => PlayerControl.AllPlayerControls.ToArray().ToList().Find(x => x.Data.Role is T)?.Data?.Role as T;

    public static IEnumerable<RoleBehaviour> GetRoles(RoleAlignment alignment) => CustomRoleUtils.GetActiveRoles().Where(x => x is ITownOfUsRole role && role.RoleAlignment == alignment);

    public static PlayerControl? GetPlayerWithModifier<T>() where T : BaseModifier => ModifierUtils.GetPlayersWithModifier<T>().FirstOrDefault();

    public static Color GetRoleColour(string name)
    {
        var pInfo = typeof(TownOfUsColors).GetProperty(name, BindingFlags.Public | BindingFlags.Static);

        if (pInfo == null) return TownOfUsColors.Impostor;
        var colour = (Color)pInfo.GetValue(null)!;

        return colour;
    }

    public static string RoleNameLookup(RoleTypes roleType)
    {
        var role = RoleManager.Instance.GetRole(roleType);
        return role?.NiceName ?? (roleType == RoleTypes.Crewmate ? "Crewmate" : "Impostor");
    }

    public static IEnumerable<RoleBehaviour> GetPotentialRoles()
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;
        var assignmentData = RoleManager.Instance.AllRoles.Select(role => new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role), roleOptions.GetChancePerGame(role.Role))).ToList();

        var roleList = assignmentData.Where(x => x is { Chance: > 0, Role: ICustomRole }).Select(x => x.Role);

        var crewmateRole = RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Crewmate);
        roleList = roleList.AddItem(crewmateRole!);
        //Logger<TownOfUsPlugin>.Error($"GetPotentialRoles - crewmateRole: '{crewmateRole?.NiceName}'");

        var impostorRole = RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Impostor);
        roleList = roleList.AddItem(impostorRole!);
        //Logger<TownOfUsPlugin>.Error($"GetPotentialRoles - impostorRole: '{impostorRole?.NiceName}'");

        //roleList.Do(x => Logger<TownOfUsPlugin>.Error($"GetPotentialRoles - role: '{x.NiceName}'"));

        return roleList;
    }
    public static void AddFakeChat(NetworkedPlayerInfo basePlayer, string nameText, string message, bool showHeadsup = false, bool altColors = false, bool onLeft = true)
    {
        var chat = HudManager.Instance.Chat;

        var pooledBubble = chat.GetPooledBubble();

        pooledBubble.transform.SetParent(chat.scroller.Inner);
        pooledBubble.transform.localScale = Vector3.one;
        if (onLeft) pooledBubble.SetLeft();
        else pooledBubble.SetRight();
        pooledBubble.SetCosmetics(basePlayer);
        pooledBubble.NameText.text = nameText;
        pooledBubble.NameText.color = Color.white;
        pooledBubble.NameText.ForceMeshUpdate(true, true);
        pooledBubble.votedMark.enabled = false;
        pooledBubble.Xmark.enabled = false;
        pooledBubble.TextArea.text = message;
        pooledBubble.TextArea.ForceMeshUpdate(true, true);
        pooledBubble.Background.size = new(5.52f, 0.2f + pooledBubble.NameText.GetNotDumbRenderedHeight() + pooledBubble.TextArea.GetNotDumbRenderedHeight());
        pooledBubble.MaskArea.size = pooledBubble.Background.size - new Vector2(0, 0.03f);
        if (altColors)
        {
            pooledBubble.Background.color = Color.black;
            pooledBubble.TextArea.color = Color.white;
        }

        pooledBubble.AlignChildren();
        var pos = pooledBubble.NameText.transform.localPosition;
        pooledBubble.NameText.transform.localPosition = pos;
        chat.AlignAllBubbles();
        if (chat is { IsOpenOrOpening: false, notificationRoutine: null })
        {
            chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
        }
        if (showHeadsup && !chat.IsOpenOrOpening)
        {
            SoundManager.Instance.PlaySound(chat.messageSound, false).pitch = 0.5f + PlayerControl.LocalPlayer.PlayerId / 15f;
            chat.chatNotification.SetUp(PlayerControl.LocalPlayer, message);
        }
    }
    public static void AddTeamChat(NetworkedPlayerInfo basePlayer, string nameText, string message, bool showHeadsup = false, bool onLeft = true)
    {
        var chat = HudManager.Instance.Chat;

        var pooledBubble = chat.GetPooledBubble();

        pooledBubble.transform.SetParent(chat.scroller.Inner);
        pooledBubble.transform.localScale = Vector3.one;
        if (onLeft) pooledBubble.SetLeft();
        else pooledBubble.SetRight();
        pooledBubble.SetCosmetics(basePlayer);
        pooledBubble.NameText.text = nameText;
        pooledBubble.NameText.color = Color.white;
        pooledBubble.NameText.ForceMeshUpdate(true, true);
        pooledBubble.votedMark.enabled = false;
        pooledBubble.Xmark.enabled = false;
        pooledBubble.TextArea.text = message;
        pooledBubble.TextArea.ForceMeshUpdate(true, true);
        pooledBubble.Background.size = new(5.52f, 0.2f + pooledBubble.NameText.GetNotDumbRenderedHeight() + pooledBubble.TextArea.GetNotDumbRenderedHeight());
        pooledBubble.MaskArea.size = pooledBubble.Background.size - new Vector2(0, 0.03f);

        pooledBubble.Background.color = new Color(0.2f, 0.2f, 0.27f, 1f);
        pooledBubble.TextArea.color = Color.white;

        pooledBubble.AlignChildren();
        var pos = pooledBubble.NameText.transform.localPosition;
        pooledBubble.NameText.transform.localPosition = pos;
        chat.AlignAllBubbles();
        if (chat is { IsOpenOrOpening: false, notificationRoutine: null })
        {
            chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
        }
        if (showHeadsup && !chat.IsOpenOrOpening)
        {
            SoundManager.Instance.PlaySound(chat.messageSound, false).pitch = 0.1f;
            chat.chatNotification.SetUp(PlayerControl.LocalPlayer, message);
        }
    }

    public static bool StartsWithVowel(this string word)
    {
        var vowels = new[] { 'a', 'e', 'i', 'o', 'u' };
        return vowels.Any(vowel => word.StartsWith(vowel.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    public static List<PlayerControl> GetCrewmates(List<PlayerControl> impostors)
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(player => impostors.All(imp => imp.PlayerId != player.PlayerId)).ToList();
    }

    public static List<PlayerControl> GetImpostors(List<NetworkedPlayerInfo> infected)
    {
        return infected.Select(impData => impData.Object).ToList();
    }

    public static List<(ushort RoleType, int Chance)> GetRolesToAssign(ModdedRoleTeams team, Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(team);

        return GetRolesToAssign(roles, filter);
    }

    public static List<(ushort RoleType, int Chance)> GetRolesToAssign(RoleAlignment alignment, Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(alignment);

        return GetRolesToAssign(roles, filter);
    }

    private static List<(ushort RoleType, int Chance)> GetRolesToAssign(IEnumerable<RoleBehaviour> roles, Func<RoleBehaviour, bool>? filter = null)
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var assignmentData = roles.Where(x => !x.IsDead && (filter == null || filter(x))).Select(role => new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role), roleOptions.GetChancePerGame(role.Role))).ToList();

        var chosenRoles = GetPossibleRoles(assignmentData);

        var rolesToKeep = chosenRoles.ToList();
        rolesToKeep.Shuffle();

        // Log.Message($"GetRolesToKeep Kept - Count: {rolesToKeep.Count}");
        return rolesToKeep;
    }

    public static List<ushort> GetMaxRolesToAssign(ModdedRoleTeams team, int max = 1, Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(team);

        return GetMaxRolesToAssign(roles, max, filter);
    }

    public static List<ushort> GetMaxRolesToAssign(RoleAlignment alignment, int max, Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(alignment);

        return GetMaxRolesToAssign(roles, max, filter);
    }

    private static List<ushort> GetMaxRolesToAssign(IEnumerable<RoleBehaviour> roles, int max, Func<RoleBehaviour, bool>? filter = null)
    {
        if (max <= 0) return [];

        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var assignmentData = roles.Where(x => !x.IsDead && (filter == null || filter(x))).Select(role => new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role), roleOptions.GetChancePerGame(role.Role))).ToList();

        var chosenRoles = GetPossibleRoles(assignmentData, x => x.Chance == 100);

        // Shuffle to ensure that the same 100% roles do not appear in
        // every game if there are more than the maximum.
        chosenRoles.Shuffle();

        // Truncate the list if there are more 100% roles than the max.
        chosenRoles = chosenRoles.GetRange(0, Math.Min(max, chosenRoles.Count));

        if (chosenRoles.Count < max)
        {
            var potentialRoles = GetPossibleRoles(assignmentData, x => x.Chance < 100);

            // Determine which roles appear in this game.
            var optionalRoles = potentialRoles.Where(x => HashRandom.Next(101) < x.Chance).ToList();
            potentialRoles = potentialRoles.Where(x => !optionalRoles.Contains(x)).ToList();

            optionalRoles.Shuffle();
            chosenRoles.AddRange(optionalRoles.GetRange(0, Math.Min(max - chosenRoles.Count, optionalRoles.Count)));

            // If there are not enough roles after that, randomly add
            // ones which were previously eliminated, up to the max.
            if (chosenRoles.Count < max)
            {
                potentialRoles.Shuffle();
                chosenRoles.AddRange(potentialRoles.GetRange(0, Math.Min(max - chosenRoles.Count, potentialRoles.Count)));
            }
        }

        var rolesToKeep = chosenRoles.Select(x => x.RoleType).ToList();
        rolesToKeep.Shuffle();

        // Log.Message($"GetMaxRolesToAssign Kept - Count: {rolesToKeep.Count}");
        return rolesToKeep;
    }

    private static List<(ushort RoleType, int Chance)> GetPossibleRoles(List<RoleManager.RoleAssignmentData> assignmentData, Func<RoleManager.RoleAssignmentData, bool>? predicate = null)
    {
        var roles = new List<(ushort, int)>();

        assignmentData.Where(x => predicate == null || predicate(x)).ToList().ForEach((x) =>
        {
            for (var i = 0; i < x.Count; i++)
            {
                roles.Add(((ushort)x.Role.Role, x.Chance));
            }
        });

        return roles;
    }

    public static RoleManager.RoleAssignmentData GetAssignData(RoleTypes roleType)
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var role = GetRegisteredRole(roleType);
        var assignmentData = new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role!.Role), roleOptions.GetChancePerGame(role.Role));

        return assignmentData;
    }

    public static PlayerControl? PlayerById(byte id)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == id)
                return player;
        }

        return null;
    }

    public static IEnumerator PerformTimedAction(float duration, Action<float> action)
    {
        for (var t = 0f; t < duration; t += Time.deltaTime)
        {
            action(t / duration);
            yield return new WaitForEndOfFrame();
        }

        action(1f);
    }

    public static IEnumerator CoFlash(Color color, float waitfor = 1f, float alpha = 0.3f)
    {
        color.a = alpha;
        if (HudManager.InstanceExists && HudManager.Instance.FullScreen)
        {
            var fullscreen = HudManager.Instance.FullScreen;
            fullscreen.enabled = true;
            fullscreen.gameObject.SetActive(true);
            fullscreen.color = color;
        }

        yield return new WaitForSeconds(waitfor);

        if (HudManager.InstanceExists && HudManager.Instance.FullScreen)
        {
            var fullscreen = HudManager.Instance.FullScreen;
            if (!fullscreen.color.Equals(color)) yield break;

            fullscreen.color = new Color(1f, 0f, 0f, 0.37254903f);
            fullscreen.enabled = false;
        }
    }

    public static IEnumerator FadeOut(SpriteRenderer? rend, float delay = 0.01f, float decrease = 0.01f)
    {
        if (rend == null)
        {
            yield break;
        }

        var alphaVal = rend.color.a;
        var tmp = rend.color;

        while (alphaVal > 0)
        {
            alphaVal -= decrease;
            tmp.a = alphaVal;
            rend.color = tmp;

            yield return new WaitForSeconds(delay);
        }
    }

    public static IEnumerator FadeIn(SpriteRenderer? rend, float delay = 0.01f, float increase = 0.01f)
    {
        if (rend == null)
        {
            yield break;
        }

        var tmp = rend.color;
        tmp.a = 0;
        rend.color = tmp;

        while (rend.color.a < 1)
        {
            tmp.a = Mathf.Min(rend.color.a + increase, 1f); // Ensure it doesn't go above 1
            rend.color = tmp;

            yield return new WaitForSeconds(delay);
        }
    }

    public static GameObject CreateSpherePrimitive(Vector3 location, float radius)
    {
        var spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        spherePrimitive.name = "Sphere Primitive";
        spherePrimitive.transform.localScale = new Vector3(
            radius * ShipStatus.Instance.MaxLightRadius * 2f,
            radius * ShipStatus.Instance.MaxLightRadius * 2f,
            radius * ShipStatus.Instance.MaxLightRadius * 2f);

        Object.Destroy(spherePrimitive.GetComponent<SphereCollider>());

        spherePrimitive.GetComponent<MeshRenderer>().material = AuAvengersAnims.BombMaterial.LoadAsset();
        spherePrimitive.transform.position = location;

        return spherePrimitive;
    }
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input; // Return empty or null string if input is empty or null
        }

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower(CultureInfo.CurrentCulture)); // Convert to lowercase first and then title case
    }


    public static ArrowBehaviour CreateArrow(Transform parent, Color color)
    {
        var gameObject = new GameObject("Arrow")
        {
            layer = 5,
            transform =
            {
                parent = parent
            }
        };

        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = TouAssets.ArrowSprite.LoadAsset();
        renderer.color = color;

        var arrow = gameObject.AddComponent<ArrowBehaviour>();
        arrow.image = renderer;
        arrow.image.color = color;

        return arrow;
    }

    public static IEnumerator BetterBloop(Transform target, float delay = 0, float finalSize = 1f, float duration = 0.5f, float intensity = 1f)
    {
        for (var t = 0f; t < delay; t += Time.deltaTime)
        {
            yield return null;
        }
        var localScale = default(Vector3);
        for (var t = 0f; t < duration; t += Time.deltaTime)
        {
            var z = 1f + (Effects.ElasticOut(t, duration) - 1f) * intensity;
            z *= finalSize;
            localScale.x = localScale.y = localScale.z = z;
            target.localScale = localScale;
            yield return null;
        }
        localScale.z = localScale.y = localScale.x = finalSize;
        target.localScale = localScale;
    }

    public static void AdjustGhostTasks(PlayerControl player)
    {
        foreach (var task in player.myTasks)
        {
            if (task.TryCast<NormalPlayerTask>() != null)
            {
                var normalPlayerTask = task.Cast<NormalPlayerTask>();

                var updateArrow = normalPlayerTask.taskStep > 0;

                normalPlayerTask.taskStep = 0;
                normalPlayerTask.Initialize();
                if (normalPlayerTask.TaskType is TaskTypes.PickUpTowels)
                {
                    foreach (var console in Object.FindObjectsOfType<TowelTaskConsole>())
                    {
                        console.Image.color = Color.white;
                    }
                }

                normalPlayerTask.taskStep = 0;
                if (normalPlayerTask.TaskType == TaskTypes.UploadData)
                {
                    normalPlayerTask.taskStep = 1;
                }
                if (normalPlayerTask.TaskType is TaskTypes.EmptyGarbage or TaskTypes.EmptyChute
                    && (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 0 ||
                    GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3 ||
                    GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4))
                {
                    normalPlayerTask.taskStep = 1;
                }

                if (updateArrow)
                {
                    normalPlayerTask.UpdateArrowAndLocation();
                }

                var taskInfo = player.Data.FindTaskById(task.Id);
                taskInfo.Complete = false;
            }
        }
    }

    public static void UpdateLocalPlayerCamera(MonoBehaviour target, Transform lightParent)
    {
        HudManager.Instance.PlayerCam.SetTarget(target);
        PlayerControl.LocalPlayer.lightSource.transform.parent = lightParent;
        PlayerControl.LocalPlayer.lightSource.Initialize(PlayerControl.LocalPlayer.Collider.offset / 2);
    }

    public static void SnapPlayerCamera(MonoBehaviour target)
    {
        var cam = HudManager.Instance.PlayerCam;
        cam.SetTarget(target);
        cam.centerPosition = cam.Target.transform.position;
    }

    public static List<ushort> ReadFromBucket(List<RoleListOption> buckets, List<(ushort RoleType, int Chance)> roles, RoleListOption roleType, RoleListOption replaceType)
    {
        var result = new List<ushort>();

        while (buckets.Contains(roleType))
        {
            if (roles.Count == 0)
            {
                var count = buckets.RemoveAll(x => x == roleType);
                buckets.AddRange(Enumerable.Repeat(replaceType, count));

                break;
            }

            var addedRole = SelectRole(roles);
            result.Add(addedRole.RoleType);
            roles.Remove(addedRole);

            buckets.Remove(roleType);
        }

        return result;
    }

    public static List<ushort> ReadFromBucket(List<RoleListOption> buckets, List<(ushort RoleType, int Chance)> roles, RoleListOption roleType)
    {
        var result = new List<ushort>();

        while (buckets.Contains(roleType))
        {
            if (roles.Count == 0)
            {
                buckets.RemoveAll(x => x == roleType);

                break;
            }

            var addedRole = SelectRole(roles);
            result.Add(addedRole.RoleType);
            roles.Remove(addedRole);

            buckets.Remove(roleType);
        }

        return result;
    }

    public static (ushort RoleType, int Chance) SelectRole(List<(ushort RoleType, int Chance)> roles)
    {
        var chosenRoles = roles.Where(x => x.Chance == 100).ToList();
        if (chosenRoles.Count > 0)
        {
            chosenRoles.Shuffle();
            return chosenRoles.TakeFirst();
        }

        chosenRoles = roles.Where(x => x.Chance < 100).ToList();
        var total = chosenRoles.Sum(x => x.Chance);
        var random = Random.RandomRangeInt(1, total + 1);

        var cumulative = 0;
        (ushort RoleType, int SpawnChance) selectedRole = default;

        foreach (var role in chosenRoles)
        {
            cumulative += role.Chance;
            if (random <= cumulative)
            {
                selectedRole = role;
                break;
            }
        }

        return selectedRole;
    }

    /// <summary>
    /// Gets a FakePlayer by comparing PlayerControl.
    /// </summary>
    /// <param name="player">The player themselves.</param>
    /// <returns>A fake player or null if its not found.</returns>
    public static FakePlayer? GetFakePlayer(PlayerControl player)
    {
        var faker = FakePlayer.FakePlayers.FirstOrDefault(x => x.body?.name == $"Fake {player.gameObject.name}");
        return faker != null ? faker : null;
    }

    public static bool IsMap(byte mapid)
    {
        return GameOptionsManager.Instance.currentNormalGameOptions.MapId == mapid
              || TutorialManager.InstanceExists && AmongUsClient.Instance.TutorialMapId == mapid;
    }
    public static bool IsConcealed(this PlayerControl Player)
    {
        if (Player.HasModifier<ConcealedModifier>() || !Player.Visible || (Player.TryGetModifier<DisabledModifier>(out var mod) && !mod.IsConsideredAlive))
        {
            return true;
        }

        if (Player.inVent)
        {
            return true;
        }

        var mushroom = UnityEngine.Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            return true;
        }

        if (OptionGroupSingleton<GeneralOptions>.Instance.CamouflageComms)
        {
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var commsSystem) || commsSystem == null)
            {
                return false;
            }

            var isActive = false;
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq || ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
            {
                var hqSystem = commsSystem.Cast<HqHudSystemType>();
                if (hqSystem != null) isActive = hqSystem.IsActive;
            }
            else
            {
                var hudSystem = commsSystem.Cast<HudOverrideSystemType>();
                if (hudSystem != null) isActive = hudSystem.IsActive;
            }

            return isActive;
        }

        return false;
    }
}
