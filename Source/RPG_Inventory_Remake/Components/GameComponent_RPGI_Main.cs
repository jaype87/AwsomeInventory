using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake
{
    public class GameComponent_RPGI_Main : GameComponent
    {
        public static bool HasSimpleSidearm = false;

        public GameComponent_RPGI_Main(Game game)
        {

        }
        public override void FinalizeInit()
        {
            JobGiver_RPGIUnload.JobInProgress = false;
            //JobGiver_UpdateInventory.ResetSearchRadius(Current.Game.InitData.mapSize);

            if (LoadedModManager.RunningModsListForReading.Any(m => m.Name == "Simple sidearms"))
            {
                HasSimpleSidearm = true;
            }
        }
    }
}
