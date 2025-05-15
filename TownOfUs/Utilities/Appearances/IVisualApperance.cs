namespace TownOfUs.Utilities.Appearances;

public interface IVisualAppearance
{
    VisualAppearance? GetVisualAppearance();
    bool VisualPriority => false;
}
