// <copyright file="AwesomeInventoryTex.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Texture resources for Awesome Inventory.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class AwesomeInventoryTex
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

        /************
         *  Colors  *
         ************/
        public static readonly Color Lavendar = GenColor.FromHex("dda0dd");
        public static readonly Color LightGrey = GenColor.FromHex("b0b3af");
        public static readonly Color Valvet = GenColor.FromHex("cc1a00");
        public static readonly Color HighlightGreen = new Color(134 / 255f, 206 / 255f, 0, 1);
        public static readonly Color HighlightBrown = new Color(212 / 255f, 141 / 255f, 0, 1);
        public static readonly Color RWPrimaryColor = GenColor.FromHex("6a512e");

        // For Lighter Colorful Traits
        public static readonly Color LCT_MediumPurple = GenColor.FromHex("9370DB");
        public static readonly Color LCT_Olivine = GenColor.FromHex("9AB973");
        public static readonly Color LCT_CanaryYellow = GenColor.FromHex("FFFF99");
        public static readonly Color LCT_AtomicTangerine = GenColor.FromHex("FF9966");
        public static readonly Color LCT_PantoneRed = GenColor.FromHex("ED2939");

        // For Color Coded Mood Bar
        public static readonly float CCMB_Alpha = 0.44f;
        public static readonly Color CCMB_HappyColor = new Color(0.1f, 0.75f, 0.2f, CCMB_Alpha);
        public static readonly Color CCMB_Cyan = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, CCMB_Alpha);
        public static readonly Color CCMB_NeutralColor = new Color(0.87f, 0.96f, 0.79f, CCMB_Alpha);
        public static readonly Color CCMB_Yellow = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, CCMB_Alpha);
        public static readonly Color CCMB_Orange = new Color(1f, 0.5f, 0.31f, CCMB_Alpha);
        public static readonly Color CCMB_Red = new Color(Color.red.r, Color.red.g, Color.red.b, CCMB_Alpha);

        /************
         * Textures *
         ************/
        public static readonly Texture OrangeTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Orange);
        public static readonly Texture CyanTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Cyan);
        public static readonly Texture BrightPurpleTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.BrightPurple);
        public static readonly Texture BlueTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Blue);
        public static readonly Texture LightGreenTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.LightGreen);
        public static readonly Texture SandTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Sand);
        public static readonly Texture SalmonTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Salmon);
        public static readonly Texture BrickRedTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.BrickRed);
        public static readonly Texture LightBlueTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.LightBlue);
        public static readonly Texture RoyalBlueTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.RoyalBlue);
        public static readonly Texture ValvetTex = SolidColorMaterials.NewSolidColorTexture(Valvet);
        public static readonly Texture LightGreyTex = SolidColorMaterials.NewSolidColorTexture(LightGrey);
        public static readonly Texture RWPrimaryTex = SolidColorMaterials.NewSolidColorTexture(RWPrimaryColor);
        public static readonly Texture LeatherTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Leather);

        public static readonly Texture Lengendary = OrangeTex;
        public static readonly Texture Masterwork = SandTex;
        public static readonly Texture Excellent = CyanTex;
        public static readonly Texture Good = LightGreenTex;
        public static readonly Texture Normal = BaseContent.WhiteTex;
        public static readonly Texture Poor = LightGreyTex;
        public static readonly Texture Awful = ValvetTex;

        // For Lighter Colorful Traits
        public static readonly Texture LCT_Lengendary = SolidColorMaterials.NewSolidColorTexture(LCT_MediumPurple);
        public static readonly Texture LCT_Masterwork = CyanTex; // TODO: possibly tweak this?
        public static readonly Texture LCT_Excellent = SolidColorMaterials.NewSolidColorTexture(LCT_Olivine);
        public static readonly Texture LCT_Good = BaseContent.WhiteTex;
        public static readonly Texture LCT_Normal = SolidColorMaterials.NewSolidColorTexture(LCT_CanaryYellow);
        public static readonly Texture LCT_Poor = SolidColorMaterials.NewSolidColorTexture(LCT_AtomicTangerine);
        public static readonly Texture LCT_Awful = SolidColorMaterials.NewSolidColorTexture(LCT_PantoneRed);

        // For Color Coded Mood Bar
        public static readonly Texture CCMB_Lengendary = BrightPurpleTex; // TODO: possibly tweak this?
        public static readonly Texture CCMB_Masterwork = SolidColorMaterials.NewSolidColorTexture(CCMB_HappyColor);
        public static readonly Texture CCMB_Excellent = SolidColorMaterials.NewSolidColorTexture(CCMB_Cyan);
        public static readonly Texture CCMB_Good = SolidColorMaterials.NewSolidColorTexture(CCMB_NeutralColor);
        public static readonly Texture CCMB_Normal = SolidColorMaterials.NewSolidColorTexture(CCMB_Yellow);
        public static readonly Texture CCMB_Poor = SolidColorMaterials.NewSolidColorTexture(CCMB_Orange);
        public static readonly Texture CCMB_Awful = SolidColorMaterials.NewSolidColorTexture(CCMB_Red);

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
