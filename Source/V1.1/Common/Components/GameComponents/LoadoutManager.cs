// <copyright file="LoadoutManager.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeInventory.Resources;
using RimWorld;
using RPGIResource;
using Verse;

namespace AwesomeInventory.Common.Loadout
{
    /// <summary>
    /// It handles loadouts CRUD operations in the game.
    /// </summary>
    public class LoadoutManager : GameComponent
    {
        #region Fields
        private static readonly List<AILoadout> _loadouts = new List<AILoadout>();
        private static readonly Regex _pattern = new Regex(@"^(.*?)(\d*)$");
        private static List<Outfit> _outfits;
        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadoutManager"/> class.
        /// </summary>
        /// <param name="game"> Current game. </param>
        /// <remarks> Constructor is called on new/first game. </remarks>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
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
        /// Gets a cache of loadouts used in this game.
        /// </summary>
        public static List<AILoadout> Loadouts => _loadouts;

        #region Override Methods

        /// <summary>
        /// Called by RimWorld when the game is basically ready to be played.
        /// </summary>
        public override void FinalizeInit()
        {
            _loadouts.Clear();
            _outfits = Current.Game.outfitDatabase.AllOutfits;
            foreach (Outfit outfit in _outfits)
            {
                if (outfit is AILoadout loadout)
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
        }

        #endregion Override Methods

        public static void AddLoadout(AILoadout loadout)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }
            _outfits.Add(loadout);
            _loadouts.Add(loadout);
        }

        public static bool TryRemoveLoadout(AILoadout loadout, bool fromOutfit = false)
        {
            if (!fromOutfit)
            {
                AcceptanceReport report = Current.Game.outfitDatabase.TryDelete(loadout);
                if (report.Accepted)
                {
                    return true;
                }
                Messages.Message(report.Reason, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            else
            {
                _loadouts.Remove(loadout);
                return true;
            }
        }

        public static string GetIncrementalLabel(object obj)
        {
            if (obj is AILoadout loadout)
            {
                return GetIncrementalLabel(loadout.label);
            }
            else if (obj is string previousLabel)
            {
                return GetIncrementalLabel(previousLabel);
            }
            throw new ArgumentException(ErrorMessage.WrongArgumentType, nameof(obj));
        }

        public static string GetIncrementalLabel(string previousLabel)
        {
            if (previousLabel.NullOrEmpty())
            {
                throw new ArgumentNullException(nameof(previousLabel));
            }
            string onlyName = _pattern.Match(previousLabel).Groups[1].Value;
            Regex namePattern = new Regex(string.Concat("^(", onlyName, @")(\d*)", '$'));

            List<int> numsPostfix = new List<int>();
            foreach (AILoadout loadout in _loadouts)
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
    }
}