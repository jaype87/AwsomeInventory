using RimWorld;
using RPG_Inventory_Remake.Loadout;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake
{
    // TODO test if non-hostile NPC, caravan and wild man, will take weapons
    /// <summary>
    ///     Find pawn a suitable weapon. This JobGiver is ignored for colonists when Simple Sidearm is present.
    /// It is inserted to ThinkNode_SubtreesByTag with tag, Humanlike_PostMentalState. Check Humanlike.xml for more info
    /// </summary>
    public class JobGiver_RPGI_FindWeapon : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (GameComponent_RPGI_Main.HasSimpleSidearm && pawn.Faction.IsPlayer)
            {
                return null;
            }
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

            Log.Message("At start of FindWeapon");
            bool IsBrawler = pawn.story?.traits?.HasTrait(TraitDefOf.Brawler) ?? false;
            bool preferRanged = !IsBrawler && (pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= pawn.skills.GetSkill(SkillDefOf.Melee).Level
                                             || pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 6);
            bool hasPrimary = pawn.equipment.Primary != null;

            // Commnet Group 1
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

            // Commnet Group 1
            // Find and equip a suitable weapon
            Log.Message("Ready to FindWeapon");
            if (!hasPrimary)
            {
                Log.Message("Starting to FindWeapon");

                Thing closestWeapon = GenClosest.RegionwiseBFSWorker
                (pawn.Position
                , pawn.Map
                , ThingRequest.ForGroup(ThingRequestGroup.Weapon)
                , PathEndMode.OnCell
                , TraverseParms.For(pawn)
                , (Thing x) => pawn.CanReserve(x) && !x.IsBurning()
                , (Thing x) => preferRanged ? (x.def.IsRangedWeapon ? 2f : 1f) : (x.def.IsMeleeWeapon ? 2f : 1f)
                , 0, 30, 50, out int _
                );

                if (closestWeapon == null)
                {
                    return null;
                }
                Log.Message("Weapon found");

                return new Job(RPGI_JobDefOf.RPGI_Map_Equip, closestWeapon);
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
                if (MassUtility.FreeSpace(pawn) - newEq.GetStatValue(StatDefOf.Mass) > 0)
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
