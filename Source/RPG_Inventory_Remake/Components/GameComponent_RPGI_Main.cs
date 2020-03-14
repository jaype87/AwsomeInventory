using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using RPG_Inventory_Remake_Common;
using System.Diagnostics.CodeAnalysis;

namespace RPG_Inventory_Remake
{
    public class GameComponent_RPGI_Main : GameComponent
    {
        public static bool HasSimpleSidearm = false;

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public GameComponent_RPGI_Main(Game game)
        {

        }
        public override void FinalizeInit()
        {
            JobGiver_RPGIUnload.JobInProgress = false;
            JobGiver_FindItemByRadius<Thing>.Reset();

            if (LoadedModManager.RunningModsListForReading.Any(m => m.Name == "Simple sidearms"))
            {
                HasSimpleSidearm = true;
            }

            Log.Warning("Process Id: " + Process.GetCurrentProcess().Id);
        }
    }
}
