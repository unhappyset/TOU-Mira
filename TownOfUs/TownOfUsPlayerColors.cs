﻿using MiraAPI.Colors;
using UnityEngine;

namespace TownOfUs;

[RegisterCustomColors]
public static class TownOfUsPlayerColors
{
    public static CustomColor Watermelon { get; } = new("Watermelon",
        new Color32(168, 50, 62, byte.MaxValue),
        new Color32(101, 30, 37, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Chocolate { get; } = new("Chocolate",
        new Color32(60, 48, 44, byte.MaxValue),
        new Color32(30, 24, 22, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor SkyBlue { get; } = new("SkyBlue",
        new Color32(61, 129, 255, byte.MaxValue),
        new Color32(31, 65, 128, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Beige { get; } = new("Beige",
        new Color32(240, 211, 165, byte.MaxValue),
        new Color32(120, 106, 83, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Magenta { get; } = new("Magenta",
        new Color32(255, 0, 127, byte.MaxValue),
        new Color32(191, 0, 95, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor SeaGreen { get; } = new("SeaGreen",
        new Color32(61, 255, 181, byte.MaxValue),
        new Color32(31, 128, 91, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Lilac { get; } = new("Lilac",
        new Color32(186, 161, 255, byte.MaxValue),
        new Color32(93, 81, 128, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Olive { get; } = new("Olive",
        new Color32(97, 114, 24, byte.MaxValue),
        new Color32(66, 91, 15, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Azure { get; } = new("Azure",
        new Color32(1, 166, 255, byte.MaxValue),
        new Color32(17, 104, 151, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Plum { get; } = new("Plum",
        new Color32(79, 0, 127, byte.MaxValue),
        new Color32(55, 0, 95, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Jungle { get; } = new("Jungle",
        new Color32(0, 47, 0, byte.MaxValue),
        new Color32(0, 23, 0, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Mint { get; } = new("Mint",
        new Color32(151, 255, 151, byte.MaxValue),
        new Color32(109, 191, 109, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Chartreuse { get; } = new("Chartreuse",
        new Color32(207, 255, 0, byte.MaxValue),
        new Color32(143, 191, 61, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Macau { get; } = new("Macau",
        new Color32(0, 97, 93, byte.MaxValue),
        new Color32(0, 65, 61, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Tawny { get; } = new("Tawny",
        new Color32(205, 63, 0, byte.MaxValue),
        new Color32(141, 31, 0, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Gold { get; } = new("Gold",
        new Color32(255, 207, 0, byte.MaxValue),
        new Color32(191, 143, 0, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Snow { get; } = new("Snow",
        new Color32(255, 255, 255, byte.MaxValue),
        new Color32(163, 194, 223, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Turquoise { get; } = new("Turquoise",
        new Color32(31, 164, 159, byte.MaxValue),
        new Color32(4, 102, 141, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Nacho { get; } = new("Nacho",
        new Color32(242, 166, 38, byte.MaxValue),
        new Color32(185, 87, 25, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Blood { get; } = new("Blood",
        new Color32(110, 0, 21, byte.MaxValue),
        new Color32(61, 0, 46, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Grass { get; } = new("Grass",
        new Color32(59, 130, 90, byte.MaxValue),
        new Color32(9, 86, 73, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Mandarin { get; } = new("Mandarin",
        new Color32(255, 149, 79, byte.MaxValue),
        new Color32(230, 52, 76, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Glass { get; } = new("Glass",
        new Color32(149, 202, 220, byte.MaxValue),
        new Color32(79, 125, 161, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Ash { get; } = new("Ash",
        new Color32(11, 14, 19, byte.MaxValue),
        new Color32(4, 5, 7, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Midnight { get; } = new("Midnight",
        new Color32(16, 46, 104, byte.MaxValue),
        new Color32(8, 27, 65, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Steel { get; } = new("Steel",
        new Color32(93, 97, 118, byte.MaxValue),
        new Color32(59, 60, 81, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Silver { get; } = new("Silver",
        new Color32(203, 220, 219, byte.MaxValue),
        new Color32(105, 125, 121, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Shimmer { get; } = new("Shimmer",
        new Color32(54, 252, 169, byte.MaxValue),
        new Color32(30, 189, 191, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Crimson { get; } = new("Crimson",
        new Color32(174, 29, 74, byte.MaxValue),
        new Color32(107, 22, 72, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Charcoal { get; } = new("Charcoal",
        new Color32(50, 48, 78, byte.MaxValue),
        new Color32(12, 15, 46, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Violet { get; } = new("Violet",
        new Color32(128, 6, 178, byte.MaxValue),
        new Color32(78, 16, 145, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor Denim { get; } = new("Denim",
        new Color32(54, 47, 188, byte.MaxValue),
        new Color32(21, 21, 129, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Darker
    };

    public static CustomColor CottonCandy { get; } = new("CottonCandy",
        new Color32(255, 141, 189, byte.MaxValue),
        new Color32(241, 68, 166, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };

    public static CustomColor Rainbow { get; } = new("Rainbow",
        new Color32(0, 0, 0, byte.MaxValue))
    {
        ColorBrightness = CustomColorBrightness.Lighter
    };
}
