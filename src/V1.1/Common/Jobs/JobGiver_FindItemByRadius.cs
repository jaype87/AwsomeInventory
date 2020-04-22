// <copyright file="JobGiver_FindItemByRadius.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwesomeInventory.Resources;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// Find item by preset radius.
    /// </summary>
    public class JobGiver_FindItemByRadius : ThinkNode_Priority
    {
        /// <summary>
        /// A list of radius that defines the range of search.
        /// </summary>
        protected List<int> _radius = new List<int>(3);

        /// <summary>
        /// A search factor that times the map size to get a tiny search range.
        /// </summary>
        protected float _tinyRadiusFactor = 0.08f;

        /// <summary>
        /// A search factor that times the map size to get a small search range.
        /// </summary>
        protected float _smallRadiusFactor = 0.2f;

        /// <summary>
        /// A search factor that times the map size to get a medium search range.
        /// </summary>
        protected float _mediumRadiusFactor = 0.4f;

        /// <summary>
        /// Last used radius index.
        /// </summary>
        protected int _lastUsedRadiusIndex = 1;

        /// <summary>
        /// Result of the last search.
        /// </summary>
        protected bool _itemFound = false;

        /// <summary>
        /// Universal validation.
        /// </summary>
        protected Func<Pawn, Thing, bool> _validatorBase = (Pawn p, Thing x) =>
        {
            if (!EquipmentUtility.CanEquip(x, p))
                return false;

            return p.CanReserve(x) && !x.IsForbidden(p) && x.IsSociallyProper(p);
        };

        /// <summary>
        /// Gets the default starting index used for querying _radius.
        /// </summary>
        public static int DefaultRadiusIndex { get; private set; } = 1;

        /// <summary>
        /// Gets a copy of currently used radius.
        /// </summary>
        public List<int> Radius => new List<int>(_radius);

        /// <summary>
        /// Gets the last used radius index.
        /// </summary>
        public int LastUsedRadiusIndex => _lastUsedRadiusIndex;

        /// <summary>
        /// Gets a value indicating whether last search found an item.
        /// </summary>
        public bool Itemfound => _itemFound;

        /// <summary>
        /// Find the best fitted item in <paramref name="searchSet"/>.
        /// </summary>
        /// <param name="pawn"> Pawn who needs a job. </param>
        /// <param name="searchSet"> A list of candidate items. </param>
        /// <param name="validator"> Validate if item in <paramref name="searchSet"/> is suitable. </param>
        /// <param name="priorityGetter"> Give a priority value for items in <paramref name="searchSet"/>. </param>
        /// <param name="searchLevel"> Number of search radius to use. </param>
        /// <returns> Returns the item that is found, null if none is found. </returns>
        public virtual Thing FindItem(Pawn pawn, IEnumerable<Thing> searchSet, Func<Thing, bool> validator = null, Func<Thing, float> priorityGetter = null, int searchLevel = 2)
        {
            if (pawn == null)
            {
                Log.Error(ErrorMessage.ArgumentIsNull + nameof(pawn));
                return null;
            }

            Reset(pawn);

            if (_itemFound)
            {
                if (--_lastUsedRadiusIndex < 0)
                {
                    ++_lastUsedRadiusIndex;
                }

                if (--searchLevel < 1)
                {
                    ++searchLevel;
                }
#if DEBUG
                Log.Message(string.Format(ErrorMessage.ExpectedString, nameof(_itemFound), true, _itemFound));
                Log.Message(string.Format(ErrorMessage.ExpectedString, nameof(_lastUsedRadiusIndex), "-", _lastUsedRadiusIndex));
                Log.Message(string.Format(ErrorMessage.ExpectedString, nameof(searchLevel), "-", searchLevel));
#endif
            }

            Thing thing = null;
            _itemFound = false;
            while (thing == null && searchLevel-- > 0)
            {
                thing = GenClosest.ClosestThing_Global_Reachable(
                    pawn.Position
                    , pawn.Map
                    , searchSet
                    , PathEndMode.OnCell
                    , TraverseParms.For(pawn)
                    , _radius[_lastUsedRadiusIndex]
                    , (Thing x) =>
                    {
                        Thing innerThing = x.GetInnerIfMinified();
                        return _validatorBase(pawn, x) && (validator == null ? true : validator(innerThing));
                    }
                    , priorityGetter);
                if (thing == null)
                {
                    if (++_lastUsedRadiusIndex == _radius.Count)
                    {
                        _lastUsedRadiusIndex = DefaultRadiusIndex;
                        break;
                    }
                }
                else
                {
#if DEBUG
                    Log.Message(string.Format(ErrorMessage.ReportString, "Thing Found", thing.LabelCap, "-"));
#endif
                    _itemFound = true;
                    break;
                }
#if DEBUG
                Log.Message(string.Format(ErrorMessage.ReportString, "While loop end", nameof(searchLevel), searchLevel));
                Log.Message(string.Format(ErrorMessage.ReportString, "While loop end", nameof(_lastUsedRadiusIndex), _lastUsedRadiusIndex));
#endif
            }

            return thing;
        }

        /// <summary>
        /// Reset search radius based on the map <paramref name="pawn"/> is currently in.
        /// </summary>
        /// <param name="pawn"> Pawn that needs a job. </param>
        private void Reset(Pawn pawn)
        {
            float mapSize = pawn.Map.Size.LengthHorizontal;
            _radius.Clear();
            _radius.Add(Mathf.FloorToInt(mapSize * _tinyRadiusFactor));
            _radius.Add(Mathf.FloorToInt(mapSize * _smallRadiusFactor));
            _radius.Add(Mathf.FloorToInt(mapSize * _mediumRadiusFactor));
#if DEBUG
            foreach (int radius in _radius)
            {
                Log.Message(string.Concat(nameof(radius), ": ", radius));
            }
#endif
        }
    }
}