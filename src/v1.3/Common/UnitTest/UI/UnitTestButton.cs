// <copyright file="UnitTestButton.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AwesomeInventory;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    [StaticConstructorOnStartup]
    public static class UnitTestButton
    {
        public static Texture2D UnitTextIcon = ContentFinder<Texture2D>.Get("UI/Icons/UnitTest");

        static UnitTestButton()
        {
            Harmony harmony = new Harmony(StringResource.HarmonyInstance);
            MethodInfo original = AccessTools.Method(typeof(DebugWindowsOpener), "DevToolStarterOnGUI");
            MethodInfo postfix = AccessTools.Method(typeof(UnitTestButton), "Draw");
            harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Draw()
        {
            if (Prefs.DevMode && AwesomeInventoryMod.Settings.ShowRestartButton)
            {
                Vector2 vector = new Vector2((float)UI.screenWidth * 0.5f - WidgetRow.IconSize, 3f);
                Find.WindowStack.ImmediateWindow(
                    typeof(UnitTestButton).GetHashCode(),
                    new Rect(vector.x, vector.y, WidgetRow.IconSize, WidgetRow.IconSize).Rounded(),
                    WindowLayer.GameUI,
                    () =>
                    {
                        WidgetRow row = new WidgetRow(WidgetRow.IconSize, 0, UIDirection.LeftThenDown);
                        if (row.ButtonIcon(UnitTextIcon, "Restart Rimworld"))
                        {
                            GenCommandLine.Restart();
                        }
                    },
                    doBackground: false,
                    absorbInputAroundWindow: false, 0f);
            }
        }
    }
}
