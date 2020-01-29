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

        static void Main()
        {
            Root_Entry entry = new Root_Entry();
            entry.Start();

        }


        [TestInitialize]
        public void Init()
        {
            Prefs.Init();
            PlayDataLoader.LoadAllPlayData();
        }

        [TestMethod]
        public void Test_Pawn()
        {
            PawnKindDef pawnKind = DefDatabase<PawnKindDef>.GetNamed("Mercenary_Gunner", true);
            Assert.IsTrue(true);
        }
    }
}
