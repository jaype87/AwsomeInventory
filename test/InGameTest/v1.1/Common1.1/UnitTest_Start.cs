// <copyright file="UnitTest_Start.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using System.Threading;
using RimWorld;

namespace AwesomeInventory.UnitTest
{
    public class UnitTest_Start : GameComponent
    {
        public static AwesomeInventoryUnitTest Root = new UnitTest_Root();

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
            AwesomeInventoryUnitTest.Report(Root, ref sb);
            Log.Warning(sb.ToString());
        }
    }
}
