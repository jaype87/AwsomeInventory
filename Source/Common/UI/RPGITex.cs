using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace RPG_Inventory_Remake_Common
{
    public static class RPGITex
    {
        public static readonly Texture OrangeTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Orange);
        public static readonly Texture CyanTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Cyan);
        public static readonly Texture BrightPurpleTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.BrightPurple);
        public static readonly Texture BlueTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Blue);
        public static readonly Texture LightGreenTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.LightGreen);
        public static readonly Texture SandTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Sand);
        public static readonly Texture SalmonTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.Salmon);
        public static readonly Texture BrickRedTex = SolidColorMaterials.NewSolidColorTexture(ColorLibrary.BrickRed);

        public static readonly Texture Lengendary = OrangeTex;
        public static readonly Texture Masterwork = CyanTex;
        public static readonly Texture Excellent = BrightPurpleTex;
        public static readonly Texture Good = LightGreenTex;
        public static readonly Texture Normal = BlueTex;
        public static readonly Texture Poor = SandTex;
        public static readonly Texture Awful = BrickRedTex;
    }
}
