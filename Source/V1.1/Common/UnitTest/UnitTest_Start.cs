#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using System.Threading;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class UnitTest_Start : GameComponent
    {
        public static RPGIUnitTest Root = new UnitTest_Root();

        public UnitTest_Start(Game game)
        {
        }

        public override void LoadedGame()
        {
            Start();
        }

        public override void StartedNewGame()
        {
            Start();
        }

        private void Start()
        {
            try
            {
                Root.Start();
            }
            catch (Exception e)
            {
                Log.Error(e.Message + e.StackTrace);
            }

            StringBuilder sb = new StringBuilder();
            RPGIUnitTest.Report(Root, ref sb);
            Log.Warning(sb.ToString());
        }
    }
}
#endif