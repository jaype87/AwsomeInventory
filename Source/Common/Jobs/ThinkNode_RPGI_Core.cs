using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
#endif

namespace RPG_Inventory_Remake_Common
{
    public class ThinkNode_RPGI_Core : ThinkNode_ConditionalColonist
    {
        protected override bool Satisfied(Pawn pawn)
        {
            Log.Message("In ThinkNode_RPGI_Core");
#if DEBUG
            bool needRestock = base.Satisfied(pawn) && (pawn.TryGetComp<compRPGILoadout>()?.NeedRestock ?? false);
            Log.Message("Need restock: " + needRestock);
#endif
            return base.Satisfied(pawn) && (pawn.TryGetComp<compRPGILoadout>()?.NeedRestock ?? false);
        }
    }
}
