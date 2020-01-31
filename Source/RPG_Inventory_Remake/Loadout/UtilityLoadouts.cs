using System.Text.RegularExpressions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using RPG_Inventory_Remake_Common;
using RPGIResource;
using UnityEngine;
using Verse;

namespace RPG_Inventory_Remake.Loadout
{
    [StaticConstructorOnStartup]
    public static class UtilityLoadouts
    {
        #region Fields


        private static float _labelSize = -1f;
        private static float _margin = 6f;
        private static bool _loadoutSet = false;
        private static Texture2D _overburdenedTex;
        private static ThingDef _genericStuffDef = DefDatabase<ThingDef>.GetNamed(StringConstant.GenericResource);

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

        public static ThingDef GenericStuff => _genericStuffDef;

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
                Widgets.FillableBar(barRect, fillPercentage);

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

        public static RPGILoadout GetLoadout(this Pawn pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException(nameof(pawn));

            return pawn.TryGetComp<compRPGILoudout>()?.Loadout;
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
        /// Set Pawn's loadout. Called by a harmony patch
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="loadout"></param>
        public static void SetLoadout(this Pawn pawn, RPGILoadout loadout)
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }
            Log.Message("start setting loadout: " + _loadoutSet);
            if (pawn.TryGetComp<compRPGILoudout>() is compRPGILoudout comp
                && _loadoutSet == false)
            {
                Log.Message("set loadout");
                _loadoutSet = true;
                comp.UpdateForNewLoadout(loadout);
                pawn.outfits.CurrentOutfit = loadout;
            }
            _loadoutSet = false;
        }

        public static string GetDefaultLoadoutName(this Pawn pawn)
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }
            return string.Concat(pawn.Name.ToStringFull, " ", "Corgi_DefaultLoadoutName".Translate());
        }

        // Note Revisit for generic stuff
        public static Thing MakeThingSimple(ThingStuffPairWithQuality pair)
        {
            if (pair.thing.MadeFromStuff && pair.stuff == null)
            {
                if (pair.thing.thingCategories.Count == 1)
                {
                    pair.stuff = GenStuff.DefaultStuffFor(pair.thing);
                }
                else
                {
                    pair.stuff = _genericStuffDef;
                }
            }
            Thing thing = pair.MakeThing();
            if (thing.def.useHitPoints)
            {
                thing.HitPoints = thing.MaxHitPoints;
            }
            return thing;
        }

        public static Thing DeepCopySimple(this Thing thing)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }
            Thing copy = MakeThingSimple(thing.MakeThingStuffPairWithQuality());
            copy.stackCount = thing.stackCount;
            return copy;
        }

        public static ThingStuffPairWithQuality MakeThingStuffPairWithQuality(this Thing thing)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }
            thing.TryGetQuality(out QualityCategory quality);
            return new ThingStuffPairWithQuality(thing.def, thing.Stuff, quality);
        }

        #endregion Methods
    }
}
