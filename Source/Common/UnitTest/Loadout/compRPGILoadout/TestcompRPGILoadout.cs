#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RPGIResource;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class TestcompRPGILoadout : RPGIUnitTest
    {
        public override void Setup()
        {
            Tests.Add(new Test_UpdateForNewLoadout());
        }
    }
}
#endif