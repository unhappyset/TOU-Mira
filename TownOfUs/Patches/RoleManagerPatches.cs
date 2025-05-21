using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using Random = UnityEngine.Random;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class TouRoleManagerPatches
{
    private static List<RoleTypes> crewmateGhostRolePool = [];
    private static List<RoleTypes> impostorGhostRolePool = [];
    private static List<RoleTypes> customGhostRolePool = [];
    public static bool ReplaceRoleManager;

    private static void GhostRoleSetup()
    {
        // var ghostRoles = RoleManager.Instance.AllRoles.Where(x => x.IsDead);
        var ghostRoles = MiscUtils.GetRegisteredGhostRoles();

        // Logger<TownOfUsPlugin>.Message($"GhostRoleSetup - ghostRoles Count: {ghostRoles.Count()}");
        crewmateGhostRolePool.Clear();
        impostorGhostRolePool.Clear();
        customGhostRolePool.Clear();

        foreach (var role in ghostRoles)
        {
            // Logger<TownOfUsPlugin>.Message($"GhostRoleSetup - ghostRoles role NiceName: {role.NiceName}");
            var data = MiscUtils.GetAssignData(role.Role);

            switch (data.Chance)
            {
                case 100:
                    {
                        if (data.Count > 0)
                        {
                            if (role is ICustomRole cRole && cRole.Team == ModdedRoleTeams.Custom)
                                customGhostRolePool.Add(role.Role);
                            else if (role.TeamType == RoleTeamTypes.Crewmate)
                                crewmateGhostRolePool.Add(role.Role);
                            else if (role.TeamType == RoleTeamTypes.Impostor)
                                impostorGhostRolePool.Add(role.Role);
                        }

                        break;
                    }
                case > 0:
                    {
                        if (data.Count > 0 && HashRandom.Next(101) < data.Chance)
                        {
                            if (role is ICustomRole cRole && cRole.Team == ModdedRoleTeams.Custom)
                                customGhostRolePool.Add(role.Role);
                            else if (role.TeamType == RoleTeamTypes.Crewmate)
                                crewmateGhostRolePool.Add(role.Role);
                            else if (role.TeamType == RoleTeamTypes.Impostor)
                                impostorGhostRolePool.Add(role.Role);
                        }

                        break;
                    }
            }
        }

        crewmateGhostRolePool.RemoveAll(x => x == (RoleTypes)RoleId.Get<HaunterRole>());
        customGhostRolePool.RemoveAll(x => x == (RoleTypes)RoleId.Get<PhantomTouRole>());
    }

    private static void AssignRoles(List<NetworkedPlayerInfo> infected)
    {
        var impCount = infected.Count;
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var nbCount = Random.RandomRange((int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralBenign.Value, (int)OptionGroupSingleton<RoleOptions>.Instance.MaxNeutralBenign.Value + 1);
        var neCount = Random.RandomRange((int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralEvil.Value, (int)OptionGroupSingleton<RoleOptions>.Instance.MaxNeutralEvil.Value + 1);
        var nkCount = Random.RandomRange((int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralKiller.Value, (int)OptionGroupSingleton<RoleOptions>.Instance.MaxNeutralKiller.Value + 1);

        var canSubtract = (int faction, int minFaction) => { return faction > minFaction; };
        var factions = new List<string>() { "Benign", "Evil", "Killing" };

        // Crew must always start out outnumbering neutrals, so subtract roles until that can be guaranteed.
        while (Math.Ceiling((double)crewmates.Count / 2) <= nbCount + neCount + nkCount)
        {
            bool canSubtractBenign = canSubtract(nbCount, (int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralBenign.Value);
            bool canSubtractEvil = canSubtract(neCount, (int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralEvil.Value);
            bool canSubtractKilling = canSubtract(nkCount, (int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralKiller.Value);
            bool canSubtractNone = !canSubtractBenign && !canSubtractEvil && !canSubtractKilling;

            factions.Shuffle();
            switch (factions[0])
            {
                case "Benign":
                    if (nbCount > 0 && (canSubtractBenign || canSubtractNone))
                    {
                        nbCount -= 1;
                        break;
                    }
                    goto case "Evil";
                case "Evil":
                    if (neCount > 0 && (canSubtractEvil || canSubtractNone))
                    {
                        neCount -= 1;
                        break;
                    }
                    goto case "Killing";
                case "Killing":
                    if (nkCount > 0 && (canSubtractKilling || canSubtractNone))
                    {
                        nkCount -= 1;
                        break;
                    }
                    goto default;
                default:
                    if (nbCount > 0)
                    {
                        nbCount -= 1;
                    }
                    else if (neCount > 0)
                    {
                        neCount -= 1;
                    }
                    else if (nkCount > 0)
                    {
                        nkCount -= 1;
                    }
                    break;
            }

            if (nbCount + neCount + nkCount == 0)
                break;
        }
        var excluded = MiscUtils.AllRoles.Where(x => x is ISpawnChange change && change.NoSpawn).Select(x => x.Role);
        Func<RoleBehaviour, bool>? impFilter = (x => !excluded.Contains(x.Role));

        var impRoles = MiscUtils.GetRolesToAssign(ModdedRoleTeams.Impostor, impCount, impFilter);

        var uniqueRole = MiscUtils.AllRoles.FirstOrDefault(x => x is ISpawnChange change2 && !change2.NoSpawn);
        if (uniqueRole != null && impRoles.Contains(RoleId.Get(uniqueRole.GetType())))
        {
            impCount = 1;
            impRoles.RemoveAll(x => x != RoleId.Get(uniqueRole.GetType()));

            while (impostors.Count > impCount)
            {
                crewmates.Add(impostors.TakeFirst());
            }
        }
        
        var nbRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralBenign, nbCount);
        var neRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralEvil, neCount);
        var nkRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralKilling, nkCount);

        var crewCount = crewmates.Count - nbRoles.Count - neRoles.Count - nkRoles.Count;

        Func<RoleBehaviour, bool>? crewFilter = null;

        if ((MapNames)GameOptionsManager.Instance.GameHostOptions.MapId == MapNames.Fungle)
        {
            crewFilter = (x => x.Role != (RoleTypes)RoleId.Get<SpyRole>());
        }

        var crewRoles = MiscUtils.GetRolesToAssign(ModdedRoleTeams.Crewmate, crewCount, crewFilter);

        var crewAndNeutRoles = new List<ushort>();
        crewAndNeutRoles.AddRange(nbRoles);
        crewAndNeutRoles.AddRange(neRoles);
        crewAndNeutRoles.AddRange(nkRoles);
        crewAndNeutRoles.AddRange(crewRoles);
        crewAndNeutRoles.Shuffle();

        foreach (var role in crewAndNeutRoles)
        {
            int num = HashRandom.FastNext(crewmates.Count);
            var player = crewmates[num];

            player.RpcSetRole((RoleTypes)role);

            crewmates.RemoveAt(num);

            // Logger<TownOfUsPlugin>.Message($"SelectRoles - player: '{player.Data.PlayerName}', role: '{(RoleType)item}'");
        }

        foreach (var role in impRoles)
        {
            int num = HashRandom.FastNext(impostors.Count);
            var player = impostors[num];

            player.RpcSetRole((RoleTypes)role);

            impostors.RemoveAt(num);

            // Logger<TownOfUsPlugin>.Message($"SelectRoles - player: '{player.Data.PlayerName}', role: '{(RoleType)item}'");
        }

        foreach (var player in crewmates)
        {
            player.RpcSetRole(RoleTypes.Crewmate);
        }

        foreach (var player in impostors)
        {
            player.RpcSetRole(RoleTypes.Impostor);
        }
    }

    private static void AssignRolesFromRoleList(List<NetworkedPlayerInfo> infected)
    {
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var crewRoles = new List<ushort>();
        var impRoles = new List<ushort>();

        var opts = OptionGroupSingleton<RoleOptions>.Instance;

        // sort out bad lists
        var players = impostors.Count + crewmates.Count;
        List<RoleListOption> crewNKBuckets = [RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewPower, RoleListOption.CrewProtective,
                RoleListOption.CrewSupport, RoleListOption.CrewCommon, RoleListOption.CrewSpecial, RoleListOption.CrewRandom, RoleListOption.NeutKilling];
        List<RoleListOption> impBuckets = [RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpSupport, RoleListOption.ImpCommon, RoleListOption.ImpRandom];
        List<RoleListOption> buckets = [(RoleListOption)opts.Slot1.Value, (RoleListOption)opts.Slot2.Value, (RoleListOption)opts.Slot3.Value, (RoleListOption)opts.Slot4.Value];
        var impCount = 0;
        var anySlots = 0;

        if (players > 4) buckets.Add((RoleListOption)opts.Slot5.Value);
        if (players > 5) buckets.Add((RoleListOption)opts.Slot6.Value);
        if (players > 6) buckets.Add((RoleListOption)opts.Slot7.Value);
        if (players > 7) buckets.Add((RoleListOption)opts.Slot8.Value);
        if (players > 8) buckets.Add((RoleListOption)opts.Slot9.Value);
        if (players > 9) buckets.Add((RoleListOption)opts.Slot10.Value);
        if (players > 10) buckets.Add((RoleListOption)opts.Slot11.Value);
        if (players > 11) buckets.Add((RoleListOption)opts.Slot12.Value);
        if (players > 12) buckets.Add((RoleListOption)opts.Slot13.Value);
        if (players > 13) buckets.Add((RoleListOption)opts.Slot14.Value);
        if (players > 14) buckets.Add((RoleListOption)opts.Slot15.Value);
        if (players > 15)
        {
            for (int i = 0; i < players - 15; i++)
            {
                int random = Random.RandomRangeInt(0, 4);
                if (random == 0) 
                    buckets.Add(RoleListOption.CrewRandom);
                else 
                    buckets.Add(RoleListOption.NonImp);
            }
        }

        // imp issues
        foreach (var roleOption in buckets)
        {
            if (impBuckets.Contains(roleOption)) 
                impCount += 1;
            else if (roleOption == RoleListOption.Any) 
                anySlots += 1;
        }

        while (impCount > impostors.Count)
        {
            buckets.Shuffle();
            buckets.Remove(buckets.FindLast(x => impBuckets.Contains(x)));
            buckets.Add(RoleListOption.NonImp);
            impCount -= 1;
        }

        while (impCount + anySlots < impostors.Count)
        {
            buckets.Shuffle();
            buckets.RemoveAt(0);
            buckets.Add(RoleListOption.ImpRandom);
            impCount += 1;
        }

        while (buckets.Contains(RoleListOption.Any))
        {
            buckets.Shuffle();
            buckets.Remove(buckets.FindLast(x => x == RoleListOption.Any));
            if (impCount < impostors.Count)
            {
                buckets.Add(RoleListOption.ImpRandom);
                impCount += 1;
            }
            else
            {
                buckets.Add(RoleListOption.NonImp);
            }
        }

        // crew and neut issues
        var noChange = false;
        var nonImp = false;
        var randNeut = false;

        foreach (var roleOption in buckets)
        {
            if (crewNKBuckets.Contains(roleOption))
            {
                noChange = true;
                break;
            }
            else if (roleOption == RoleListOption.NeutRandom)
            {
                randNeut = true;
                break;
            }
            else if (roleOption == RoleListOption.NonImp)
            {
                nonImp = true;
                break;
            }
        }

        if (!noChange)
        {
            List<RoleListOption> add = [RoleListOption.CrewRandom, RoleListOption.NeutKilling];
            add.Shuffle();

            if (randNeut)
            {
                buckets.Remove(RoleListOption.NeutRandom);
                buckets.Add(RoleListOption.NeutKilling);
            }
            else if (nonImp)
            {
                buckets.Remove(RoleListOption.NonImp);
                buckets.Add(add[0]);
            }
            else
            {
                buckets.Remove(buckets.FindLast(x => !impBuckets.Contains(x)));
                buckets.Add(add[0]);
            }
        }

        Func<RoleBehaviour, bool>? crewFilter = null;
        if ((MapNames)GameOptionsManager.Instance.GameHostOptions.MapId == MapNames.Fungle)
        {
            crewFilter = (x => x.Role != (RoleTypes)RoleId.Get<SpyRole>());
        }
        var excluded = MiscUtils.AllRoles.Where(x => x is ISpawnChange && ((ISpawnChange)x).NoSpawn).Select(x => x.Role).ToList();
        Func<RoleBehaviour, bool>? impFilter = (x => !excluded.Contains(x.Role));

        var crewInvestRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateInvestigative, filter: crewFilter);
        var crewKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateKilling);
        var crewProtectRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateProtective);
        var crewPowerRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmatePower);
        var crewSupportRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateSupport);
        var neutBenignRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralBenign);
        var neutEvilRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralEvil);
        var neutKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralKilling);
        var impConcealRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorConcealing);
        var impKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorKilling, filter: impFilter);
        var impSupportRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorSupport);

        // imp buckets
        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, impConcealRoles, RoleListOption.ImpConceal, RoleListOption.ImpCommon));

        var commonImpRoles = impConcealRoles;

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, impSupportRoles, RoleListOption.ImpSupport, RoleListOption.ImpCommon));

        commonImpRoles.AddRange(impSupportRoles);

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, impKillingRoles, RoleListOption.ImpKilling, RoleListOption.ImpRandom));

        var randomImpRoles = impKillingRoles;

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, commonImpRoles, RoleListOption.ImpCommon, RoleListOption.ImpRandom));

        randomImpRoles.AddRange(commonImpRoles);

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomImpRoles, RoleListOption.ImpRandom));

        // crew buckets
        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewInvestRoles, RoleListOption.CrewInvest, RoleListOption.CrewCommon));

        var commonCrewRoles = crewInvestRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewProtectRoles, RoleListOption.CrewProtective, RoleListOption.CrewCommon));

        commonCrewRoles.AddRange(crewProtectRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewSupportRoles, RoleListOption.CrewSupport, RoleListOption.CrewCommon));

        commonCrewRoles.AddRange(crewSupportRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewKillingRoles, RoleListOption.CrewKilling, RoleListOption.CrewSpecial));

        var specialCrewRoles = crewKillingRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewPowerRoles, RoleListOption.CrewPower, RoleListOption.CrewSpecial));

        specialCrewRoles.AddRange(crewPowerRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, commonCrewRoles, RoleListOption.CrewCommon, RoleListOption.CrewRandom));

        var randomCrewRoles = commonCrewRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, specialCrewRoles, RoleListOption.CrewSpecial, RoleListOption.CrewRandom));

        randomCrewRoles.AddRange(specialCrewRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomCrewRoles, RoleListOption.CrewRandom));

        var randomNonImpRoles = randomCrewRoles;

        // neutral buckets
        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutBenignRoles, RoleListOption.NeutBenign, RoleListOption.NeutCommon));

        var commonNeutRoles = neutBenignRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutEvilRoles, RoleListOption.NeutEvil, RoleListOption.NeutCommon));

        commonNeutRoles.AddRange(neutEvilRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutKillingRoles, RoleListOption.NeutKilling, RoleListOption.NeutRandom));

        var randomNeutRoles = neutKillingRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, commonNeutRoles, RoleListOption.NeutCommon, RoleListOption.NeutRandom));

        randomNeutRoles.AddRange(commonNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomNeutRoles, RoleListOption.NeutRandom, RoleListOption.NonImp));

        randomNonImpRoles.AddRange(randomNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomNonImpRoles, RoleListOption.NonImp));

        // Shuffle roles before handing them out.
        // This should ensure a statistically equal chance of all permutations of roles.
        crewRoles.Shuffle();
        impRoles.Shuffle();

        var chosenImpRoles = impRoles.Take(impCount).ToList();
        
        var uniqueRole = MiscUtils.AllRoles.FirstOrDefault(x => x is ISpawnChange change2 && !change2.NoSpawn);

        if (uniqueRole != null && chosenImpRoles.Contains(RoleId.Get(uniqueRole.GetType())))
        {
            impCount = 1;

            while (impostors.Count > impCount)
            {
                crewmates.Add(impostors.TakeFirst());
            }
        }

        foreach (var role in chosenImpRoles)
        {
            int num = HashRandom.FastNext(impostors.Count);
            var player = impostors[num];

            player.RpcSetRole((RoleTypes)role);

            impostors.RemoveAt(num);
        }

        foreach (var role in crewRoles)
        {
            int num = HashRandom.FastNext(crewmates.Count);
            var player = crewmates[num];

            player.RpcSetRole((RoleTypes)role);

            crewmates.RemoveAt(num);
        }

        // Assign vanilla roles to anyone who did not receive a role.
        foreach (var player in crewmates)
        {
            player.RpcSetRole(RoleTypes.Crewmate);
        }

        foreach (var player in impostors)
        {
            player.RpcSetRole(RoleTypes.Impostor);
        }
    }

    public static void AssignTargets()
    {
        foreach (var role in MiscUtils.AllRoles.Where(x => x is IAssignableTargets))
        {
            var assignRole = role as IAssignableTargets;
            if (assignRole != null) assignRole.AssignTargets();
        }
        foreach (var modifier in MiscUtils.AllModifiers.Where(x => x is IAssignableTargets))
        {
            var assignMod = modifier as IAssignableTargets;
            if (assignMod != null) assignMod.AssignTargets();
        }
        GhostRoleSetup();
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool SelectRolesPatch(RoleManager __instance)
    {
        if (TutorialManager.InstanceExists || ReplaceRoleManager) return true;

        var players = GameData.Instance.AllPlayers.ToArray().ToList();
        players.Shuffle();

        var impCount = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(players.Count);
        var infected = players.Take(impCount).ToList();

        if (OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled)
        {
            AssignRolesFromRoleList(infected);
        }
        else
        {
            AssignRoles(infected);
        }

        AssignTargets();

        return false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    [HarmonyPrefix]
    public static bool RpcSetRolePatch(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType, [HarmonyArgument(1)] bool canOverrideRole = false)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            __instance.StartCoroutine(__instance.CoSetRole(roleType, canOverrideRole));
        }

        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 44, SendOption.Reliable);
        messageWriter.Write((ushort)roleType);
        messageWriter.Write(canOverrideRole);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    [HarmonyPrefix]
    public static bool AssignRoleOnDeathPatch(RoleManager __instance, PlayerControl player, bool specialRolesAllowed)
    {
        // Note: I know this is a like for like recreation of the AssignRoleOnDeath function but for some reason
        // the original won't spawn the Phantom and just spawns Neutral Ghost instead

        // Logger<TownOfUsPlugin>.Message($"AssignRoleOnDeathPatch - Player: '{player.Data.PlayerName}', specialRolesAllowed: {specialRolesAllowed}");
        if (player == null || !player.Data.IsDead)
        {
            // Logger<TownOfUsPlugin>.Message($"AssignRoleOnDeathPatch - !player.Data.IsDead: '{!player.Data.IsDead}'");
            return false;
        }

        if (!player.Data.Role.IsImpostor && specialRolesAllowed)
        {
            // Logger<TownOfUsPlugin>.Message($"AssignRoleOnDeathPatch - !player.Data.Role.IsImpostor: '{!player.Data.Role.IsImpostor}' specialRolesAllowed: {specialRolesAllowed}");
            RoleManager.TryAssignSpecialGhostRoles(player);
        }

        if (!RoleManager.IsGhostRole(player.Data.Role.Role))
        {
            // Logger<TownOfUsPlugin>.Message($"AssignRoleOnDeathPatch - !RoleManager.IsGhostRole(player.Data.Role.Role): '{!RoleManager.IsGhostRole(player.Data.Role.Role)}'");
            player.RpcSetRole(player.Data.Role.DefaultGhostRole, false);
        }

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.TryAssignSpecialGhostRoles))]
    [HarmonyPrefix]
    public static bool TryAssignSpecialGhostRolesPatch(RoleManager __instance, PlayerControl player)
    {
        // Logger<TownOfUsPlugin>.Message($"TryAssignSpecialGhostRolesPatch - Player: '{player.Data.PlayerName}'");
        var ghostRole = RoleTypes.CrewmateGhost;

        if (player.IsCrewmate() && crewmateGhostRolePool.Count > 0)
        {
            ghostRole = crewmateGhostRolePool.TakeFirst();
        }
        else if (player.IsImpostor() && impostorGhostRolePool.Count > 0)
        {
            ghostRole = impostorGhostRolePool.TakeFirst();
        }
        else if (player.IsNeutral() && customGhostRolePool.Count > 0)
        {
            ghostRole = customGhostRolePool.TakeFirst();
        }

        if (ghostRole != RoleTypes.CrewmateGhost && ghostRole != RoleTypes.ImpostorGhost && ghostRole != (RoleTypes)RoleId.Get<NeutralGhostRole>())
        {
            // var newRole = RoleManager.Instance.GetRole(ghostRole);
            // Logger<TownOfUsPlugin>.Message($"TryAssignSpecialGhostRolesPatch - ghostRoles role: {newRole.NiceName}");
            player.RpcChangeRole((ushort)ghostRole);
        }

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
    [HarmonyPostfix]
    public static void SetRolePatch(RoleManager __instance, [HarmonyArgument(0)] PlayerControl targetPlayer, [HarmonyArgument(1)] RoleTypes roleType)
    {
        GameHistory.RegisterRole(targetPlayer, targetPlayer.Data.Role);
    }
}
