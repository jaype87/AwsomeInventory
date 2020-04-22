// <copyright file="StatPanelToggle.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    public class StatPanelToggle
    {
        private static float _x;
        private static float _y;

        public StatPanelToggle(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public static void Draw()
        {
            Rect rect = new Rect(_x, _y, GenUI.ListSpacing, GenUI.ListSpacing * 2);
            Find.WindowStack.ImmediateWindow(
                typeof(StatPanelToggle).GetHashCode()
                , rect
                , WindowLayer.GameUI
                , () =>
                {
                    Texture2D atlas = TexResource.ButtonBGAtlas;
                    if (Mouse.IsOver(rect))
                    {
                        atlas = TexResource.ButtonBGAtlasMouseover;
                        if (Input.GetMouseButton(0))
                        {
                            atlas = TexResource.ButtonBGAtlasClick;
                        }
                    }

                    Widgets.DrawAtlas(rect, atlas);
                    Widgets.ButtonText(rect.AtZero(), string.Empty);
                    GUI.DrawTexture(new Rect(0, 0, GenUI.SmallIconSize, GenUI.SmallIconSize), TexResource.TriangleRight);
                }
                , true
                , false);
        }
    }
}
