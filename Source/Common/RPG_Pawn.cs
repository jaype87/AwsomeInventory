using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake_Common
{
    public class RPG_Pawn
    {
        public bool CanControl;
        public bool CanControlColonist;
        public Pawn Pawn;
        public IEnumerable<BodyPartGroupDef> bpgroups;

        public RPG_Pawn(Pawn selPawn, Thing selThing)
        {
            this.Pawn = SelectPawnForGear(selPawn, selThing);
            CanControl = CheckControl(selPawn);
            CanControlColonist = CheckControlColonist(selPawn);
            bpgroups = selPawn.RaceProps.body.AllParts
                .SelectMany(r => r.groups)
                .Distinct()
                .OrderByDescending(p => p.listOrder);
        }

        private bool CheckControl(Pawn selPawn)
        {
            return !selPawn.Downed &&
                    !selPawn.InMentalState &&
                    (selPawn.Faction == Faction.OfPlayer || selPawn.IsPrisonerOfColony) &&
                    (!selPawn.IsPrisonerOfColony || !selPawn.Spawned || selPawn.Map.mapPawns.AnyFreeColonistSpawned) &&
                    (!selPawn.IsPrisonerOfColony || (!PrisonBreakUtility.IsPrisonBreaking(selPawn) &&
                    (selPawn.CurJob == null || !selPawn.CurJob.exitMapOnArrival)));
        }

        private bool CheckControlColonist(Pawn selPawn)
        {
            return this.CanControl && selPawn.IsColonistPlayerControlled;
        }

        private Pawn SelectPawnForGear(Pawn selPawn, Thing selThing)
        {
            if (selPawn != null)
            {
                return selPawn;
            }
            Corpse corpse = selThing as Corpse;
            if (corpse != null)
            {
                return corpse.InnerPawn;
            }
            throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + selThing);
        }
    }
}
