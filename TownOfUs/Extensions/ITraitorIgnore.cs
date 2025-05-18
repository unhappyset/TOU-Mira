namespace TownOfUs.Extensions;

public interface ITraitorIgnore
{
    // Used incase a role shouldn't be pickable by the Traitor
    bool IsIgnored { get; }
}
