// <copyright file="Theme.cs" company="Zizhen Li">
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
    /// Color themes.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Theme
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

        [StaticConstructorOnStartup]
        public static class MilkySlicky
        {
            public static Color BackGround = GenColor.FromHex("C7AF84C0");
            public static Color ForeGround = GenColor.FromHex("383961");
            public static Color Highlight = GenColor.FromHex("5F758E");
            public static Color DarkForeGround = GenColor.FromHex("3B1F2B");
            public static Color Warning = GenColor.FromHex("DB162F");

            public static Texture2D BGTex = SolidColorMaterials.NewSolidColorTexture(BackGround);
        }

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
