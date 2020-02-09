#if UnitTest
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class IntegrationTestHandler
    {
        public static Dictionary<Type, RPGIIntegrationTest> Tests;

        public static void SetFlag(TestFlags flag, Type type, object value)
        {
            Tests[type].SetFlag(flag, value);
        }
    }
}
#endif