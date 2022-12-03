// <copyright file="PawnUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using RimWorld;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Extension functions for pawn.
    /// </summary>
    public static class PawnUtility
    {
        /// <summary>
        /// Gets all player pawns in the game.
        /// </summary>
        public static List<Pawn> AllPlayerPawns
        {
            get
            {
                return Find.Maps
                        .SelectMany(
                            m => m.mapPawns.AllPawns.Where(p => p.Faction == Faction.OfPlayer))
                        .Concat(
                            Find.WorldPawns.AllPawnsAlive.Where(p => p.Faction == Faction.OfPlayer))
                        .Where(
                            p => p.UseLoadout(out _))
                        .ToList();
            }
        }

        /// <summary>
        /// Check if pawn uses loadout.
        /// </summary>
        /// <param name="pawn"> Pawn to check. </param>
        /// <param name="comp"> The comp on <paramref name="pawn"/>, if found. </param>
        /// <returns> Returns true if <paramref name="pawn"/> uses loadout. </returns>
        public static bool UseLoadout(this Pawn pawn, out CompAwesomeInventoryLoadout comp)
        {
            comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            return comp?.Loadout != null && AwesomeInventoryMod.Settings.UseLoadout;
        }

        /// <summary>
        /// Make float menu options for creating empty loadout or loadout derived from equipped items.
        /// </summary>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <returns> A list of options. </returns>
        public static List<FloatMenuOption> MakeActionableLoadoutOption(this Pawn selPawn)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            return new List<FloatMenuOption>()
                {
                    new FloatMenuOption(
                        UIText.MakeEmptyLoadout.Translate(selPawn.NameShortColored)
                        , () =>
                        {
                            AwesomeInventoryLoadout emptyLoadout = AwesomeInventoryLoadout.MakeEmptyLoadout(selPawn);
                            LoadoutManager.AddLoadout(emptyLoadout);
                            selPawn.SetLoadout(emptyLoadout);
                            Find.WindowStack.Add(
                                AwesomeInventoryServiceProvider.MakeInstanceOf<Dialog_ManageLoadouts>(emptyLoadout, selPawn, true));

                            if (BetterPawnControlUtility.IsActive)
                                BetterPawnControlUtility.SaveState(new List<Pawn> { selPawn });
                        }),
                    new FloatMenuOption(
                         UIText.MakeNewLoadout.Translate(selPawn.NameShortColored)
                        , () =>
                        {
                            AwesomeInventoryLoadout loadout = new AwesomeInventoryLoadout(selPawn);
                            LoadoutManager.AddLoadout(loadout);
                            selPawn.SetLoadout(loadout);
                            Find.WindowStack.Add(
                                AwesomeInventoryServiceProvider.MakeInstanceOf<Dialog_ManageLoadouts>(loadout, selPawn, true));

                            if (BetterPawnControlUtility.IsActive)
                                BetterPawnControlUtility.SaveState(new List<Pawn> { selPawn });
                        }),
                };
        }

        /// <summary>
        /// Check wheter pawn has an empty slot to wear <paramref name="newApparel"/>.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="newApparel"> Apparel to wear. </param>
        /// <returns> Returns true if there is an empty slot. </returns>
        public static bool IsOldApparelForced(this Pawn pawn, Apparel newApparel)
        {
            if (pawn.apparel?.WornApparel == null || newApparel == null)
                return false;

            foreach (Apparel wornApparel in pawn.apparel.WornApparel)
            {
                if (ApparelUtility.CanWearTogether(wornApparel.def, newApparel.def, BodyDefOf.Human))
                {
                    continue;
                }
                else if (pawn.outfits.forcedHandler.IsForced(wornApparel))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
