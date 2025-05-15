using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class TransporterRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string RoleName => "Transporter";
    public string RoleDescription => "Choose Two Players To Swap Locations";
    public string RoleLongDescription => "Choose two players to swap locations with one another";
    public Color RoleColor => TownOfUsColors.Transporter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public override bool IsAffectedByComms => false;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Transporter,
        IntroSound = TouAudio.TimeLordIntroSound,
    };

    [MethodRpc((uint)TownOfUsRpc.Transport, SendImmediately = true)]
    public static void RpcTransport(PlayerControl transporter, byte player1, byte player2)
    {
        if (transporter.Data.Role is not TransporterRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcTransport - Invalid Transporter");
            return;
        }

        var t1 = GetTarget(player1);
        var t2 = GetTarget(player2);

        //also check again incase they went on the usable while the transporter was picking but ignore vents
        if (t1 == null || t2 == null)
        {
            if (transporter.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.red));
            }
            return;
        }

        if (t1.TryCast<DeadBody>()) PreCheckUndertaker(t1.TryCast<DeadBody>()!);
        if (t2.TryCast<DeadBody>()) PreCheckUndertaker(t2.TryCast<DeadBody>()!);

        var positions = GetAdjustedPositions(t1, t2);

        Transport(t1, positions.Item2);
        Transport(t2, positions.Item1);

        HandleOtherRoles((transporter.Data.Role as TransporterRole)!, MiscUtils.PlayerById(player1)!, MiscUtils.PlayerById(player2)!);

        if (transporter.AmOwner)
        {
            var button = CustomButtonSingleton<TransporterTransportButton>.Instance;
            button.DecreaseUses();
            button.ResetCooldownAndOrEffect();
        }

        var p1 = t1.TryCast<CustomNetworkTransform>();
        var p2 = t2.TryCast<CustomNetworkTransform>();

        if (transporter.AmOwner || (p1 != null && p1.AmOwner) || (p2 != null && p2.AmOwner))
        {
            if (!transporter.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Transporter.ToTextColor()} You were transported!</color></b>", Color.white, spr: TouRoleIcons.Transporter.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
        }

        MonoBehaviour? GetTarget(byte id)
        {
            var data = GameData.Instance.GetPlayerById(id);
            if (!data)
            {
                return null;
            }

            var body = Helpers.GetBodyById(id);
            if (data.IsDead && body)
            {
                return body;
            }

            var pc = data.Object;
            if (!pc)
            {
                return null;
            }

            if (pc.moveable || pc.inVent)
            {
                if (pc.inVent)
                {
                    pc.MyPhysics.ExitAllVents();
                }

                return pc;
            }

            return null;
        }

        void Transport(MonoBehaviour mono, Vector3 position)
        {
            if (mono.TryCast<PlayerControl>() is PlayerControl player && player.HasModifier<ImmovableModifier>())
            {
                return;
            }

            if (mono.TryCast<DeadBody>() is DeadBody deadBody && MiscUtils.PlayerById(deadBody.ParentId)?.HasModifier<ImmovableModifier>() == true)
            {
                return;
            }

            var cnt = mono.TryCast<CustomNetworkTransform>();
            if (cnt != null)
            {
                cnt.SnapTo(position, (ushort)(cnt.lastSequenceId + 1));

                if (cnt.AmOwner)
                {
                    try
                    {
                        Minigame.Instance.Close();
                        Minigame.Instance.Close();
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }
            else
            {
                mono.transform.position = position;
            }
        }

        void HandleOtherRoles(TransporterRole transporter, PlayerControl player1, PlayerControl player2)
        {
            // TODO: medic, guardian angel, pestilence, veteran, shy?, cleric
            LookoutEvents.CheckForLookoutWatched(transporter.Player, player1);
            LookoutEvents.CheckForLookoutWatched(transporter.Player, player2);

            var warden = player1.GetModifier<WardenFortifiedModifier>()?.Warden.GetRole<WardenRole>();
            if (warden != null && transporter.Player.AmOwner)
            {
                WardenRole.RpcWardenNotify(warden.Player, transporter.Player, player1);
            }

            var warden2 = player2.GetModifier<WardenFortifiedModifier>()?.Warden.GetRole<WardenRole>();
            if (warden2 != null && transporter.Player.AmOwner)
            {
                WardenRole.RpcWardenNotify(warden2.Player, transporter.Player, player2);
            }

            var mercenary = PlayerControl.LocalPlayer.Data.Role as MercenaryRole;
            if (player1.HasModifier<MercenaryGuardModifier>() || player2.HasModifier<MercenaryGuardModifier>() && mercenary)
            {
                mercenary!.AddPayment();
            }

            var infectedtrans = transporter.Player.GetModifier<PlaguebearerInfectedModifier>();
            var infectedplayer1 = player1.GetModifier<PlaguebearerInfectedModifier>();
            var infectedplayer2 = player2.GetModifier<PlaguebearerInfectedModifier>();
            if (infectedtrans != null)
            {
                if (infectedplayer1 == null) player1.AddModifier<PlaguebearerInfectedModifier>(infectedtrans.PlagueBearerId);
                if (infectedplayer2 == null) player2.AddModifier<PlaguebearerInfectedModifier>(infectedtrans.PlagueBearerId);
            }
            else if (infectedtrans == null && infectedplayer1 != null)
            {
                transporter.Player.AddModifier<PlaguebearerInfectedModifier>(infectedplayer1.PlagueBearerId);
            }
            else if (infectedtrans == null && infectedplayer2 != null)
            {
                transporter.Player.AddModifier<PlaguebearerInfectedModifier>(infectedplayer2.PlagueBearerId);
            }
        }

        void PreCheckUndertaker(DeadBody body)
        {
            if (PlayerControl.LocalPlayer.Data.Role is not TransporterRole)
            {
                return;
            }

            var mods = ModifierUtils.GetActiveModifiers<DragModifier>();

            foreach (var mod in mods)
            {
                if (mod.BodyId == body.ParentId)
                {
                    UndertakerRole.RpcStopDragging(mod.Player, body.transform.position);
                }
            }
        }

        (Vector2, Vector2) GetAdjustedPositions(MonoBehaviour transportable, MonoBehaviour transportable2)
        {
            // assign dummy values so it doesnt error about returning unassigned variables
            Vector2 TP1Position = new(0, 0);
            Vector2 TP2Position = new(0, 0);

            if (transportable.TryCast<DeadBody>() == null && transportable2.TryCast<DeadBody>() == null)
            {
                Logger<TownOfUsPlugin>.Error($"type: {transportable.GetIl2CppType().Name}");
                var TP1 = transportable.TryCast<PlayerControl>()!;
                TP1Position = TP1.GetTruePosition();
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var TP2 = transportable2.TryCast<PlayerControl>()!;
                TP2Position = TP2.GetTruePosition();
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                if (TP1.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y - 0.2233912f * 0.75f);
                }
                else if (TP2.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y - 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() != null && transportable2.TryCast<DeadBody>() == null)
            {
                var Player1Body = transportable.TryCast<DeadBody>()!;
                TP1Position = Player1Body.TruePosition;
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var TP2 = transportable2.TryCast<PlayerControl>()!;
                TP2Position = TP2.GetTruePosition();
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                if (TP2.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y - 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() == null && transportable2.TryCast<DeadBody>() != null)
            {
                var TP1 = transportable.TryCast<PlayerControl>()!;
                TP1Position = TP1.GetTruePosition();
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var Player2Body = transportable2.TryCast<DeadBody>()!;
                TP2Position = Player2Body.TruePosition;
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);
                if (TP1.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y - 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() != null && transportable2.TryCast<DeadBody>() != null)
            {
                TP1Position = transportable.TryCast<DeadBody>()!.TruePosition;
                TP2Position = transportable2.TryCast<DeadBody>()!.TruePosition;
            }

            return (TP1Position, TP2Position);
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    public string GetAdvancedDescription()
    {
        return
            "The Transporter is a Crewmate Support role that can transport two players, dead or alive, to swap their locations."
               + MiscUtils.AppendOptionsText(GetType());
    }
    
    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Transport",
            "Switch the positions of two players. Players can be transported out of vents." +
            "A red flash means one of the players became an invalid target," +
            "such as going on a ladder or zipline",
            TouCrewAssets.Transport),
    ];
}


