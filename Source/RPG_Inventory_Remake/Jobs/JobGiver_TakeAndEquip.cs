using RimWorld;
using RPG_Inventory_Remake.Loadout;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RPG_Inventory_Remake
{
    // TODO Refactor
    /// <summary>
    /// Legacy JobGiver from CE, it touches equipment and ammo but not other parts of loadout. The name creates confusion.
    /// After trimming down for vanilla, it becomes an AI oriented JobGiver.
    /// </summary>
    public class JobGiver_TakeAndEquip : ThinkNode_JobGiver
    {
        /// <summary>
        /// For reference, priority to rest is 9.5 and need for chemical is 9.25. Minimum is 0.
        /// </summary>
        private enum WorkPriority
        {
            None,
            Unloading,
            Weapon
            //Apparel
        }

        private WorkPriority GetPriorityWork(Pawn pawn)
        {
            #region Traders have no work priority
            if (pawn.kindDef.trader)
            {
                return WorkPriority.None;
            }
            #endregion

            #region Colonists with primary and a loadout have no work priority
            if (pawn.Faction.IsPlayer)
            {
                RPGILoadout loadout = pawn.GetLoadout();
                // if (loadout != null && !loadout.Slots.NullOrEmpty())
                if (loadout != null && loadout.Any())
                {
                    return WorkPriority.None;
                }
            }
            #endregion

            bool hasPrimary = (pawn.equipment != null && pawn.equipment.Primary != null);

            // Pawns without weapon..
            if (!hasPrimary)
            {
                // With inventory && non-colonist && not stealing && little space left
                if (Unload(pawn))
                {
                    return WorkPriority.Unloading;
                }
                // Without inventory || colonist || stealing || lots of space left
                if (!hasWeaponInInventory(pawn))
                {
                    return WorkPriority.Weapon;
                }
            }

            return WorkPriority.None;
        }

        // Currently not in use
        public override float GetPriority(Pawn pawn)
        {
            // Wild man has no faction
            if (pawn.Faction == null) return 0f;

            var priority = GetPriorityWork(pawn);

            if (priority == WorkPriority.Unloading) return 9.2f;
            else if (priority == WorkPriority.Weapon) return 8f;
            //else if (priority == WorkPriority.Apparel) return 5f;
            else if (priority == WorkPriority.None) return 0f;

            TimeAssignmentDef assignment = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
            if (assignment == TimeAssignmentDefOf.Sleep) return 0f;

            if (pawn.health == null || pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return 0f;
            }
            else return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Faction == null) //Wild man (b19 incident added) Faction is null
            {
                return null;
            }

            if (!pawn.RaceProps.Humanlike || (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent)))
            {
                return null;
            }
            if (pawn.Faction.IsPlayer && pawn.Drafted)
            {
                return null;
            }

            if (!Rand.MTBEventOccurs(60, 5, 30))
            {
                return null;
            }

            if (!pawn.Faction.IsPlayer && FindBattleWorthyEnemyPawnsCount(Find.CurrentMap, pawn) > 25)
            {
                return null;
            }

            //Log.Message(pawn.ThingID +  " - priority:" + (GetPriorityWork(pawn)).ToString() + " capacityWeight: " + pawn.TryGetComp<CompInventory>().capacityWeight.ToString() + " currentWeight: " + pawn.TryGetComp<CompInventory>().currentWeight.ToString() + " capacityBulk: " + pawn.TryGetComp<CompInventory>().capacityBulk.ToString() + " currentBulk: " + pawn.TryGetComp<CompInventory>().currentBulk.ToString());

            bool IsBrawler = pawn.story?.traits?.HasTrait(TraitDefOf.Brawler) ?? false;
            CompInventory inventory = pawn.TryGetComp<CompInventory>();
            bool hasPrimary = pawn.equipment?.Primary != null;
            if (inventory != null)
            {
                if (!pawn.Faction.IsPlayer)
                {
                    ThingWithComps rangedWeapon = inventory.rangedWeaponList.FirstOrDefault();
                    ThingWithComps meleeWeapon = inventory.meleeWeaponList.FirstOrDefault();
                    if (rangedWeapon != null && !IsBrawler)
                    {
                        if (pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= pawn.skills.GetSkill(SkillDefOf.Melee).Level
                        || pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 6)
                        {
                            if (!(hasPrimary && pawn.equipment.Primary.def.IsRangedWeapon))
                            {
                                inventory.TrySwitchToWeapon(rangedWeapon);
                            }
                        }
                        else
                        {
                            if (!(hasPrimary && pawn.equipment.Primary.def.IsMeleeWeapon))
                            {
                                if (meleeWeapon != null)
                                {
                                    inventory.TrySwitchToWeapon(meleeWeapon);
                                }
                            }
                            else
                            {
                                inventory.TrySwitchToWeapon(rangedWeapon);
                            }
                        }
                    }
                    else if (IsBrawler)
                    {
                        if (hasPrimary)
                        {
                            if (!pawn.equipment.Primary.def.IsMeleeWeapon)
                            {
                                if (meleeWeapon != null)
                                {
                                    inventory.TrySwitchToWeapon(meleeWeapon);
                                }
                            }
                        }
                        else
                        {
                            ThingWithComps weapon = meleeWeapon ?? rangedWeapon;
                            if (weapon != null)
                            {
                                pawn.equipment.AddEquipment(weapon);
                            }
                        }
                    }
                    else
                    {
                        if (meleeWeapon != null)
                        {
                            pawn.equipment.AddEquipment(meleeWeapon);
                        }
                    }
                }

                WorkPriority priority = GetPriorityWork(pawn);

                // Drop excess ranged weapon
                // Non-player colonist
                if (!pawn.Faction.IsPlayer && hasPrimary && priority == WorkPriority.Unloading && inventory.rangedWeaponList.Count >= 1)
                {
                    Thing ListGun = inventory.rangedWeaponList.Find(thing => thing.def != pawn.equipment.Primary.def);
                    if (ListGun != null)
                    {
                        inventory.container.TryDrop(ListGun, pawn.Position, pawn.Map, ThingPlaceMode.Near, ListGun.stackCount, out _);
                    }
                }

                // No primary weapon and no ranged weapon in inventory
                if (priority == WorkPriority.Weapon)
                {
                    // Find weapon for AI.
                    if (!pawn.Faction.IsPlayer)
                    {
                        Predicate<Thing> validatorWS = (Thing w) => w.def.IsWeapon
                            && w.MarketValue > 500 && pawn.CanReserve(w, 1)
                            && pawn.Position.InHorDistOf(w.Position, 25f)
                            && pawn.CanReach(w, PathEndMode.Touch, Danger.Deadly, true)
                            && (pawn.Faction.HostileTo(Faction.OfPlayer) || pawn.Faction == Faction.OfPlayer || !pawn.Map.areaManager.Home[w.Position]);

                        // generate a list of all weapons (this includes melee weapons)
                        List<Thing> allWeapons = (
                            from w in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
                            where validatorWS(w)
                            orderby w.MarketValue - w.Position.DistanceToSquared(pawn.Position) * 2f descending
                            select w
                            ).ToList();

                        // now just get the ranged weapons out...
                        List<Thing> rangedWeapons = allWeapons.Where(w => w.def.IsRangedWeapon).ToList();

                        if (!rangedWeapons.NullOrEmpty())
                        {
                            foreach (Thing thing in rangedWeapons)
                            {
                                if (inventory.CanFitInInventory(thing, out _))
                                {
                                    return new Job(JobDefOf.Equip, thing);
                                }
                            }
                        }

                        // else if no ranged weapons was found, lets consider a melee weapon.
                        if (allWeapons != null && allWeapons.Count > 0)
                        {
                            // since we don't need to worry about ammo, just pick one.
                            Thing meleeWeapon = allWeapons.FirstOrDefault(w => !w.def.IsRangedWeapon && w.def.IsMeleeWeapon);

                            if (meleeWeapon != null)
                            {
                                return new Job(JobDefOf.Equip, meleeWeapon);
                            }
                        }
                    }
                }

                /*
                if (!pawn.Faction.IsPlayer && pawn.apparel != null && priority == WorkPriority.Apparel)
                {
                    if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Torso))
                    {
                        Apparel apparel = this.FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.Torso);
                        if (apparel != null)
                        {
                            int numToapparel = 0;
                            if (inventory.CanFitInInventory(apparel, out numToapparel))
                            {
                                return new Job(JobDefOf.Wear, apparel)
                                {
                                    ignoreForbidden = true
                                };
                            }
                        }
                    }
                    if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Legs))
                    {
                        Apparel apparel2 = this.FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.Legs);
                        if (apparel2 != null)
                        {
                            int numToapparel2 = 0;
                            if (inventory.CanFitInInventory(apparel2, out numToapparel2))
                            {
                                return new Job(JobDefOf.Wear, apparel2)
                                {
                                    ignoreForbidden = true
                                };
                            }
                        }
                    }
                    if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.FullHead))
                    {
                        Apparel apparel3 = this.FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.FullHead);
                        if (apparel3 != null)
                        {
                            int numToapparel3 = 0;
                            if (inventory.CanFitInInventory(apparel3, out numToapparel3))
                            {
                                return new Job(JobDefOf.Wear, apparel3)
                                {
                                    ignoreForbidden = true,
                                    locomotionUrgency = LocomotionUrgency.Sprint
                                };
                            }
                        }
                    }
                }
                */
                return null;
            }
            return null;
        }

        /*
        private static Job GotoForce(Pawn pawn, LocalTargetInfo target, PathEndMode pathEndMode)
        {
            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, target, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), pathEndMode))
            {
                IntVec3 cellBeforeBlocker;
                Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                if (thing != null)
                {
                    Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true);
                    if (job != null)
                    {
                        return job;
                    }
                }
                if (thing == null)
                {
                    return new Job(JobDefOf.Goto, target, 100, true);
                }
                if (pawn.equipment.Primary != null)
                {
                    Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
                    if (primaryVerb.verbProps.ai_IsBuildingDestroyer && (!primaryVerb.verbProps.ai_IsIncendiary || thing.FlammableNow))
                    {
                        return new Job(JobDefOf.UseVerbOnThing)
                        {
                            targetA = thing,
                            verbToUse = primaryVerb,
                            expiryInterval = 100
                        };
                    }
                }
                return MeleeOrWaitJob(pawn, thing, cellBeforeBlocker);
            }
        }
        */

        private static bool hasWeaponInInventory(Pawn pawn)
        {
            Thing ListGun = pawn.TryGetComp<CompInventory>().rangedWeaponList.FirstOrDefault();
            if (ListGun != null)
            {
                //Log.Message("pawn: " + pawn.ThingID +  " gun: " + ListGun.ToString());
                return true;
            }
            return false;
        }

        public static int FindBattleWorthyEnemyPawnsCount(Map map, Pawn pawn)
        {
            if (pawn == null || pawn.Faction == null)
            {
                return 0;
            }
            IEnumerable<Pawn> pawns = map.mapPawns.FreeHumanlikesSpawnedOfFaction(pawn.Faction).Where(p => p.Faction != Faction.OfPlayer && !p.Downed);
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }

        private static bool Unload(Pawn pawn)
        {
            var inv = pawn.TryGetComp<CompInventory>();
            if (inv != null
            && !pawn.Faction.IsPlayer
            && (pawn.CurJob != null && pawn.CurJob.def != JobDefOf.Steal)
            && ((inv.capacityWeight - inv.currentWeight < 3f)))
            {
                return true;
            }
            else return false;
        }

        /*
        private Apparel FindGarmentCoveringPart(Pawn pawn, BodyPartGroupDef bodyPartGroupDef)
        {
            Room room = pawn.GetRoom();
            Predicate<Thing> validator = (Thing t) => pawn.CanReserve(t, 1) 
            && pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, true) 
            && (t.Position.DistanceToSquared(pawn.Position) < 12f || room == RegionAndRoomQuery.RoomAtFast(t.Position, t.Map));
            List<Thing> aList = (
                from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel)
                orderby t.MarketValue - t.Position.DistanceToSquared(pawn.Position) * 2f descending
                where validator(t)
                select t
                ).ToList();
            foreach (Thing current in aList)
            {
                Apparel ap = current as Apparel;
                if (ap != null && ap.def.apparel.bodyPartGroups.Contains(bodyPartGroupDef) && pawn.CanReserve(ap, 1) && ApparelUtility.HasPartsToWear(pawn, ap.def))
                {
                    return ap;
                }
            }
            return null;
        }
        */
    }
}
