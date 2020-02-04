using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake
{
    public class JobGiver_RPGI_FindItems : JobGiver_FindItemByRadius<Thing>
    {
        public JobGiver_RPGI_FindItems() : base()
        {

        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            compRPGILoudout RPGIloadout = ((ThingWithComps)pawn).TryGetComp<compRPGILoudout>();
            Thing targetA = null;

            if (RPGIloadout == null || !RPGIloadout.NeedRestock)
                return null;

            foreach (Thing item in RPGIloadout.ItemsToRestock)
            {
                if (!item.def.IsApparel || !item.def.IsWeapon)
                {
                    ThingFilterAll filter = RPGIloadout.Loadout[item.MakeThingStuffPairWithQuality()];
                    
                    if (item.def is LoadoutGenericDef genericDef)
                    {
                        targetA = FindItem(pawn, null, genericDef.thingRequestGroup, (Thing thing) => genericDef.Validator(thing.def));
                    }
                    else if (targetA.Stuff == RPGI_StuffDefOf.RPGIGenericResource)
                    {
                        targetA = FindItem(pawn, item.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing, false));
                    }
                    else
                    {
                        targetA = FindItem(pawn, item.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing));
                    }
                }
            }
            if (targetA == null)
            {
                return null;
            }
            else
            {
                return new Job(JobDefOf.TakeInventory, targetA)
                {
                    // Check JobDriver_RPGI_ApparelOptions for details
                    count = 0
                };
            }
        }
    }
}
