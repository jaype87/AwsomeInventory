using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
#endif

namespace AwesomeInventory.Common.Loadout
{
    public class ThinkNode_RPGI_Core : ThinkNode_ConditionalColonist
    {
        protected override bool Satisfied(Pawn pawn)
        {
            Log.Message("In ThinkNode_RPGI_Core");
#if DEBUG
            bool needRestock = base.Satisfied(pawn) && (pawn.TryGetComp<CompRPGILoadout>()?.NeedRestock ?? false);
            Log.Message("Need restock: " + needRestock);
#endif
            return base.Satisfied(pawn) && (pawn.TryGetComp<CompRPGILoadout>()?.NeedRestock ?? false);
        }
    }
}
