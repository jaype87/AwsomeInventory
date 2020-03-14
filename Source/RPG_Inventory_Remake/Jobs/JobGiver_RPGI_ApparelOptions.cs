using System;
using System.Diagnostics.CodeAnalysis;
using Verse;
using Verse.AI;
using RimWorld;

namespace AwesomeInventory.Common
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Follow naming convention.")]
    public class JobGiver_AwsInv_DressAndUndress : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public static Job TryGiveJob(Pawn pawn, Apparel apparel)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (pawn.inventory != null && apparel != null)
            {
                Job job = new Job(AwesomeInventory_JobDefOf.RPGI_ApparelOptions)
                {
                    playerForced = true
                };
                return job;
            }
            return null;
        }
    }
}
