// <copyright file="JobGiver_FindItemByRadius.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AwesomeInventory.Resources;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    public abstract class JobGiver_FindItemByRadius<T> : ThinkNode_JobGiver
        where T : Thing
    {
        protected bool _itemFound = false;
        protected static List<int> _radius = new List<int>();
        protected const float _tinyRadiusFactor = 0.08f;
        protected const float _smallRadiusFactor = 0.2f;
        protected const float _mediumRadiusFactor = 0.4f;
        protected static int _defaultRadiusIndex;
        protected static int _lastRadiusIndex;

        public static List<int> Radius
        {
            get => new List<int>(_radius);
        }

        protected Func<Pawn, Thing, bool> ValidatorBase = (Pawn p, Thing x) => p.CanReserve(x) && !x.IsBurning();

        protected virtual T FindItem(Pawn pawn, ThingDef thingDef = null, ThingRequestGroup thingRequestGroup = 0, Func<Thing, bool> validator = null, Func<Thing, float> priorityGetter = null, int searchLevel = 2)
        {
            if (thingDef == null && thingRequestGroup == ThingRequestGroup.Undefined)
            {
                Log.Error(string.Format(ErrorMessage.BothArgumentsAreNull, nameof(thingDef), nameof(thingRequestGroup)));
                return null;
            }
            if (thingDef != null && thingRequestGroup != ThingRequestGroup.Undefined)
            {
                Log.Error(string.Format(ErrorMessage.BothArgumentsAreNotNull, nameof(thingDef), nameof(thingRequestGroup)));
                return null;
            }
            if (pawn == null)
            {
                Log.Error(ErrorMessage.ArgumentIsNull + nameof(pawn));
                return null;
            }
            if (_itemFound)
            {
                if (--_lastRadiusIndex < 0)
                {
                    ++_lastRadiusIndex;
                }
                if (--searchLevel < 1)
                {
                    ++searchLevel;
                }
#if DEBUG
                Log.Message(string.Format(ErrorMessage.ExpectedString, nameof(_itemFound), true, _itemFound));
                Log.Message(string.Format(ErrorMessage.ExpectedString, nameof(_lastRadiusIndex), "-", _lastRadiusIndex));
                Log.Message(string.Format(ErrorMessage.ExpectedString, nameof(searchLevel), "-", searchLevel));
#endif
            }
            Thing thing = null;
            while (thing == null && searchLevel-- > 0)
            {
                thing = GenClosest.ClosestThing_Global_Reachable
                    (pawn.Position
                    , pawn.Map
                    , thingDef != null ? pawn.Map.listerThings.ThingsOfDef(thingDef) : pawn.Map.listerThings.ThingsInGroup(thingRequestGroup)
                    , PathEndMode.OnCell
                    , TraverseParms.For(pawn)
                    , _radius[_lastRadiusIndex]
                    , (Thing x) => ValidatorBase(pawn, x) && validator == null ? true : validator(x)
                    , priorityGetter
                    );
                if (thing == null)
                {
                    _itemFound = false;
                    if (++_lastRadiusIndex == _radius.Count)
                    {
                        _lastRadiusIndex = _defaultRadiusIndex;
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
                Log.Message(string.Format(ErrorMessage.ReportString, "While loop end", nameof(_lastRadiusIndex), _lastRadiusIndex));
#endif
            }
            return thing as T;
        }

        public static void Reset()
        {
            int mapSize = Current.Game.AnyPlayerHomeMap.Size.x;
            _radius.Add(Mathf.FloorToInt(mapSize * _tinyRadiusFactor));
            _radius.Add(Mathf.FloorToInt(mapSize * _smallRadiusFactor));
            _radius.Add(Mathf.FloorToInt(mapSize * _mediumRadiusFactor));
            _defaultRadiusIndex = Mathf.FloorToInt(_radius.Count / 2);
            _lastRadiusIndex = _defaultRadiusIndex;
#if DEBUG
            foreach (int radius in _radius)
            {
                Log.Message(string.Concat(nameof(radius), ": ", radius));
            }
#endif
        }
    }
}