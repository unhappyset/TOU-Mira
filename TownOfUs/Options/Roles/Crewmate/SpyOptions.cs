using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SpyOptions : AbstractOptionGroup<SpyRole>
{
    public override string GroupName => "Spy";

    [ModdedEnumOption("Who Sees Dead Bodies On Admin", typeof(AdminDeadPlayers), ["Nobody", "Spy", "Everyone But Spy", "Everyone"])]
    public AdminDeadPlayers WhoSeesDead { get; set; } = AdminDeadPlayers.Nobody;
}

public enum AdminDeadPlayers
{
    Nobody,
    Spy,
    EveryoneButSpy,
    Everyone,
}
