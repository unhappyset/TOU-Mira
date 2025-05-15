using System.Collections;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using PowerTools;
using Reactor.Utilities;
using TownOfUs.Modules.Wiki;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MayorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable
{
    public string RoleName => "Mayor";
    public string RoleDescription => "Reveal Yourself To Save The Crew";
    public string RoleLongDescription => "Lead the crew to victory";
    public Color RoleColor => TownOfUsColors.Mayor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public bool Revealed { get; set; }
    public static GameObject MayorPlayer;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Mayor,
        HideSettings = true,
        MaxRoleCount = 0,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        CanModifyChance = false,
    };

    public bool IsPowerCrew => true;
    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);
        if (!MeetingHud.Instance) return;
        OnMeetingStart();
    }

    public override void OnMeetingStart()
    {
        RoleStubs.RoleBehaviourOnMeetingStart(this);

        var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == Player.PlayerId);
        if (!Revealed) Coroutines.Start(CoAnimateReveal(targetVoteArea));
        else Coroutines.Start(CoAnimatePostReveal(targetVoteArea));
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    public static bool MayorVisibilityFlag(PlayerControl player)
    {
        return player.IsRole<MayorRole>() && player.GetRole<MayorRole>()!.Revealed;
    }
    private static IEnumerator CoAnimateReveal(PlayerVoteArea voteArea)
    {
        MayorPlayer = UnityEngine.Object.Instantiate(TouAssets.MayorRevealPrefab.LoadAsset(), voteArea.transform);
        MayorPlayer.transform.localPosition = new Vector3(-0.8f, 0, 0);
        MayorPlayer.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        MayorPlayer.gameObject.layer = MayorPlayer.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = MayorPlayer.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        var handRend = MayorPlayer.transform.FindRecursive("Hands").GetComponent<SpriteRenderer>();
        if (!handRend)
        {
            handRend = MayorPlayer.transform.FindRecursive("Hand").GetComponent<SpriteRenderer>();
        }
        if (handRend) handRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;

        voteArea.PlayerIcon.gameObject.SetActive(false);
        MayorPlayer.gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(0).gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(1).gameObject.SetActive(true);

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Mayor, 0.15f, 0.15f));

        var bodysAnim = MayorPlayer.GetComponent<SpriteAnim>();
        var outfitAnim = MayorPlayer.transform.GetChild(0).GetComponent<SpriteAnim>();
        var handAnim = MayorPlayer.transform.GetChild(1).GetComponent<SpriteAnim>();
            bodysAnim.SetSpeed(1.02f);
            outfitAnim.SetSpeed(1.02f);
            handAnim.SetSpeed(1.02f);
        TouAudio.PlaySound(TouAudio.MayorRevealSound);
        yield return new WaitForSeconds(0.1f);
        var player = MiscUtils.PlayerById(voteArea.TargetPlayerId);
        if (player!.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = true;
        }

        yield return new WaitForSeconds(bodysAnim.m_currAnim.length - 0.25f);
    }
    private static IEnumerator CoAnimatePostReveal(PlayerVoteArea voteArea)
    {
        MayorPlayer = UnityEngine.Object.Instantiate(TouAssets.MayorRevealPrefab.LoadAsset(), voteArea.transform);
        MayorPlayer.transform.localPosition = new Vector3(-0.8f, 0, 0);
        MayorPlayer.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        MayorPlayer.gameObject.layer = MayorPlayer.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = MayorPlayer.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        var handRend = MayorPlayer.transform.FindRecursive("Hands").GetComponent<SpriteRenderer>();
        if (!handRend)
        {
            handRend = MayorPlayer.transform.FindRecursive("Hand").GetComponent<SpriteRenderer>();
        }
        if (handRend) handRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;

        voteArea.PlayerIcon.gameObject.SetActive(false);
        MayorPlayer.gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(0).gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(1).gameObject.SetActive(true);

        var bodysAnim = MayorPlayer.GetComponent<SpriteAnim>();
        var outfitAnim = MayorPlayer.transform.GetChild(0).GetComponent<SpriteAnim>();
        var handAnim = MayorPlayer.transform.GetChild(1).GetComponent<SpriteAnim>();
            bodysAnim.SetSpeed(1.02f);
            outfitAnim.SetSpeed(1.02f);
            handAnim.SetSpeed(1.02f);
        yield return new WaitForSeconds(bodysAnim.m_currAnim.length - 0.25f);
    }
    public static void DestroyReveal(PlayerVoteArea voteArea)
    {
        if (MayorPlayer != null)
        {
            MayorPlayer.gameObject.SetActive(false);
            voteArea.PlayerIcon.gameObject.SetActive(true);
            Destroy(MayorPlayer);
            MayorPlayer = null!;
        }
    }
    public string GetAdvancedDescription()
    {
        return "The Mayor is a Crewmate Power role that gains three votes and is revealed to all players, also changing their look in meetings.";
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}
