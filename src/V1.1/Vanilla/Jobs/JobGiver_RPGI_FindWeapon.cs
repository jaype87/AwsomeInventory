// <copyright file="JobGiver_RPGI_FindWeapon.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Jobs;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AwesomeInventory.Jobs
{
    // TODO test if non-hostile NPC, caravan and wild man, will take weapons

    /// <summary>
    ///     Find pawn a suitable weapon. This JobGiver is ignored for colonists when Simple Sidearm is present.
    /// It is inserted to ThinkNode_SubtreesByTag with tag, Humanlike_PostDuty. Check Humanlike.xml for more info.
    /// </summary>
    public class JobGiver_RPGI_FindWeapon : ThinkNode_JobGiver
    {
        /// <summary>
        /// Try to give a job to <paramref name="pawn"/>.
        /// </summary>
        /// <param name="pawn"> Pawn that will be assigned a job to. </param>
        /// <returns> A job assigned to <paramref name="pawn"/>. </returns>
        protected override Job TryGiveJob(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (!pawn.Faction.IsPlayer || GameComponent_AwesomeInventory_Entry.HasSimpleSidearm)
            {
                return null;
            }

            if (!pawn.RaceProps.Humanlike)
            {
                return null;
            }

            if (pawn.Drafted)
            {
                return null;
            }

            if (pawn.equipment == null)
            {
                return null;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }

            if (pawn.Map == null)
            {
                return null;
            }

            bool isBrawler = pawn.story?.traits?.HasTrait(TraitDefOf.Brawler) ?? false;
            bool preferRanged = !isBrawler && (pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= pawn.skills.GetSkill(SkillDefOf.Melee).Level
                                             || pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 6);
            bool hasPrimary = pawn.equipment.Primary != null;

            // Switch to better suited weapon
            if (pawn.inventory.innerContainer.Any())
            {
                IEnumerable<Thing> weapons = from thing in pawn.inventory.innerContainer.InnerListForReading
                                             where thing.def.IsWeapon
                                             select thing;

                ThingWithComps meleeWeapon = (ThingWithComps)weapons
                                                .Where(w => w.def.IsMeleeWeapon)
                                                .OrderBy(w => w.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS))
                                                .FirstOrDefault();
                ThingWithComps rangedWeapon = (ThingWithComps)weapons.FirstOrDefault(w => w.def.IsRangedWeapon);

                if (meleeWeapon == null && rangedWeapon == null)
                {
                    return null;
                }

                if (preferRanged)
                {
                    if (rangedWeapon != null)
                    {
                        if (!(hasPrimary && pawn.equipment.Primary.def.IsRangedWeapon))
                        {
                            TrySwitchToWeapon(rangedWeapon, pawn);
                        }
                    }
                }
                else
                {
                    if (!(hasPrimary && pawn.equipment.Primary.def.IsMeleeWeapon))
                    {
                        if (meleeWeapon != null)
                        {
                            TrySwitchToWeapon(meleeWeapon, pawn);
                        }
                    }
                }

                return null;
            }

            // Find and equip a suitable weapon
            if (!hasPrimary)
            {
                Thing closestWeapon = GenClosest.RegionwiseBFSWorker(
                    pawn.Position
                    , pawn.Map
                    , ThingRequest.ForGroup(ThingRequestGroup.Weapon)
                    , PathEndMode.OnCell
                    , TraverseParms.For(pawn)
                    , (Thing x) => pawn.CanReserve(x) && !x.IsBurning()
                    , (Thing x) => preferRanged ? (x.def.IsRangedWeapon ? 2f : 1f) : (x.def.IsMeleeWeapon ? 2f : 1f)
                    , 0
                    , 30
                    , Current.Game.CurrentMap.Size.LengthHorizontal / 7
                    , out int _);

                if (closestWeapon == null)
                {
                    return null;
                }

                return new Job(AwesomeInventory_JobDefOf.AwesomeInventory_MapEquip, closestWeapon);
            }

            return null;
        }

        private void TrySwitchToWeapon(ThingWithComps newEq, Pawn pawn)
        {
            if (newEq == null || pawn.equipment == null || !pawn.inventory.innerContainer.Contains(newEq))
            {
                return;
            }

            if (newEq.def.stackLimit > 1 && newEq.stackCount > 1)
            {
                newEq = (ThingWithComps)newEq.SplitOff(1);
            }

            if (pawn.equipment.Primary != null)
            {
                if (MassUtility.FreeSpace(pawn) > 0)
                {
                    pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.innerContainer);
                }
                else
                {
                    pawn.equipment.MakeRoomFor(newEq);
                }
            }

            pawn.equipment.GetDirectlyHeldThings().TryAddOrTransfer(newEq);
            if (newEq.def.soundInteract != null)
                newEq.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.MapHeld, false));
        }
    }
}
