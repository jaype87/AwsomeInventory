using RimWorld;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RPG_Inventory_Remake;
using RPGIResource;

namespace RPG_Inventory_Remake.Loadout
{
    public class LoadoutManager : GameComponent
    {
        #region Fields
        private static List<RPGILoadout> _loadouts = new List<RPGILoadout>();
        private static readonly Regex _pattern = new Regex(@"^(.*?)(\d*)$");
        #endregion Fields

        #region Constructors
        // constructor called on new/first game.
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public LoadoutManager(Game game)
        {
        }

        // this constructor is also used by the scribe.
        public LoadoutManager()
        {

        }
        #endregion Constructors

        #region Properties

        public static List<RPGILoadout> Loadouts => _loadouts;

        #endregion Properties

        #region Override Methods
        /// <summary>
        /// Called by RimWorld when the game is basically ready to be played.
        /// </summary>
        public override void FinalizeInit()
        {
            _loadouts.Clear();
        }

        /// <summary>
        /// Load/Save handler.
        /// </summary>
        public override void ExposeData() // - called when saving a game as well as in the construction phase of creating a new instance on game load.
        {
            Scribe_Collections.Look(ref _loadouts, "loadouts", LookMode.Deep);
        }
        #endregion Override Methods

        #region Methods

        public static void AddLoadout(RPGILoadout loadout)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }
            _loadouts.Add(loadout);
        }

        public static void RemoveLoadout(RPGILoadout loadout)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }
            _loadouts.Remove(loadout);
        }

        public static string GetIncrementalLabel(object obj)
        {
            if (obj is RPGILoadout loadout)
            {
                return GetIncrementalLabel(loadout.Label);
            }
            else if (obj is string previousLabel)
            {
                return GetIncrementalLabel(previousLabel);
            }
            throw new ArgumentException(ErrorMessage.WrongArgumentType, nameof(obj));
        }

        public static string GetIncrementalLabel(string previousLabel)
        {
            string onlyName = _pattern.Match(previousLabel).Groups[1].Value;
            Regex namePattern = new Regex(string.Concat("^(", onlyName, @")(\d*)", '$'));

            List<int> numsPostfix = new List<int>();
            foreach (RPGILoadout loadout in _loadouts)
            {
                Match match = namePattern.Match(loadout.Label);
                if (match.Success)
                {
                    if (Int32.TryParse(match.Groups[2].Value, out int result))
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

        #endregion Methods
    }
}