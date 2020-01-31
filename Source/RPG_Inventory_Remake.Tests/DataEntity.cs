using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using RimWorld;
using Verse;
using RPG_Inventory_Remake.Loadout;

namespace RPG_Inventory_Remake.Tests
{
    public class RPGILoadoutEntity
    {

        public static Pawn MakePawn()
        {
            Pawn pawn = new Pawn();
            pawn.equipment = new Pawn_EquipmentTracker(pawn);
            pawn.apparel = new Pawn_ApparelTracker(pawn);
            pawn.inventory = new Pawn_InventoryTracker(pawn);
            return pawn;
        }
    }
}
