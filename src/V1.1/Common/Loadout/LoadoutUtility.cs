// <copyright file="LoadoutUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
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
    /// <summary>
    /// Utility support for loadout.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class LoadoutUtility
    {
        /// <summary>
        /// Get the current loadout from <paramref name="pawn"/>.
        /// </summary>
        /// <param name="pawn"> Target pawn. </param>
        /// <returns> Loadout assigned to <paramref name="pawn"/>. </returns>
        public static AwesomeInventoryLoadout GetLoadout(this Pawn pawn)
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }

            return pawn.TryGetComp<CompAwesomeInventoryLoadout>()?.Loadout;
        }

        public static string GetDetailedTooltip(this ThingDef def, int count = 1)
        {
            return def.description +
                (count != 1 ? " x" + count : string.Empty) +
                "\n" + def.GetWeightTip(count) + "\n";
        }

        public static string GetWeightTip(this ThingDef def, int count = 1)
        {
            return
                UIText.Weight.Translate() + ": " + StatDefOf.Mass.ValueToString(def.GetStatValueAbstract(StatDefOf.Mass) * count, StatDefOf.Mass.toStringNumberSense);
        }

        /// <summary>
        /// Set Pawn's loadout. Called by a harmony patch, Pawn_OutfitTracker_CurrentOutfit.
        /// </summary>
        /// <param name="pawn"> Set loadout on this <paramref name="pawn"/>. </param>
        /// <param name="loadout"> Loadout to assign to <paramref name="pawn"/>. </param>
        public static void SetLoadout(this Pawn pawn, AwesomeInventoryLoadout loadout)
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

        public static Thing MakeThing(ThingStuffPairWithQuality pair)
        {
            if (pair.thing.MadeFromStuff && pair.stuff == null)
            {
                pair.stuff = AwesomeInventoryStuffDefOf.AwesomeInventoryGenericResource;
            }

            Thing thing = pair.MakeThing();
            if (thing.def.useHitPoints)
            {
                thing.HitPoints = thing.MaxHitPoints;
            }

            return thing;
        }

        /// <summary>
        /// Creata a carbon copy of <paramref name="thing"/> except <see cref="Thing.stackCount"/>.
        /// </summary>
        /// <param name="thing"> Thing to copy. </param>
        /// <param name="withID"> If true, assign ID to the newly created thing. </param>
        /// <returns> A copy of <paramref name="thing"/>. </returns>
        public static Thing DeepCopy(this Thing thing, bool withID = true)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }

            Thing copy;
            if (withID)
            {
                copy = MakeThing(thing.MakeThingStuffPairWithQuality());
            }
            else
            {
                copy = MakeThingWithoutID(thing.MakeThingStuffPairWithQuality());
            }

            copy.stackCount = thing.stackCount;
            return copy;
        }

        public static Thing MakeThingWithoutID(this ThingStuffPairWithQuality pair)
        {
            if (pair.thing.MadeFromStuff && pair.stuff == null)
            {
                pair.stuff = AwesomeInventoryStuffDefOf.AwesomeInventoryGenericResource;
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

            if (thing is ThingWithComps thingWithComps)
            {
                thingWithComps.InitializeComps();
            }

            thing.TryGetComp<CompQuality>()?.SetQuality(pair.Quality, ArtGenerationContext.Outsider);
            return thing;
        }

        public static Thing MakeThingWithoutID(this ThingStuffPair pair)
        {
            if (pair.thing.MadeFromStuff && pair.stuff == null)
            {
                pair.stuff = AwesomeInventoryStuffDefOf.AwesomeInventoryGenericResource;
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

            if (thing is ThingWithComps thingWithComps)
            {
                thingWithComps.InitializeComps();
            }

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
                quality = null,
            };
        }
    }
}
