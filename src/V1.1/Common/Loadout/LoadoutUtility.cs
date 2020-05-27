// <copyright file="LoadoutUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
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
        /// <param name="delay"> If true, put jobs for changing apparel in job queue, otherwise, execute jobs immediately. </param>
        /// <param name="forced"> If true, update pawn's comp for <paramref name="loadout"/> even though it is the same as current loadout. </param>
        public static void SetLoadout(this Pawn pawn, AwesomeInventoryLoadout loadout, bool delay = false, bool forced = false)
        {
            if (pawn.TryGetComp<CompAwesomeInventoryLoadout>() is CompAwesomeInventoryLoadout comp)
            {
                if (comp.Loadout == loadout && !forced)
                {
                    return;
                }

                ApparelOptionUtility.StopDressingJobs(pawn);
                comp.UpdateForNewLoadout(loadout, delay, forced: forced);

                if (loadout == pawn.outfits.CurrentOutfit)
                {
                    return;
                }

                pawn.outfits.CurrentOutfit = loadout;

                if (!comp.HotSwapCostume?.InSameLoadoutTree(loadout) ?? true)
                {
                    comp.HotSwapCostume = null;
                    comp.LoadoutBeforeHotSwap = null;
                    comp.HotswapState = CompAwesomeInventoryLoadout.HotSwapState.Inactive;
                }
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

        /// <summary>
        /// Make <see cref="ThingStuffPairWithQuality"/> from <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Thing to be made to pair. </param>
        /// <returns> A struct contains thing def, stuff def and quality information about <paramref name="thing"/>. </returns>
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

        /// <summary>
        /// Make <see cref="ThingGroupSelector"/> from <paramref name="thing"/>. The selector doen't contain reference to <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Source for <see cref="ThingGroupSelector"/>. </param>
        /// <returns> A <see cref="ThingGroupSelector"/> made from <paramref name="thing"/>. </returns>
        public static ThingGroupSelector MakeThingGrouopSelector(this Thing thing)
        {
            SingleThingSelector singleThingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<SingleThingSelector>(new[] { thing });
            ThingGroupSelector thingSelectors = new ThingGroupSelector(thing.def)
            {
                singleThingSelector,
            };

            thingSelectors.SetStackCount(1);
            return thingSelectors;
        }

        /// <summary>
        /// Divide <paramref name="things"/> into different groups.
        /// </summary>
        /// <param name="things"> Things to sort. </param>
        /// <returns> A sorted data entity on things. </returns>
        public static ThingGroupModel MakeThingGroup(this IEnumerable<Thing> things)
        {
            ValidateArg.NotNull(things, nameof(things));

            List<Thing> weapons = new List<Thing>();
            List<Thing> apparels = new List<Thing>();
            List<Thing> items = new List<Thing>();

            foreach (Thing thing in things)
            {
                if (thing == null)
                    continue;
                else if (thing.def.IsWeapon && !thing.def.IsDrug && !thing.def.IsStuff)
                    weapons.Add(thing);
                else if (thing.def.IsApparel)
                    apparels.Add(thing);
                else
                    items.Add(thing);
            }

            return new ThingGroupModel(weapons, apparels, items);
        }

        /// <summary>
        /// Add <paramref name="selector"/> to all loadout in <paramref name="loadouts"/>.
        /// </summary>
        /// <param name="selector"> Selector to add. </param>
        /// <param name="loadouts"> A list of loadouts that <paramref name="selector"/> will be added to. </param>
        public static void AddToLoadouts(this ThingGroupSelector selector, IEnumerable<AwesomeInventoryLoadout> loadouts)
        {
            ValidateArg.NotNull(loadouts, nameof(Loadout));

            foreach (AwesomeInventoryLoadout loadout in loadouts)
            {
                loadout.Add(new ThingGroupSelector(selector));
            }
        }

        /// <summary>
        /// Compare things based on their categories: weapon, apparel or miscellaneous.
        /// </summary>
        public class ThingTypeComparer : IComparer<Thing>
        {
            /// <inheritdoc/>
            public int Compare(Thing x, Thing y)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                else if (x == null)
                    return -1;
                else if (y == null)
                    return 1;
                else if (this.GetOrderIfWeapon(x, y, out int result))
                    return result;
                else if (this.GetOrderIfApparel(x, y, out result))
                    return result;
                else if (x.thingIDNumber > y.thingIDNumber)
                    return 1;
                else if (x.thingIDNumber == y.thingIDNumber)
                    return 0;
                else return -1;
            }

            private bool GetOrderIfWeapon(Thing x, Thing y, out int result)
            {
                return this.Comparer(
                    x
                    , y
                    , (thing) => thing.def.IsWeapon && !thing.def.IsDrug
                    , (thingX, thingY) =>
                    {
                        this.Comparer(thingX, thingY, (t) => t.def.IsRangedWeapon, (a, b) => 0, out int r);
                        return r;
                    }
                    , out result);
            }

            private bool GetOrderIfApparel(Thing x, Thing y, out int result)
            {
                return this.Comparer(
                    x
                    , y
                    , (thing) => thing.def.IsApparel
                    , (thingX, thingY) =>
                    {
                        int listOrderX = thingX.def.apparel.bodyPartGroups[0].listOrder;
                        int listOrderY = thingY.def.apparel.bodyPartGroups[0].listOrder;
                        if (listOrderX == listOrderY)
                            return 0;
                        else if (listOrderX > listOrderY)
                            return 1;
                        else
                            return -1;
                    }
                    , out result);
            }

            private bool Comparer(Thing x, Thing y, Func<Thing, bool> filter, Func<Thing, Thing, int> worker, out int result)
            {
                bool flagX = filter(x);
                bool flagY = filter(y);

                if (flagX && flagY && worker != null)
                    result = worker(x, y);
                else if (flagX)
                    result = 1;
                else if (flagY)
                    result = -1;
                else
                    result = 0;

                return result != 0;
            }
        }
    }
}
