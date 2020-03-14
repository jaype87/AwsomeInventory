using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using HarmonyLib;
using Verse;
using Verse.AI;
using System.Reflection;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake
{
    [StaticConstructorOnStartup]
    public class JobDriver_Cleanup_RPGI_Patch
    {
        static JobDriver_Cleanup_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(JobDriver), "Cleanup");
            MethodInfo postfix = AccessTools.Method(typeof(JobDriver_Cleanup_RPGI_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Postfix(JobCondition condition, JobDriver __instance)
        {
            // Change item's compProperties unload to false
            CompRPGIUnload comp = __instance.job.targetA.Thing?.TryGetComp<CompRPGIUnload>();
            if (comp != null)
            {
                comp.Unload = false;
            }
            if (__instance.job.def == RPGI_JobDefOf.RPGI_Unload)
            {
                JobGiver_RPGIUnload.JobInProgress = false;
                Pawn pawn = __instance.pawn;
                if (condition == JobCondition.Succeeded || condition == JobCondition.Incompletable)
                {
                    foreach (QueuedJob qj in pawn.jobs.jobQueue.Where(j => j.job.def == RPGI_JobDefOf.RPGI_Fake).ToList())
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
                    foreach (QueuedJob qj in pawn.jobs.jobQueue.Where(j => j.job.def == RPGI_JobDefOf.RPGI_Fake).ToList())
                    {
                        CompRPGIUnload comp1 = qj.job.targetA.Thing?.TryGetComp<CompRPGIUnload>();
                        if (comp1 != null)
                        {
                            comp1.Unload = false;
                        }
                        pawn.jobs.jobQueue.Extract(qj.job);
                    }
                }
            }
        }
    }
}
