using UnityEngine;

namespace TownOfUs;

public static class TownOfUsColors
{
    // Crew Colors
    public static Color Crewmate => Palette.CrewmateRoleBlue;
    public static Color Mayor => new(0.44f, 0.31f, 0.66f, 1f);
    public static Color Sheriff => Color.yellow;
    public static Color Engineer => new(1f, 0.65f, 0.04f, 1f);
    public static Color Swapper => new(0.4f, 0.9f, 0.4f, 1f);
    public static Color Investigator => new(0f, 0.7f, 0.7f, 1f);
    public static Color Medic => new(0f, 0.4f, 0f, 1f);
    public static Color Seer => new(1f, 0.8f, 0.5f, 1f);
    public static Color Spy => new(0.8f, 0.64f, 0.8f, 1f);
    public static Color Snitch => new(0.83f, 0.69f, 0.22f, 1f);
    public static Color Altruist => new(0.4f, 0f, 0f, 1f);
    public static Color Vigilante => new(1f, 1f, 0.6f, 1f);
    public static Color Veteran => new(0.6f, 0.5f, 0.25f, 1f);
    public static Color Haunter => new(0.83f, 0.83f, 0.83f, 1f);
    public static Color Transporter => new(0f, 0.93f, 1f, 1f);
    public static Color Medium => new(0.65f, 0.5f, 1f, 1f);
    public static Color Mystic => new(0.3f, 0.6f, 0.9f, 1f);
    public static Color Trapper => new(0.65f, 0.82f, 0.7f, 1f);
    public static Color Detective => new(0.3f, 0.3f, 1f, 1f);
    public static Color Imitator => new(0.7f, 0.85f, 0.3f, 1f);
    public static Color Prosecutor => new(0.7f, 0.5f, 0f, 1f);
    public static Color Oracle => new(0.75f, 0f, 0.75f, 1f);
    public static Color Aurial => new(0.7f, 0.3f, 0.6f, 1f);
    public static Color Politician => new(0.4f, 0f, 0.6f, 1f);
    public static Color Warden => new(0.6f, 0f, 1f, 1f);
    public static Color Jailor => new(0.65f, 0.65f, 0.65f, 1f);
    public static Color Hunter => new(0.16f, 0.67f, 0.53f, 1f);
    public static Color Tracker => new(0f, 0.6f, 0f, 1f);
    public static Color Lookout => new(0.2f, 1f, 0.4f, 1f);
    public static Color Deputy => new(1f, 0.8f, 0f, 1f);
    public static Color Plumber => new(0.8f, 0.4f, 0f, 1f);
    public static Color Cleric => new(0f, 1f, 0.7f, 1f);

    // Neutral Colors
    public static Color Neutral => Color.gray;
    public static Color Jester => new(1f, 0.75f, 0.8f, 1f);
    public static Color Executioner => new(0.55f, 0.25f, 0.02f, 1f);
    public static Color Glitch => Color.green;
    public static Color Arsonist => new(1f, 0.3f, 0f);
    public static Color Amnesiac => new(0.5f, 0.7f, 1f, 1f);
    public static Color Juggernaut => new(0.55f, 0f, 0.3f, 1f);
    public static Color Survivor => new(1f, 0.9f, 0.3f, 1f);
    public static Color Protector => new(0.7f, 1f, 1f, 1f);
    public static Color Plaguebearer => new(0.9f, 1f, 0.7f, 1f);
    public static Color Pestilence => new(0.3f, 0.3f, 0.3f, 1f);
    public static Color Werewolf => new(0.66f, 0.4f, 0.16f, 1f);
    public static Color Doomsayer => new(0f, 1f, 0.5f, 1f);
    public static Color Vampire => new(0.15f, 0.15f, 0.15f, 1f);
    public static Color SoulCollector => new(0.6f, 1f, 0.8f, 1f);
    public static Color GuardianAngel => new(0.7f, 1f, 1f, 1f);
    public static Color Phantom => new(0.4f, 0.16f, 0.38f, 1f);
    public static Color Mercenary => new(0.55f, 0.4f, 0.6f, 1f);

    // Impostor Colors
    public static Color Impostor => Palette.ImpostorRed;
    public static Color ImpSoft => new(0.84f, 0.25f, 0.26f, 1f);

    // Modifiers
    public static Color Bait => new(0.2f, 0.7f, 0.7f, 1f);
    public static Color Aftermath => new(0.65f, 1f, 0.65f, 1f);
    public static Color Diseased => Color.grey;
    public static Color Torch => new(1f, 1f, 0.6f, 1f);
    public static Color ButtonBarry => new(0.7f, 0.2f, 0.8f, 1f);
    public static Color Flash => new(1f, 0.5f, 0.5f, 1f);
    public static Color Giant => new(1f, 0.7f, 0.3f, 1f);
    public static Color Lover => new(1f, 0.4f, 0.8f, 1f);
    public static Color Sleuth => new(0.5f, 0.2f, 0.2f, 1f);
    public static Color Tiebreaker => new(0.6f, 0.9f, 0.6f, 1f);
    public static Color Radar => new(1f, 0f, 0.5f, 1f);
    public static Color Multitasker => new(1f, 0.5f, 0.3f, 1f);
    public static Color Frosty => new(0.6f, 1f, 1f, 1f);
    public static Color SixthSense => new(0.85f, 1f, 0.55f, 1f);
    public static Color Shy => new(1f, 0.7f, 0.8f, 1f);
    public static Color Mini => new(0.8f, 1f, 0.9f, 1f);
    public static Color Camouflaged => Color.gray;
    public static Color Satellite => new(0f, 0.6f, 0.8f, 1f);
    public static Color Taskmaster => new(0.4f, 0.6f, 0.4f, 1f);
    public static Color Celebrity => new(1f, 0.6f, 0.6f, 1f);
    public static Color Immovable => new(0.9f, 0.9f, 0.8f, 1f);

}
