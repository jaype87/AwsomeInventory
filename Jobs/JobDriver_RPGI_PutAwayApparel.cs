using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace RPG_Inventory_Remake
{
    public class JobDriver_RPGI_PutAwayApparel : JobDriver
    {

        private int duration;
        private Apparel apparel;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            apparel = TargetThingA as Apparel;
            if (apparel != null)
            {
                duration = (int)(apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            yield return Toils_General.Wait(duration).WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil()
            {
                initAction = delegate
                {
                    pawn.apparel.Remove(apparel);
                    pawn.inventory.innerContainer.TryAdd(apparel);
                }
            };
        }
    }
}
