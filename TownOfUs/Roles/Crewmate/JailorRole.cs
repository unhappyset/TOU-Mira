using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Stubs;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles.Crewmate;

public sealed class JailorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Jailor";
    public string RoleDescription => "Jail And Execute The <color=#FF0000FF>Impostors</color>";
    public string RoleLongDescription => "Execute evildoers in meetings but avoid crewmates";
    public Color RoleColor => TownOfUsColors.Jailor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public DoomableType DoomHintType => DoomableType.Relentless;
    public bool IsPowerCrew => Executes > 0; // Stop end game checks if the Jailor can still execute someone
    public override bool IsAffectedByComms => false;
    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Jailor,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Impostor),
    };

    public int Executes { get; set; } = (int)OptionGroupSingleton<JailorOptions>.Instance.MaxExecutes;
    public PlayerControl Jailed => PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.GetModifier<JailedModifier>()?.JailorId == Player.PlayerId)!;

    private GameObject? jailCell;
    private GameObject? executeButton;
    private TMP_Text? usesText;

    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);

        Executes = (int)OptionGroupSingleton<JailorOptions>.Instance.MaxExecutes;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);

        Clear();
    }

    public override void OnMeetingStart()
    {
        RoleStubs.RoleBehaviourOnMeetingStart(this);

        Clear();

        if (Player.HasDied()) return;

        if (PlayerControl.LocalPlayer.GetModifier<JailedModifier>()?.JailorId == Player.PlayerId)
        {
            var title = $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>Jailee Feedback</color>";
            var text = "You are jailed, convince the Jailor that you are Crew to avoid being executed";
            if (PlayerControl.LocalPlayer.Is(ModdedRoleTeams.Crewmate)) text = "You are jailed, provide relevant information to the Jailor to prove you are Crew";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, text, true, true);

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Jailor.ToTextColor()}{text}</color></b>", Color.white, spr: TouRoleIcons.Jailor.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }

        if (Player.AmOwner)
        {
            if (Jailed!.HasDied())
                return;
            var title = $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>Jailor Feedback</color>";
            MiscUtils.AddFakeChat(Jailed.Data, title, "Use /jail to communicate with your jailee", true, true);
        }

        if (MeetingHud.Instance)
            AddMeetingButtons(MeetingHud.Instance);
    }

    public override void OnVotingComplete()
    {
        RoleStubs.RoleBehaviourOnVotingComplete(this);

        executeButton?.Destroy();
        usesText?.Destroy();
    }

    public void Clear()
    {
        jailCell?.Destroy();
        executeButton?.Destroy();
        usesText?.Destroy();
    }

    public void LobbyStart()
    {
        Clear();
    }

    private void AddMeetingButtons(MeetingHud __instance)
    {
        if (Jailed == null || Jailed?.HasDied() == true) return;

        foreach (var voteArea in __instance.playerStates)
        {
            if (Jailed?.PlayerId == voteArea.TargetPlayerId)
            {
                GenCell(voteArea);
            }
        }

        if (!Player.AmOwner) return;

        if (Executes <= 0 || Jailed?.HasDied() == true) return;

        foreach (var voteArea in __instance.playerStates)
        {
            if (Jailed?.PlayerId == voteArea.TargetPlayerId)
            {
                // if (!(jailorRole.Jailed.IsLover() && PlayerControl.LocalPlayer.IsLover()))
                GenButton(voteArea);
            }
        }
    }

    private void GenCell(PlayerVoteArea voteArea)
    {
        var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;
        var parent = confirmButton.transform.parent.parent;

        var jailCellObj = Object.Instantiate(confirmButton, voteArea.transform);

        var cellRenderer = jailCellObj.GetComponent<SpriteRenderer>();
        cellRenderer.sprite = TouAssets.InJailSprite.LoadAsset();

        jailCellObj.transform.localPosition = new Vector3(-0.95f, 0f, -2f);
        jailCellObj.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        jailCellObj.layer = 5;
        jailCellObj.transform.parent = parent;
        jailCellObj.transform.GetChild(0).gameObject.Destroy();

        var passive = jailCellObj.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();

        jailCell = jailCellObj;
    }

    private void GenButton(PlayerVoteArea voteArea)
    {
        var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;

        var newButtonObj = Object.Instantiate(confirmButton, voteArea.transform);
        newButtonObj.transform.position = confirmButton.transform.position - new Vector3(0.75f, 0f, 0f);
        newButtonObj.transform.localScale *= 0.8f;
        newButtonObj.layer = 5;
        newButtonObj.transform.parent = confirmButton.transform.parent.parent;

        executeButton = newButtonObj;

        var renderer = newButtonObj.GetComponent<SpriteRenderer>();
        renderer.sprite = TouAssets.ExecuteSprite.LoadAsset();

        var passive = newButtonObj.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener(Execute());

        var usesTextObj = Object.Instantiate(voteArea.NameText, voteArea.transform);
        usesTextObj.transform.localPosition = new Vector3(-0.22f, 0.12f, 0f);
        usesTextObj.text = $"{Executes}";
        usesTextObj.transform.localScale = usesTextObj.transform.localScale * 0.65f;

        usesText = usesTextObj;
    }

    [HideFromIl2Cpp]
    private Action Execute()
    {
        void Listener()
        {
            if (Player.HasDied()) return;

            Clear();

            Executes--;

            if (!Jailed.IsRole<PestilenceRole>())
            {
                if (Jailed.Is(ModdedRoleTeams.Crewmate))
                {
                    Executes = 0;

                    CustomButtonSingleton<JailorJailButton>.Instance.ExecutedACrew = true;

                    Coroutines.Start(MiscUtils.CoFlash(Color.red));
                }
                else
                {
                    Coroutines.Start(MiscUtils.CoFlash(Color.green));
                }

                Player.RpcCustomMurder(Jailed, createDeadBody: false, teleportMurderer: false);
            }
        }

        return Listener;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Jailor is a Crewmate Power role that can jail other players. During a meeting, the Jailor can choose to execute their jailed player."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Jail",
            "Jail a player. During the meeting everyone will see who is jailed. You can privately talk with your detained player using the instructions that are in the chatbox",
            TouCrewAssets.JailSprite),
        new("Execute (Meeting)",
            "Execute the detained player. If the player is a crewmate the Jailor will lose the ability to Jail.",
            TouAssets.ExecuteSprite)
    ];
}
