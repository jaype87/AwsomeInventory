#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class UnitTest_Root : RPGIUnitTest
    {
        public override void Setup()
        {
            Tests.Add(new TestRPGILoadout());
            Tests.Add(new TestcompRPGILoadout());
        }
    }
}
#endif