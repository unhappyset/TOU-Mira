using HarmonyLib;
using TownOfUs.Roles.Other;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class HideSpecVoteAreas
{
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var voteArea in __instance.playerStates)
        {
            if (SpectatorRole.TrackedSpectators.Contains(GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Data.PlayerName))
                voteArea.gameObject.SetActive(false);
        }
    }
}