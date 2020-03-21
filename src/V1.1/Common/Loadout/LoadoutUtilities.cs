// <copyright file="LoadoutUtilities.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AwesomeInventory.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Loadout
{
    [StaticConstructorOnStartup]
    public static class LoadoutUtilities
    {
        #region Fields

        private static float _labelSize = -1f;
        private static float _margin = 6f;
        private static bool _loadoutSet = false;
        private static Texture2D _overburdenedTex;

        #endregion Fields

        #region Properties

        public static float LabelSize
        {
            get
            {
                if (_labelSize < 0)
                {
                    // get size of label
                    _labelSize = (_margin + Text.CalcSize("Corgi_Weight".Translate()).x);
                }
                return _labelSize;
            }
        }

        public static Texture2D OverburdenedTex
        {
            get
            {
                if (_overburdenedTex == null)
                    _overburdenedTex = SolidColorMaterials.NewSolidColorTexture(Color.red);
                return _overburdenedTex;
            }
        }

        #endregion Properties

        #region Methods

        public static void DrawBar(Rect canvas, float current, float capacity, string label = "", string tooltip = "")
        {
            // rects
            Rect labelRect = new Rect(canvas);
            Rect barRect = new Rect(canvas);
            if (!label.NullOrEmpty())
                barRect.xMin += LabelSize;
            labelRect.width = LabelSize;

            // label
            if (!label.NullOrEmpty())
                Widgets.Label(labelRect, label);

            // bar
            bool overburdened = current > capacity;
            float fillPercentage = overburdened ? 1f : (float.IsNaN(current / capacity) ? 1f : current / capacity);
            if (overburdened)
            {
                Widgets.FillableBar(barRect, fillPercentage, OverburdenedTex);
                DrawBarThreshold(barRect, capacity / current, 1f);
            }
            else
            {
                Widgets.FillableBar(barRect, fillPercentage, AwesomeInventoryTex.RoyalBlueTex as Texture2D);
            }

            // tooltip
            if (!tooltip.NullOrEmpty())
                TooltipHandler.TipRegion(canvas, tooltip);
        }

        public static void DrawBarThreshold(Rect barRect, float pct, float curLevel = 1f)
        {
            float thresholdBarWidth = (float)((barRect.width <= 60f) ? 1 : 2);

            Rect position = new Rect(barRect.x + barRect.width * pct - (thresholdBarWidth - 1f), barRect.y + barRect.height / 2f, thresholdBarWidth, barRect.height / 2f);
            Texture2D image;
            if (pct < curLevel)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        public static AILoadout GetLoadout(this Pawn pawn)
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }

            return pawn.TryGetComp<CompAwesomeInventoryLoadout>()?.Loadout;
        }

        public static int GetLoadoutId(this Pawn pawn)
        {
            return 0;
        }

        public static string GetWeightAndBulkTip(this ThingDef def, int count = 1)
        {
            return def.LabelCap +
                (count != 1 ? " x" + count : "") +
                "\n" + def.GetWeightTip(count) + "\n";
        }

        public static string GetWeightTip(this ThingDef def, int count = 1)
        {
            return
                "Corgi_Weight".Translate() + ": " + StatDefOf.Mass.ValueToString(def.GetStatValueAbstract(StatDefOf.Mass) * count, StatDefOf.Mass.toStringNumberSense);
        }

        public static string GetWeightTip(this Thing thing)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));
            return
                //"Corgi_Weight".Translate() + ": " + StatDefOf.Mass.ValueToString(thing.GetStatValue(StatDefOf.Mass) * thing.stackCount, StatDefOf.Mass.toStringNumberSense);
                "Corgi_Weight".Translate() + ": " + (thing.GetStatValue(StatDefOf.Mass) * thing.stackCount).ToStringMass();
        }

        /// <summary>
        /// Set Pawn's loadout. Called by a harmony patch, Pawn_OutfitTracker_CurrentOutfit.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="loadout"></param>
        public static void SetLoadout(this Pawn pawn, AILoadout loadout)
        {
            if (pawn.TryGetComp<CompAwesomeInventoryLoadout>() is CompAwesomeInventoryLoadout comp)
            {
                if (comp.Loadout == loadout)
                {
                    return;
                }
                comp.UpdateForNewLoadout(loadout);
                if (loadout == pawn.outfits.CurrentOutfit)
                {
                    return;
                }
                pawn.outfits.CurrentOutfit = loadout;
            }
        }

        public static string GetDefaultLoadoutName(this Pawn pawn)
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }
            return string.Concat(pawn.Name.ToStringFull, " ", "Corgi_DefaultLoadoutName".Translate());
        }

        public static Thing MakeThingSimple(ThingStuffPairWithQuality pair)
        {
            if (pair.thing.MadeFromStuff && pair.stuff == null)
            {
                pair.stuff = RPGI_StuffDefOf.RPGIGenericResource;
            }
            Thing thing = pair.MakeThing();
            if (thing.def.useHitPoints)
            {
                thing.HitPoints = thing.MaxHitPoints;
            }
            return thing;
        }

        /// <summary>
        /// Copy def, stuff, quality and stackCount
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static Thing DeepCopySimple(this Thing thing, bool withID = true)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }
            Thing copy;
            if (withID)
            {
                copy = MakeThingSimple(thing.MakeThingStuffPairWithQuality());
            }
            else
            {
                copy = MakeThingWithoutID(thing.MakeThingStuffPairWithQuality());
            }
            copy.stackCount = thing.stackCount;
            return copy;
        }

        private static Thing MakeThingWithoutID(this ThingStuffPairWithQuality pair)
        {
            if (pair.thing.MadeFromStuff && pair.stuff == null)
            {
                pair.stuff = RPGI_StuffDefOf.RPGIGenericResource;
            }
            if (pair.stuff != null && !pair.stuff.IsStuff)
            {
                Log.Error("MakeThing error: Tried to make " + pair.thing + " from " + pair.stuff + " which is not a stuff. Assigning default.");
                pair.stuff = GenStuff.DefaultStuffFor(pair.thing);
            }
            if (!pair.thing.MadeFromStuff && pair.stuff != null)
            {
                Log.Error("MakeThing error: " + pair.thing + " is not madeFromStuff but stuff=" + pair.stuff + ". Setting to null.");
                pair.stuff = null;
            }
            Thing thing = (Thing)Activator.CreateInstance(pair.thing.thingClass);
            thing.def = pair.thing;
            thing.SetStuffDirect(pair.stuff);
            if (thing.def.useHitPoints)
            {
                thing.HitPoints = thing.MaxHitPoints;
            }
            thing.TryGetComp<CompQuality>()?.SetQuality(pair.Quality, ArtGenerationContext.Outsider);
            return thing;
        }

        public static ThingStuffPairWithQuality MakeThingStuffPairWithQuality(this Thing thing)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }
            if (thing.TryGetQuality(out QualityCategory quality))
            {
                return new ThingStuffPairWithQuality(thing.def, thing.Stuff, quality);
            }
            return new ThingStuffPairWithQuality
            {
                thing = thing.def,
                stuff = thing.Stuff,
                quality = null
            };
        }

        public static bool CompareThing(Thing x, Thing y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            ThingStuffPairWithQuality pairX = x.MakeThingStuffPairWithQuality();
            ThingStuffPairWithQuality pairY = y.MakeThingStuffPairWithQuality();
            return pairX == pairY;
        }

        #endregion Methods
    }
}
