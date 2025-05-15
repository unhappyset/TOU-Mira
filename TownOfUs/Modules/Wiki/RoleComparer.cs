namespace TownOfUs.Modules.Wiki;

public class RoleComparer(ushort currentRole) : IComparer<RoleBehaviour>
{
    public int Compare(RoleBehaviour? x, RoleBehaviour? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        if ((ushort)x.Role == currentRole) return -1;
        if ((ushort)y.Role == currentRole) return 1;

        return string.Compare(x.NiceName, y.NiceName, StringComparison.OrdinalIgnoreCase);
    }
}