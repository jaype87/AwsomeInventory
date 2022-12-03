// <copyright file="Utility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AwesomeInventory.UI;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RPGIResource;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AwesomeInventory
{
    public static class Utility
    {
        public const float StandardLineHeight = 22f;

        /// <summary>
        /// Camera offset used for drawing paper doll in Jealous tab.
        /// </summary>
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

        /// <summary>
        /// Check if inventory should be shown for <paramref name="p"/>.
        /// </summary>
        /// <param name="p"> Pawn to check. </param>
        /// <returns> Returns true if inventory should be displayed. </returns>
        public static bool ShouldShowInventory(Pawn p)
        {
            ValidateArg.NotNull(p, nameof(p));

            return p.RaceProps.Humanlike || p.inventory.innerContainer.Any;
        }

        /// <summary>
        /// Check if apparel should be shown for <paramref name="p"/>.
        /// </summary>
        /// <param name="p"> Pawn to check. </param>
        /// <returns> Returns true if apparel should be displayed. </returns>
        public static bool ShouldShowApparel(Pawn p)
        {
            return p.apparel != null && (p.RaceProps.Humanlike || p.apparel.WornApparel.Any<Apparel>());
        }

        /// <summary>
        /// Check if Equipment should be shown for <paramref name="p"/>.
        /// </summary>
        /// <param name="p"> Pawn to check. </param>
        /// <returns> Returns true if Equipment should be displayed. </returns>
        public static bool ShouldShowEquipment(Pawn p)
        {
            return p.equipment != null;
        }

        /// <summary>
        ///     Draw overall armor for the jealous tab.
        /// </summary>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <param name="rect"> Rect for drawing. </param>
        /// <param name="stat"> Stat to draw. </param>
        /// <param name="label"> Label for <paramref name="stat"/>. </param>
        /// <param name="image"> Image that represents <paramref name="stat"/>. </param>
        /// <param name="unit"> Unit of <paramref name="stat"/>. </param>
        public static void TryDrawOverallArmorCE(Pawn selPawn, Rect rect, StatDef stat, string label, Texture image, string unit)
        {
            string text;
            float armorRating = CalculateArmorByParts(selPawn, stat, out text);
            Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
            GUI.DrawTexture(rect1, image);
            TooltipHandler.TipRegion(rect1, label);
            Rect rect2 = new Rect(rect.x + 60f, rect.y + 3f, 104f, 24f);
            TooltipHandler.TipRegion(rect2, text);

            Widgets.Label(rect2, armorRating.ToStringPercent());
        }

        public static string FormatArmorValue(float value, string unit)
        {
            var asPercent = unit.Equals("%", StringComparison.InvariantCulture);
            if (asPercent)
                value *= 100f;

            return value.ToStringByStyle(asPercent ? ToStringStyle.FloatMaxOne : ToStringStyle.FloatMaxTwo) + unit;
        }

        /// <summary>
        /// Not a very effective way to get armor number, but one with most compatibility, only if modders add
        /// the "ListOrder" tag and correct value to their new "BodyPartGroup" element in xml.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="stat">Sharp, blunt or heat, maybe electrical?. </param>
        /// <param name="text">Text for tooltip. </param>
        /// <returns> Value for <paramref name="stat"/>. </returns>
        public static float CalculateArmorByParts(Pawn pawn, StatDef stat, out string text)
        {
            text = string.Empty;
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
            ValidateArg.NotNull(pawn, nameof(pawn));

            text = string.Empty;
            float num = 0f;
            List<Apparel> wornApparel = pawn.apparel?.WornApparel;
            if (wornApparel == null)
                return num;

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

        public static void DrawColonist(Rect rect, Pawn pawn)
        {
            Vector2 pos = new Vector2(rect.width, rect.height);
            GUI.DrawTexture(rect, PortraitsCache.Get(pawn, pos, pawn.Rotation, PawnTextureCameraOffset, 1.18f));
        }

        /// <summary>
        /// Order <paramref name="pawn"/> to wear new apparel.
        /// </summary>
        /// <param name="pawn"> Pawn who is about to wear <paramref name="newApparel"/>. </param>
        /// <param name="newApparel"> Apparel to wear. </param>
        /// <param name="dropReplacedApparel"> If true, drop the old apparel. </param>
        public static void Wear(Pawn pawn, Apparel newApparel, bool dropReplacedApparel = true)
        {
            ValidateArg.NotNull(newApparel, nameof(newApparel));
            ValidateArg.NotNull(pawn, nameof(pawn));

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
                    pawn.inventory.innerContainer.TryAddOrTransfer(apparel);
                }
            }

            if (newApparel.Wearer != null)
            {
                Log.Warning(pawn + " is trying to wear " + newApparel + " but this apparel already has a wearer (" + newApparel.Wearer + "). This may or may not cause bugs.");
            }

            pawn.apparel.GetDirectlyHeldThings().TryAdd(newApparel);
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

        public static T Next<T>(this T src)
            where T : Enum
        {
            T[] array = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf(array, src) + 1;
            return (array.Length == j) ? array[0] : array[j];
        }

        public static T Previous<T>(this T src)
            where T : Enum
        {
            T[] array = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf(array, src) - 1;
            return (j == -1) ? array.Last() : array[j];
        }
    }
}
