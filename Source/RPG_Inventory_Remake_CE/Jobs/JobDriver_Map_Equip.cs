using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace RPG_Inventory_Remake_CE
{
    public class JobDriver_Map_Equip : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Following code is an excerpt of a common practise in the vanilla code that assigns
            // variables to local variables. Given my limited knowledge on
            // Unity engine especially on how data is synchronized, this practise
            // is followed.
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            bool errorOnFailed2 = errorOnFailed;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return new Toil()
            {
                initAction = delegate
                {
                    ThingWithComps thingWithComps = (ThingWithComps)job.targetA.Thing;
                    ThingWithComps thingWithComps2 = null;
                    if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
                    {
                        thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
                    }
                    else
                    {
                        thingWithComps2 = thingWithComps;
                        thingWithComps2.DeSpawn();
                    }
                    // put away equiped weapon first
                    if (pawn.equipment.Primary != null)
                    {
                        if (!pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.innerContainer))
                        {
                            // if failed, drop the weapon
                            pawn.equipment.MakeRoomFor(thingWithComps2);
                        }
                    }
                    if (pawn.equipment.Primary == null)
                    {
                        // unregister new weapon in the inventory list and register it in equipment list 
                        pawn.equipment.GetDirectlyHeldThings().TryAddOrTransfer(thingWithComps2);
                    }
                    else
                    {
                        Messages.Message("CannotEquip".Translate(thingWithComps2.LabelShort), MessageTypeDefOf.NeutralEvent);
                    }
                }
            };
        }
    }
}
