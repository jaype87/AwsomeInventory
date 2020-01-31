using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RPG_Inventory_Remake.Loadout;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake.Tests
{
    [TestClass]
    public class RPGILoadoutTest
    {

        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public void Test_Pawn()
        {
            RPGILoadout loadout = new RPGILoadout();
            foreach (Apparel apparel in Maker<Apparel>.Make())
            {
                loadout.AddItem(apparel);
                Thing temp = loadout[apparel.MakeThingStuffPairWithQuality()].Thing;
                Assert.IsNotNull(temp);
                Assert.IsTrue(apparel.def.defName == temp.def.defName);
                Assert.IsTrue(apparel.Stuff.defName == temp.Stuff.defName);
                Assert.IsTrue(apparel.TryGetComp<CompQuality>().Quality == temp.TryGetComp<CompQuality>().Quality);
            }
        }
    }
}
