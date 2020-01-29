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
    public static class RPGILoadoutEntity
    {
        private static List<RPGILoadout> _loadouts;

        public static List<RPGILoadout> Loadouts
        {
            get
            {
                if (_loadouts == null)
                {

                }
                return _loadouts;
            }
        }


        public static Pawn MakePawn()
        {
            Pawn pawn = new Pawn();
            pawn.equipment = new Pawn_EquipmentTracker(pawn);
            pawn.apparel = new Pawn_ApparelTracker(pawn);
            pawn.inventory = new Pawn_InventoryTracker(pawn);
            return pawn;
        }

        private List<RPGILoadout> MakeLoaodut()
        {
            _loadouts = new List<RPGILoadout>();
            RPGILoadout 
        }
    }
}
