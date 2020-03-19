// <copyright file="Utility.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RPG_Inventory_Remake_Common;
using RPGIResource;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AwesomeInventory.Utilities
{
    public static class Utility
    {
        public const float StandardLineHeight = 22f;
        public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

        /// <summary>
        /// Gets a harmony instance.
        /// </summary>
        public static Harmony Harmony { get; } = new Harmony("NotoShabby.rimworld.mod.RPGInventoryRemake");

        /****************************************************************************
         *  List order used by AwesomeInventory to display items.
         *
         *  UpperHead = 200, FullHead = 199, Eyes = 198, Teeth = 197, Mouth = 196,
         *  Neck = 180,
         *  Torso = 100, Arms = 90, LeftArm = 89, RightArm = 88,
         *  Shoulders = 85, LeftShoulder = 84, RightShoulder = 83,
         *  Hands = 80, LeftHand = 79, RightHand = 78,
         *  Waist = 50,
         *  Legs = 10,
         *  Feet = 9
         *
        *****************************************************************************/

        public static bool ShouldShowOverallArmor(Pawn p)
        {
            return p.RaceProps.Humanlike || ShouldShowApparel(p) || p.GetStatValue(StatDefOf.ArmorRating_Sharp, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Blunt, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Heat, true) > 0f;
        }

        public static bool ShouldShowInventory(Pawn p)
        {
            return p.RaceProps.Humanlike || p.inventory.innerContainer.Any;
        }

        public static bool ShouldShowApparel(Pawn p)
        {
            return p.apparel != null && (p.RaceProps.Humanlike || p.apparel.WornApparel.Any<Apparel>());
        }

        public static bool ShouldShowEquipment(Pawn p)
        {
            return p.equipment != null;
        }

        public static void TryDrawComfyTemperatureRange(Pawn selPawn, ref float curY, float width)
        {
            if (selPawn.Dead)
            {
                return;
            }
            Rect rect = new Rect(0f, curY, width, 22f);
            float statValue = selPawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
            float statValue2 = selPawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
            Widgets.Label(rect, string.Concat(new string[]
            {
                "ComfyTemperatureRange".Translate(),
                ": ",
                statValue.ToStringTemperature("F0"),
                "~",
                statValue2.ToStringTemperature("F0")
            }));
            curY += 22f;
        }

        /// <summary>
        ///     Draw overall armor in a format as "Sharp   100 Rha", wihout the double quotes
        /// </summary>
        /// <param name="selPawn"></param>
        /// <param name="curY">current Y position</param>
        /// <param name="width"></param>
        /// <param name="stat"></param>
        /// <param name="label"></param>
        /// <param name="unit"></param>
        public static void TryDrawOverallArmor(Pawn selPawn, ref float curY, float width, StatDef stat, string label, string unit)
        {
            Rect rect = new Rect(0f, curY, width, StandardLineHeight);
            string toolTipText = "";
            float armorRating = CalculateArmorByParts(selPawn, stat, out toolTipText);
            if (!toolTipText.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, toolTipText);
            }
            DrawArmorRatingWithText(rect, label, armorRating, unit);
            curY += StandardLineHeight;
        }

        /// <summary>
        ///     Draw overall armor for the jealous tab
        /// </summary>
        /// <param name="selPawn"></param>
        /// <param name="rect"></param>
        /// <param name="stat"></param>
        /// <param name="label"></param>
        /// <param name="image"></param>
        /// <param name="unit"></param>
        public static void TryDrawOverallArmorCE(Pawn selPawn, Rect rect, StatDef stat, string label, Texture image, string unit)
        {
            string text = "";
            float armorRating = CalculateArmorByParts(selPawn, stat, out text);
            Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
            GUI.DrawTexture(rect1, image);
            TooltipHandler.TipRegion(rect1, label);
            Rect rect2 = new Rect(rect.x + 60f, rect.y + 3f, 104f, 24f);
            if (text.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect2, text);
            }
            Widgets.Label(rect2, armorRating.ToStringPercent());
        }

        private static string FormatArmorValue(float value, string unit)
        {
            var asPercent = unit.Equals("%", StringComparison.InvariantCulture);
            if (asPercent)
            {
                value *= 100f;
            }
            return value.ToStringByStyle(asPercent ? ToStringStyle.FloatMaxOne : ToStringStyle.FloatMaxTwo) + unit;
        }

        public static void TryDrawComfyTemperatureRange(Pawn pawn, Rect rect)
        {
            if (pawn.Dead)
            {
                return;
            }
            Rect rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/minumun_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
            Rect rect2 = new Rect(rect.x + 30f, rect.y + 28f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue.ToStringTemperature("F0")
            }));

            rect1 = new Rect(rect.x, rect.y + 52f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/max_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue2 = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
            rect2 = new Rect(rect.x + 30f, rect.y + 56f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue2.ToStringTemperature("F0")
            }));

        }

        /// <summary>
        /// Not a very effective way to get armor number, but one with most compatibility, only if modders add
        /// the "ListOrder" tag and correct value to their new "BodyPartGroup" element in xml.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="stat">Sharp, blunt or heat, maybe electrical?</param>
        /// <param name="text">Text for tooltip</param>
        /// <param name="unit">unit type for the corresponding stat</param>
        /// <returns></returns>
        public static float CalculateArmorByParts(Pawn pawn, StatDef stat, out string text)
        {
            text = "";
            float effectiveArmor = 0;
            // Max amor rating is at 200% in vanilla, divide it by 2 is for scaling manipulation purpose
            // the final value will be multiplied by 2 to restore the original scaling.
            float natureArmor = Mathf.Clamp01(pawn.GetStatValue(stat, true) / 2f);
            List<BodyPartRecord> allParts = pawn.RaceProps.body.AllParts;
            List<Apparel> apparelList = (pawn.apparel == null) ? null : pawn.apparel.WornApparel;
            if (apparelList != null)
            {
                for (int i = 0; i < allParts.Count; i++)
                {
                    float effectivePen = 1 - natureArmor;
                    for (int j = 0; j < apparelList.Count; j++)
                    {
                        if (apparelList[j].def.apparel.CoversBodyPart(allParts[i]))
                        {
                            effectivePen *= 1 - Mathf.Clamp01(apparelList[j].GetStatValue(stat, true) / 2f);
                        }
                    }
                    float eArmorForPart = allParts[i].coverageAbs * (1 - effectivePen);
                    if (allParts[i].depth == BodyPartDepth.Outside &&
                                            (allParts[i].coverage >= 0.1 ||
                                             allParts[i].def == BodyPartDefOf.Eye ||
                                             allParts[i].def == BodyPartDefOf.Neck))
                    {
                        text += allParts[i].LabelCap + ": ";
                        text += ((1 - effectivePen) * 2).ToStringPercent() + "\n";
                    }
                    effectiveArmor += eArmorForPart;
                }
            }
            effectiveArmor = Mathf.Clamp(effectiveArmor * 2, 0, 2);

            return effectiveArmor;
        }

        public static float CalculateArmorByPartsCE(Pawn pawn, StatDef stat, ref string text, string unit)
        {
            text = "";
            float num = 0f;
            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                num += wornApparel[i].GetStatValue(stat, true) * wornApparel[i].def.apparel.HumanBodyCoverage;
            }
            if (num > 0.005f)
            {
                List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
                for (int i = 0; i < bpList.Count; i++)
                {
                    float armorValue = 0f;
                    BodyPartRecord part = bpList[i];
                    if (part.depth == BodyPartDepth.Outside && (part.coverage >= 0.1 || (part.def == BodyPartDefOf.Eye || part.def == BodyPartDefOf.Neck)))
                    {
                        text += part.LabelCap + ": ";
                        for (int j = wornApparel.Count - 1; j >= 0; j--)
                        {
                            Apparel apparel = wornApparel[j];
                            if (apparel.def.apparel.CoversBodyPart(part))
                            {
                                armorValue += apparel.GetStatValue(stat, true);
                            }
                        }
                        text += formatArmorValue(armorValue, unit) + "\n";
                    }
                }
            }
            return num;
        }

        public static void DrawArmorRatingWithText(Rect pos, string label, float armorRating, string unit)
        {
            // Draw Label
            Widgets.Label(pos, label.Truncate(200f, null));
            // Draw armor value
            pos.x += 200;
            Widgets.Label(pos, FormatArmorValue(armorRating, unit));
        }

        public static void TryDrawMassInfo(Pawn pawn, ref float curY, float width)
        {
            if (pawn.Dead || !ShouldShowInventory(pawn))
            {
                return;
            }
            Rect rect = new Rect(0f, curY, width, 22f);
            float num = MassUtility.GearAndInventoryMass(pawn);
            float num2 = MassUtility.Capacity(pawn, null);
            Widgets.Label(rect, "MassCarried".Translate(num.ToString("0.##"), num2.ToString("0.##")));
            curY += 22f;
        }

        public static void TryDrawMassInfoWithImage(Pawn pawn, ref Rect rect)
        {
            if (pawn.Dead || !ShouldShowInventory(pawn))
            {
                return;
            }
            Rect rect1 = new Rect(rect.x, rect.y, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get(RPGIIcons.Mass, true));
            TooltipHandler.TipRegion(rect1, "SandyMassCarried".Translate());
            float num = MassUtility.GearAndInventoryMass(pawn);
            float num2 = MassUtility.Capacity(pawn, null);
            Rect rect2 = new Rect(rect.x + 30f, rect.y + 2f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(num, num2.ToStringMass()));
            //Widgets.Label(rect2, "SandyMassValue".Translate(num.ToString("0.##"), num2.ToString("0.##")));
        }

        public static void TryDrawComfyTemperatureRangeWithImage(Pawn pawn, ref Rect rect)
        {
            if (pawn.Dead)
            {
                return;
            }
            Rect rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/min_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
            Rect rect2 = new Rect(rect.x + 30f, rect.y + 28f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue.ToStringTemperature("F0")
            }));

            rect1 = new Rect(rect.x, rect.y + 52f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/max_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue2 = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
            rect2 = new Rect(rect.x + 30f, rect.y + 56f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue2.ToStringTemperature("F0")
            }));
        }

        public static void TryDrawOverallArmorWithImage(Pawn pawn, Rect rect, StatDef stat, string label, Texture image, bool CE = false, string unit = "%")
        {
            string text = "";
            float num = CE ? CalculateArmorByPartsCE(pawn, stat, ref text, unit)
                           : CalculateArmorByParts(pawn, stat, out text);
            // draw thumbmail
            Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
            GUI.DrawTexture(rect1, image);
            TooltipHandler.TipRegion(rect1, label);
            // draw values
            Rect rect2 = new Rect(rect.x + 34f, rect.y + 3f, 104f, 24f);
            num = (float)Math.Round(num, 1, MidpointRounding.AwayFromZero);
            if (CE)
            {
                Widgets.Label(rect2, string.Concat(num, unit));
            }
            else
            {
                Widgets.Label(rect2, num.ToStringPercent());
            }
            TooltipHandler.TipRegion(rect2, text);
        }

        public static void DrawColonist(Rect rect, Pawn pawn)
        {
            Vector2 pos = new Vector2(rect.width, rect.height);
            GUI.DrawTexture(rect, PortraitsCache.Get(pawn, pos, PawnTextureCameraOffset, 1.18f));
        }

        // TODO Add brawler have ranged weapon text
        public static string TooltipTextForThing(Thing thing, bool labelCap, bool isForced)
        {
            string text = "";
            if (labelCap)
            {
                text = thing.LabelCap;
                if (isForced)
                {
                    text += ", " + "ApparelForcedLower".Translate();
                }
            }

            text += thing.DescriptionDetailed;

            // hit points
            if (thing.def.useHitPoints)
            {
                text += "\n" + thing.HitPoints + " / " + thing.MaxHitPoints;
            }

            // mass
            string mass = (thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount)
                            .ToString("G") + " kg";
            text += "\n" + mass;

            if (thing is Apparel app)
            {
                text += "\n";
                float sharp = app.GetStatValue(StatDefOf.ArmorRating_Sharp);
                float blunt = app.GetStatValue(StatDefOf.ArmorRating_Blunt);
                float heat = app.GetStatValue(StatDefOf.ArmorRating_Heat);
                if (sharp > 0.005)
                {
                    // string.Concat has a minor performance advantage over the good old fashion +
                    // no need for premature optimization, just a note
                    text = string.Concat(text, "\n", "ArmorSharp".Translate(), ":", sharp.ToStringPercent());
                }
                if (blunt > 0.005)
                {
                    text = string.Concat(text, "\n", "ArmorBlunt".Translate(), ":", blunt.ToStringPercent());
                }
                if (heat > 0.005)
                {
                    text = string.Concat(text, "\n", "ArmorHeat".Translate(), ":", heat.ToStringPercent());
                }

            }

            return text;
        }

        // Most part is from the source code
        public static void Wear(Pawn pawn, Apparel newApparel, bool dropReplacedApparel = true)
        {
            if (newApparel.Spawned)
            {
                newApparel.DeSpawn();
            }

            if (!ApparelUtility.HasPartsToWear(pawn, newApparel.def))
            {
                Log.Warning(pawn + " tried to wear " + newApparel + " but he has no body parts required to wear it.");
                return;
            }

            List<Apparel> apparelsToReplace = new List<Apparel>();
            for (int num = pawn.apparel.WornApparelCount - 1; num >= 0; num--)
            {
                Apparel oldApparel = pawn.apparel.WornApparel[num];
                if (!ApparelUtility.CanWearTogether(newApparel.def, oldApparel.def, pawn.RaceProps.body))
                {
                    apparelsToReplace.Add(oldApparel);
                }
            }

            foreach (Apparel apparel in apparelsToReplace)
            {
                if (dropReplacedApparel)
                {
                    bool forbid = pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
                    if (!pawn.apparel.TryDrop(apparel, out Apparel _, pawn.PositionHeld, forbid))
                    {
                        Messages.Message(UIText.FailToDrop.Translate(pawn, apparel), MessageTypeDefOf.NeutralEvent);
                        return;
                    }
                }
                else
                {
                    pawn.apparel.Remove(apparel);
                    pawn.inventory.innerContainer.TryAdd(apparel);
                }
            }

            if (newApparel.Wearer != null)
            {
                Log.Warning(pawn + " is trying to wear " + newApparel + " but this apparel already has a wearer (" + newApparel.Wearer + "). This may or may not cause bugs.");
            }

            // Circumvene accessibility restriction and add apparel directly to inner list.
            object wornApparel = pawn.apparel
                                     .GetType()
                                     .GetField("wornApparel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                     .GetValue(pawn.apparel);
            wornApparel.GetType().GetMethod("TryAdd", new Type[] { typeof(Thing), typeof(bool) }).Invoke(wornApparel, new object[] { newApparel, false });
        }

        private static string formatArmorValue(float value, string unit)
        {
            var asPercent = unit.Equals("%");
            if (asPercent)
            {
                value *= 100f;
            }
            return value.ToStringByStyle(asPercent ? ToStringStyle.FloatMaxOne : ToStringStyle.FloatMaxTwo) + unit;
        }

        public static void DrawMassBreakdown(Pawn pawn, Rect rect)
        {
            string text = "";
            float value;
            foreach (ThingWithComps eq in pawn.equipment.AllEquipmentListForReading)
            {
                value = eq.GetStatValue(StatDefOf.Mass) * eq.stackCount;
                if (value > 0.1)
                {
                    text += string.Concat(eq.LabelShortCap, ": ", value, "kg\n");
                }
            }
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                value = apparel.GetStatValue(StatDefOf.Mass) * apparel.stackCount;
                if (value > 0.1)
                {
                    text += string.Concat(apparel.LabelShortCap, ": ", value, "kg\n");
                }
            }
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                value = thing.GetStatValue(StatDefOf.Mass) * thing.stackCount;
                if (value > 0.1)
                {
                    text += string.Concat(thing.LabelShortCap, ": ", value, "kg\n");
                }
            }
            TooltipHandler.TipRegion(rect, text);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static string Times(this string str, float num)
        {
            if (str.NullOrEmpty())
            {
                return string.Empty;
            }
            int length = Mathf.RoundToInt(str.Length * num);
            char[] array = new char[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = 'A';
            }
            return new string(array);
        }
    }
}
