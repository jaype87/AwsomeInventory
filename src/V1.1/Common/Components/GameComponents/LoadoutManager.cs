// <copyright file="LoadoutManager.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// It handles loadouts CRUD operations in the game.
    /// </summary>
    public class LoadoutManager : GameComponent
    {
        private static readonly List<AwesomeInventoryLoadout> _loadouts = new List<AwesomeInventoryLoadout>();
        private static readonly Regex _pattern = new Regex(@"^(.*?)(\d*)$");
        private static int _thingGroupSelectorID = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadoutManager"/> class.
        /// </summary>
        /// <param name="game"> Current game. </param>
        /// <remarks> Constructor is called on new/first game. </remarks>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by game code.")]
        public LoadoutManager(Game game)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadoutManager"/> class.
        /// </summary>
        public LoadoutManager()
        {
        }

        /// <summary>
        /// Gets an ID for <see cref="ThingGroupSelector"/>.
        /// </summary>
        public static int ThingGroupSelectorID => _thingGroupSelectorID++;

        /// <summary>
        /// Gets a cache of loadouts used in this game.
        /// </summary>
        public static List<AwesomeInventoryLoadout> Loadouts => _loadouts;

        /// <summary>
        /// Gets a list of loadouts that are of type <see cref="AwesomeInventoryLoadout"/>.
        /// </summary>
        public static List<AwesomeInventoryLoadout> PlainLoadouts
            => _loadouts.Where(l => l.GetType() == typeof(AwesomeInventoryLoadout)).ToList();

        /// <summary>
        /// Add loadout to manager and the game's outfit database.
        /// </summary>
        /// <param name="loadout"> Loadout to add. </param>
        public static void AddLoadout(AwesomeInventoryLoadout loadout)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));

            _loadouts.Add(loadout);
            Current.Game.outfitDatabase.AllOutfits.Add(loadout);
        }

        /// <summary>
        /// Remove outfit from <see cref="LoadoutManager"/>.
        /// </summary>
        /// <param name="loadout"> Loadout to remove. </param>
        /// <param name="fromOutfit"> If ture, this method is called from <see cref="OutfitDatabase.TryDelete(Outfit)"/>. </param>
        /// <returns> Returns true if loadout is rmeoved successfully. </returns>
        public static bool TryRemoveLoadout(AwesomeInventoryLoadout loadout, bool fromOutfit = false)
        {
            if (fromOutfit)
            {
                _loadouts.Remove(loadout);
                return true;
            }
            else
            {
                AcceptanceReport report = Current.Game.outfitDatabase.TryDelete(loadout);
                if (report.Accepted)
                {
                    return true;
                }

                Messages.Message(report.Reason, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
        }

        /// <summary>
        /// Increment the number in a label.
        /// </summary>
        /// <param name="previousLabel"> Label for reference.</param>
        /// <returns> A label with a number suffic that is one larger than that of <paramref name="previousLabel"/>. </returns>
        public static string GetIncrementalLabel(string previousLabel)
        {
            if (previousLabel.NullOrEmpty())
            {
                throw new ArgumentNullException(nameof(previousLabel));
            }

            string onlyName = _pattern.Match(previousLabel).Groups[1].Value;
            Regex namePattern = new Regex(string.Concat("^(", onlyName, @")(\d*)", '$'));

            List<int> numsPostfix = new List<int>();
            foreach (AwesomeInventoryLoadout loadout in _loadouts)
            {
                Match match = namePattern.Match(loadout.label);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[2].Value, out int result))
                    {
                        numsPostfix.Add(result);
                    }
                }
            }

            int num = 1;
            while (num < numsPostfix.Count + 1)
            {
                if (numsPostfix.Any(n => n == num))
                {
                    ++num;
                    continue;
                }

                break;
            }

            return string.Concat(onlyName, num);
        }

        #region Override Methods

        /// <summary>
        /// Called by RimWorld when the game is ready to be played.
        /// </summary>
        public override void FinalizeInit()
        {
            _loadouts.Clear();
            foreach (Outfit outfit in Current.Game.outfitDatabase.AllOutfits)
            {
                if (outfit is AwesomeInventoryLoadout loadout)
                {
                    _loadouts.Add(loadout);
                }
            }
        }

        /// <summary>
        /// Load/Save handler. Loadouts are saved by OutfitDatabase.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref _thingGroupSelectorID, nameof(_thingGroupSelectorID));
        }

        #endregion Override Methods
    }
}