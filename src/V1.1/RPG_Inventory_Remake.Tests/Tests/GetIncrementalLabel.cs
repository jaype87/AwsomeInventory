using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RPG_Inventory_Remake.Loadout;
using Verse;
using System.Diagnostics;

namespace RPG_Inventory_Remake.Tests
{
    [TestClass]
    public class GetIncrementalLabel
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        [DataSource(@"System.Data.SqlServerCe.4.0", @"Data Source=D:\Modding\RPG-Style-Inventory-CE-master\Source\RPG_Inventory_Remake.Tests\TestData\TestDB.sdf", "LoadoutManager", DataAccessMethod.Sequential)]
        public void Test()
        {
            string previousLabel = TestContext.DataRow[TestResource.Var1].ToString();
            string expected = TestContext.DataRow[TestResource.Expected].ToString();
            string result = LoadoutManager.GetIncrementalLabel(previousLabel);
            LoadoutManager.Loadouts.Add(new RPGILoadout() { label = result });
            Assert.AreEqual(expected, result);
        }

        [TestCleanup]
        [DataSource(@"System.Data.SqlServerCe.4.0", @"Data Source=D:\Modding\RPG-Style-Inventory-CE-master\Source\RPG_Inventory_Remake.Tests\TestData\TestDB.sdf", "LoadoutManager", DataAccessMethod.Sequential)]
        public void TestCleanup()
        {
            string previousLabel = TestContext.DataRow[TestResource.Var1].ToString();
            LoadoutManager.Loadouts.Add(new RPGILoadout() { label = previousLabel });
        }
    }
}
