using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Modifiers;

public static class CelebrityEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (target.HasModifier<CelebrityModifier>())
        {
            PlainShipRoom[] allRooms = LobbyBehaviour.Instance.AllRooms;
            PlainShipRoom[] array = allRooms;
            if (ShipStatus.Instance)
            {
                array = ShipStatus.Instance.AllRooms;
            }

            PlainShipRoom? plainShipRoom = null;
            if (array != null)
                foreach (PlainShipRoom plainShipRoom2 in array)
                {
                    if (plainShipRoom2.roomArea && plainShipRoom2.roomArea.OverlapPoint(target.GetTruePosition()))
                    {
                        plainShipRoom = plainShipRoom2;
                    }
                }
            else
            {
                var allRooms2 = ShipStatus.Instance.FastRooms;
                foreach (PlainShipRoom plainShipRoom2 in allRooms2.Values)
                {
                    if (plainShipRoom2.roomArea && plainShipRoom2.roomArea.OverlapPoint(target.GetTruePosition()))
                    {
                        plainShipRoom = plainShipRoom2;
                    }
                }
            }
            var roomStr = plainShipRoom != null ? TranslationController.Instance.GetString(plainShipRoom.RoomId) : "Outside/Hallway";

            CelebrityModifier.CelebrityKilled(source, target, roomStr);
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Reporter.AmOwner)
        {
            var celebrity = ModifierUtils.GetActiveModifiers<CelebrityModifier>(x => x.Player.HasDied() && !x.Announced).FirstOrDefault();
            if (celebrity != null)
            {
                var milliSeconds = (float)(DateTime.UtcNow - celebrity.DeathTime).TotalMilliseconds;

                CelebrityModifier.RpcUpdateCelebrityKilled(celebrity.Player, milliSeconds);
            }
        }
    }
    [RegisterEvent]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var player = @event.ExileController.initData.networkedPlayer?.Object;
        if (player == null) return;
        if (player.TryGetModifier<CelebrityModifier>(out var celeb))
        {
            celeb.Announced = true;
        }
    }
}
