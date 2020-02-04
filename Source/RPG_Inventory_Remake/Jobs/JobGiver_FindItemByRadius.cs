using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse;
using RimWorld;
using RPGIResource;
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake
{
    public abstract class JobGiver_FindItemByRadius<T> : ThinkNode_JobGiver where T : Thing
    {
        protected bool _itemFound = false;
        protected static List<int> _radius = new List<int>();
        protected float _tinyRadiusFactor = 0.08f;
        protected float _smallRadiusFactor = 0.2f;
        protected float _mediumRadiusFactor = 0.4f;
        protected int _defaultRadiusIndex;
        protected int _lastRadiusIndex;

        public static List<int> Radius
        {
            get => new List<int>(_radius);
        }

        protected Func<Pawn, Thing, bool> ValidatorBase = (Pawn p, Thing x) => p.CanReserve(x) && !x.IsBurning();

        public JobGiver_FindItemByRadius()
        {
            int? mapSize = Current.Game?.InitData?.mapSize;
            if (mapSize != null)
            {
                _radius.Add(Mathf.FloorToInt(mapSize.Value * _tinyRadiusFactor));
                _radius.Add(Mathf.FloorToInt(mapSize.Value * _smallRadiusFactor));
                _radius.Add(Mathf.FloorToInt(mapSize.Value * _mediumRadiusFactor));
                _defaultRadiusIndex = Mathf.FloorToInt(_radius.Count / 2);
                _lastRadiusIndex = _defaultRadiusIndex;
            }
            
        }

        protected virtual T FindItem(Pawn pawn, ThingDef thingDef = null, ThingRequestGroup thingRequestGroup = 0, Func<Thing, bool> validator = null, Func<Thing, float> priorityGetter = null, int searchLevel = 1)
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
            }
            Thing thing = null;
            while (thing == null && searchLevel-- >= 0)
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
            }
            return thing as T;
        }
    }
}