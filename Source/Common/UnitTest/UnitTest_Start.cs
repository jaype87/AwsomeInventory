#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using System.Threading;
using RimWorld;
using Harmony;
using UnityEngine;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class UnitTest_Start : GameComponent
    {
        public UnitTest_Start(Game game)
        {

        }

        public static RPGIUnitTest Root = new UnitTest_Root();
        static UnitTest_Start()
        {
            Thread thread = new Thread((ThreadStart)
                delegate
                {
                    while (LongEventHandler.AnyEventNowOrWaiting)
                    {
                        Thread.Sleep(1000);
                    }
                    try
                    {
                        Root.Start();
                    }
                    catch(Exception e)
                    {
                        Log.Error(e.Message + e.StackTrace);
                    }

                    StringBuilder sb = new StringBuilder();
                    RPGIUnitTest.Report(Root, ref sb);
                    Log.Warning(sb.ToString());
                });
            thread.Start();
        }
    }
}
#endif