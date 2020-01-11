using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Harmony;
using Verse;
using Verse.AI;
using System.Reflection;

namespace RPG_Inventory_Remake
{
    [StaticConstructorOnStartup]
    public class JobDriver_Cleanup_RPGI_Patch
    {
        static JobDriver_Cleanup_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(JobDriver), "Cleanup");
            MethodInfo postfix = AccessTools.Method(typeof(JobDriver_Cleanup_RPGI_Patch), "Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Postfix(JobCondition condition, JobDriver __instance)
        {
            if (__instance.job.def == RPGI_JobDefOf.RPGI_Unload)
            {
                JobGiver_RPGIUnload.JobInProgress = false;
                Pawn pawn = __instance.pawn;
                if (condition == JobCondition.Succeeded || condition == JobCondition.Incompletable)
                {
                    // Change unloaded item's compProperties unload to false
                    CompRPGIUnload comp = __instance.job.targetA.Thing?.TryGetComp<CompRPGIUnload>();
                    if (comp != null)
                    {
                        comp.Unload = false;
                    }

                    for (QueuedJob qj = pawn.jobs.jobQueue.FirstOrDefault(j => j.job.def == RPGI_JobDefOf.RPGI_Fake); qj != null;)
                    {
                        pawn.jobs.jobQueue.Extract(qj.job);
                        if (qj.job.targetA.Thing != null)
                        {
                            Job newJob = JobGiver_RPGIUnload.TryGiveJobStatic(pawn, qj.job.targetA.Thing);
                            if (newJob == null)
                            {
                                qj.job.targetA.Thing.TryGetComp<CompRPGIUnload>().Unload = false;
                                continue;
                            }
                            pawn.jobs.jobQueue.EnqueueFirst(newJob, JobTag.UnloadingOwnInventory);
                            return;
                        }
                    }
                    return;
                }
                else
                {
                    for (QueuedJob qj = pawn.jobs.jobQueue.FirstOrDefault(j => j.job.def == RPGI_JobDefOf.RPGI_Fake); qj != null;)
                    {
                        CompRPGIUnload comp = qj.job.targetA.Thing?.TryGetComp<CompRPGIUnload>();
                        if (comp != null)
                        {
                            comp.Unload = false;
                        }
                        pawn.jobs.jobQueue.Extract(qj.job);
                    }
                }
            }
        }
    }
}
