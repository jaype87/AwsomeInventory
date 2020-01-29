using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            PawnKindDef pawnKind = DefDatabase<PawnKindDef>.GetNamed("Mercenary_Gunner", true);
            Assert.IsTrue(true);
        }
    }
}
