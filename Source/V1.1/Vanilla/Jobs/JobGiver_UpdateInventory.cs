using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using RPG_Inventory_Remake.Loadout;

namespace RPG_Inventory_Remake
{
    /// <summary>
    /// Insert into 
    /// </summary>
    public class JobGiver_UpdateInventory : ThinkNode_JobGiver
    {
        private enum ItemPriority : byte
        {
            None,
            Low,
            LowStock,
            Proximity
        }

        private int _tinyRadius;
        private int _smallRadius;
        private int _mediumRadius;
        private bool Foundthing;

        #region Constants  

        private const int ProximitySearchRadius = 20;
        private const int MaximumSearchRadius = 80;
        private const int TicksBeforeDropRaw = 40000;

        #endregion

        #region Methods
        /// <summary>
        /// Used by thinknodes that use priotiy to pick jobs
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public override float GetPriority(Pawn pawn)
        {
            //if (pawn.HasExcessThing())
            //{
            //    return 9.2f;
            //}
            ItemPriority priority;
            int i;
            Pawn carriedBy;
            LoadoutSlot slot = GetPrioritySlot(pawn, out priority, out _, out i, out carriedBy);
            if (slot == null)
            {
                return 0f;
            }
            if (priority == ItemPriority.Low) return 3f;

            TimeAssignmentDef assignment = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
            if (assignment == TimeAssignmentDefOf.Sleep) return 3f;

            return 9.2f;
        }

        /// <summary>
        /// This starts the work of finding something lacking that the pawn should pickup.
        /// </summary>
        /// <param name="pawn">Pawn who's inventory and loadout should be considered.</param>
        /// <param name="priority">Priority value for how important doing this job is.</param>
        /// <param name="closestThing">The thing found to be picked up.</param>
        /// <param name="count">The amount of closestThing to pickup.  Already checked if inventory can hold it.</param>
        /// <param name="carriedBy">If unable to find something on the ground to pickup, the pawn (pack animal/prisoner) which has the item to take.</param>
        /// <returns>LoadoutSlot which has something that can be picked up.</returns>
        private LoadoutSlot GetPrioritySlot(Pawn pawn, out ItemPriority priority, out Thing closestThing, out int count, out Pawn carriedBy)
        {
            priority = ItemPriority.None;
            LoadoutSlot slot = null;
            closestThing = null;
            count = 0;
            carriedBy = null;

            CompInventory inventory = pawn.TryGetComp<CompInventory>();
            if (inventory?.container != null)
            {
            }

            return slot;
        }

        /// <summary>
        /// Used by GetPrioritySlot, actually finds a requested thing.
        /// </summary>
        /// <param name="pawn">Pawn to be considered.  Used in checking equipment and position when looking for nearby things.</param>
        /// <param name="curSlot">Pawn's LoadoutSlot being considered.</param>
        /// <param name="findCount">Amount of Thing of ThingDef to try and pickup.</param>
        /// <param name="curPriority">Priority of the job.</param>
        /// <param name="curThing">Thing found near pawn for potential pickup.</param>
        /// <param name="curCarrier">Pawn that is holding the curThing that 'pawn' wants.</param>
        /// <remarks>Was split off into a sepearate method so the code could be run from multiple places in caller but that is no longer needed.</remarks>
        private void FindPickup(Pawn pawn, LoadoutSlot curSlot, int findCount, out ItemPriority curPriority, out Thing curThing, out Pawn curCarrier)
        {
            curPriority = ItemPriority.None;
            curThing = null;
            curCarrier = null;

            Predicate<Thing> isFoodInPrison = (Thing t) => (t.GetRoom()?.isPrisonCell ?? false) && t.def.IsNutritionGivingIngestible && pawn.Faction.IsPlayer;
            // Hint: The following block defines how to find items... pay special attention to the Predicates below.
            ThingRequest req;
            if (curSlot.genericDef != null)
                req = ThingRequest.ForGroup(curSlot.genericDef.thingRequestGroup);
            else
                req = curSlot.thingDef.Minifiable ? ThingRequest.ForGroup(ThingRequestGroup.MinifiedThing) : ThingRequest.ForDef(curSlot.thingDef);
            Predicate<Thing> findItem;
            if (curSlot.genericDef != null)
                findItem = t => curSlot.genericDef.lambda(t.GetInnerIfMinified().def);
            else
                findItem = t => t.GetInnerIfMinified().def == curSlot.thingDef;
            Predicate<Thing> search = t => findItem(t) && !t.IsForbidden(pawn) && pawn.CanReserve(t) && !isFoodInPrison(t);

            // look for a thing near the pawn.
            curThing = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                req,
                PathEndMode.ClosestTouch,
                TraverseParms.For(pawn, Danger.None, TraverseMode.ByPawn),
                ProximitySearchRadius,
                search);
            if (curThing != null) curPriority = ItemPriority.Proximity;
            else
            {
                // look for a thing basically anywhere on the map.
                curThing = GenClosest.ClosestThingReachable(
                    pawn.Position,
                    pawn.Map,
                    req,
                    PathEndMode.ClosestTouch,
                    TraverseParms.For(pawn, Danger.None, TraverseMode.ByPawn),
                    MaximumSearchRadius,
                    search);
                if (curThing == null && pawn.Map != null)
                {
                    // look for a thing inside caravan pack animals and prisoners.  EXCLUDE other colonists to avoid looping state.
                    List<Pawn> carriers = pawn.Map.mapPawns.AllPawns.Where(
                        p => (p.inventory?.innerContainer?.InnerListForReading?.Any() ?? false) && (p.RaceProps.packAnimal && p.Faction == pawn.Faction || p.IsPrisoner && p.HostFaction == pawn.Faction)
                        && pawn.CanReserveAndReach(p, PathEndMode.ClosestTouch, Danger.Deadly, int.MaxValue, 0)).ToList();
                    foreach (Pawn carrier in carriers)
                    {
                        Thing thing = carrier.inventory.innerContainer.FirstOrDefault(t => findItem(t));
                        if (thing != null)
                        {
                            curThing = thing;
                            curCarrier = carrier;
                            break;
                        }
                    }
                }
                if (curThing != null)
                {
                    if (!curThing.def.IsNutritionGivingIngestible && (float)findCount / curSlot.count >= 0.5f) curPriority = ItemPriority.LowStock;
                    else curPriority = ItemPriority.Low;
                }
            }
        }

        /// <summary>
        /// Tries to give the pawn a job related to picking up or dropping an item from their inventory.
        /// </summary>
        /// <param name="pawn">Pawn to which the job is given.</param>
        /// <returns>Job that the pawn was instructed to do, be it hauling a dropped Thing or going and getting a Thing.</returns>
        protected override Job TryGiveJob(Pawn pawn)
        {
            // Get inventory
            CompInventory inventory = pawn.TryGetComp<CompInventory>();
            if (inventory == null) return null;

            RPGILoadout loadout = pawn.GetLoadout();
            if (loadout != null)
            {
                ThingWithComps dropEq;
                ThingWithComps droppedEq;
                if (pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out droppedEq, pawn.Position, false))
                {
                    if (droppedEq != null)
                        return HaulAIUtility.HaulToStorageJob(pawn, droppedEq);
                }
                Thing dropThing = new Thing();
                int dropCount = 0;
                Thing droppedThing;
                if (inventory.container.TryDrop(dropThing, pawn.Position, pawn.Map, ThingPlaceMode.Near, dropCount, out droppedThing))
                {
                    if (droppedThing != null)
                    {
                        return HaulAIUtility.HaulToStorageJob(pawn, droppedThing);
                    }
                    Log.Error(string.Concat(pawn, " tried dropping ", dropThing, " from loadout but resulting thing is null"));
                }

                // Find missing items
                ItemPriority priority;
                Thing closestThing;
                int count;
                Pawn carriedBy;
                bool doEquip = false;
                LoadoutSlot prioritySlot = GetPrioritySlot(pawn, out priority, out closestThing, out count, out carriedBy);
                // moved logic to detect if should equip vs put in inventory here...
                if (closestThing != null)
                {
                    if (closestThing.TryGetComp<CompEquippable>() != null
                        && !(pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                        && (pawn.health != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                        && (pawn.equipment == null || pawn.equipment.Primary == null || !loadout.Any(s => s.def == pawn.equipment.Primary.def)))
                        doEquip = true;
                    if (carriedBy == null)
                    {
                        // Equip gun if unarmed or current gun is not in loadout
                        if (doEquip)
                        {
                            return new Job(JobDefOf.Equip, closestThing);
                        }
                        return new Job(JobDefOf.TakeInventory, closestThing) { count = Mathf.Min(closestThing.stackCount, count) };
                    }
                    else
                    {
                        // TODO Implement Take from Others
                        //return new Job(CE_JobDefOf.TakeFromOther, closestThing, carriedBy, doEquip ? pawn : null)
                        //{
                        //    count = doEquip ? 1 : Mathf.Min(closestThing.stackCount, count)
                        //};
                        return null;
                    }
                }
            }
            return null;
        }

        public static void ResetSearchRadius(int mapSize)
        {
            _smallRadius = mapSize / 5;
            _tinyRadius = Mathf.RoundToInt(_smallRadius / 2.5f);
            _mediumRadius = _smallRadius * 2;
        }

        #endregion
    }
}
