using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace RPG_Inventory_Remake
{
    public class JobGiver_RPGI_ApparelOptions : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public static Job TryGiveJob(Pawn pawn, Apparel apparel)
        {
            if (pawn.inventory != null && apparel != null)
            {
                Job job = new Job(RPGI_JobDefOf.RPGI_ApparelOptions)
                {
                    playerForced = true
                };
                return job;
            }
            return null;
        }
    }
}
