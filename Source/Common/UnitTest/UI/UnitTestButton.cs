using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    [StaticConstructorOnStartup]
    public static class UnitTestButton
    {
        public static Texture2D UnitTextIcon = ContentFinder<Texture2D>.Get("UI/Icons/UnitTest");

        static UnitTestButton()
        {
            var harmony = HarmonyInstance.Create(StringResource.HarmonyInstance);
            MethodInfo original = AccessTools.Method(typeof(DebugWindowsOpener), "DevToolStarterOnGUI");
            MethodInfo postfix = AccessTools.Method(typeof(UnitTestButton), "Draw");
            harmony.Patch(original, null, new HarmonyMethod(postfix));
        }


        public static void Draw()
        {
            if (Prefs.DevMode)
            {
                Vector2 vector = new Vector2((float)UI.screenWidth * 0.5f - WidgetRow.IconSize, 3f);
                Find.WindowStack.ImmediateWindow(typeof(UnitTestButton).GetHashCode(), new Rect(vector.x, vector.y, WidgetRow.IconSize, WidgetRow.IconSize).Rounded(), WindowLayer.GameUI, delegate
                {
                    WidgetRow row = new WidgetRow(WidgetRow.IconSize, 0, UIDirection.LeftThenDown);
                    if (row.ButtonIcon(UnitTextIcon, "Restart Rimworld"))
                    {
                        GenCommandLine.Restart();
                    }
                }, doBackground: false, absorbInputAroundWindow: false, 0f);
            }
        }
    }
}
