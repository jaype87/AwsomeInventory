using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace RPG_Inventory_Remake
{
    public class JobGiver_RPGI_OpportunisticHaul : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            List<WorkGiverDef> workGivers = WorkTypeDefOf.Hauling.workGiversByPriority;

            if (workGivers[0].Worker is WorkGiver_Scanner scanner)
            {
                bool predicate(Thing t) => !t.IsForbidden(pawn) && scanner.HasJobOnThing(pawn, t);
                IEnumerable<Thing> enumerable = scanner.PotentialWorkThingsGlobal(pawn);
                if (enumerable == null || enumerable.Any())
                {
                    return null;
                }
                Thing thing = GenClosest.ClosestThing_Global_Reachable
                    (pawn.Position
                    , pawn.Map
                    , enumerable
                    , scanner.PathEndMode
                    , TraverseParms.For(pawn, scanner.MaxPathDanger(pawn))
                    , JobGiver_FindItemByRadius<Thing>.Radius[0]
                    , predicate);
                if (thing != null)
                {
                    return scanner.JobOnThing(pawn, thing);
                }
            }
            return null;
        }
    }
}
