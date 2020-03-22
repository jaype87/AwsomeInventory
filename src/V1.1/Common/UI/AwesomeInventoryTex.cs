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

        public static readonly Color Valvet = GenColor.FromHex("cc1a00");
        public static readonly Color LightGrey = GenColor.FromHex("b0b3af");
        public static readonly Color RWPrimaryColor = GenColor.FromHex("6a512e");

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
        public static readonly Texture RMPrimaryTex = SolidColorMaterials.NewSolidColorTexture(RWPrimaryColor);
        public static readonly Texture LeatherTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Leather);

        public static readonly Texture Lengendary = OrangeTex;
        public static readonly Texture Masterwork = SandTex;
        public static readonly Texture Excellent = CyanTex;
        public static readonly Texture Good = LightGreenTex;
        public static readonly Texture Normal = BaseContent.WhiteTex;
        public static readonly Texture Poor = LightGreyTex;
        public static readonly Texture Awful = ValvetTex;

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
