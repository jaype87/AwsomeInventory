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
    public class JobGiver_RPGI_FindApparels : JobGiver_FindItemByRadius<Apparel>
    {
        public JobGiver_RPGI_FindApparels(): base()
        {

        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            compRPGILoudout RPGIloadout = ((ThingWithComps)pawn).TryGetComp<compRPGILoudout>();
            Apparel targetA = null;

            if (RPGIloadout == null || !RPGIloadout.NeedRestock)
                return null;

            foreach (Thing item in RPGIloadout.ItemsToRestock)
            {
                if (item is Apparel apparel)
                {
                    ThingFilterAll filter = RPGIloadout.Loadout[apparel.MakeThingStuffPairWithQuality()];
                    if (apparel.Stuff == RPGI_StuffDefOf.RPGIGenericResource)
                    {
                        targetA = FindItem(pawn, apparel.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing, false));
                    }
                    else
                    {
                        targetA = FindItem(pawn, apparel.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing));
                    }
                }
            }
            if (targetA == null)
                return null;
            else
            {
                return new Job(RPGI_JobDefOf.RPGI_ApparelOptions, targetA)
                {
                    // Check JobDriver_RPGI_ApparelOptions for details
                    count = 0
                };
            }
        }
    }
}
