// <copyright file="StatPanelToggle.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    public class StatPanelToggle
    {
        private float _x;
        private float _y;

        public StatPanel StatPanel { get; } = new StatPanel();

        public void SetPosition(Vector2 position)
        {
            (_x, _y) = (position.x, position.y);
            this.StatPanel.SetPosition(position);
        }

        public void Draw()
        {
            Rect rect;
            if (StatPanel.IsOpen)
            {
                if (!Find.WindowStack.IsOpen(typeof(StatPanel)))
                    Find.WindowStack.Add(this.StatPanel);

                rect = new Rect(this.StatPanel.windowRect.xMax, this.StatPanel.windowRect.y, GenUI.ListSpacing, GenUI.ListSpacing * 2);
                Find.WindowStack.ImmediateWindow(
                    typeof(StatPanelToggle).GetHashCode()
                    , rect
                    , WindowLayer.GameUI
                    , () =>
                    {
                        if (Widgets.ButtonImageWithBG(
                            rect.AtZero()
                            , TexResource.TriangleLeft
                            , new Vector2(GenUI.SmallIconSize, GenUI.SmallIconSize * 1.5f)))
                        {
                            Find.WindowStack.TryRemove(this.StatPanel);
                            StatPanel.IsOpen = false;
                        }
                    }
                    , true
                    , false);
                return;
            }

            rect = new Rect(_x, _y, GenUI.ListSpacing, GenUI.ListSpacing * 2);
            Find.WindowStack.ImmediateWindow(
                typeof(StatPanelToggle).GetHashCode()
                , rect
                , WindowLayer.GameUI
                , () =>
                {
                    if (Widgets.ButtonImageWithBG(
                        rect.AtZero()
                        , TexResource.TriangleRight
                        , new Vector2(GenUI.SmallIconSize, GenUI.SmallIconSize * 1.5f)))
                    {
                        Find.WindowStack.Add(this.StatPanel);
                        StatPanel.IsOpen = true;
                    }
                }
                , true
                , false);
        }
    }
}
