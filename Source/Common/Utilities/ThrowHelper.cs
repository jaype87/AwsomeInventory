using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPGIResource;
using Verse;

namespace RPG_Inventory_Remake_Common
{
    public static class ThrowHelper
    {
        public static void ArgumentNullException(params object[] list)
        {
            foreach (object obj in list)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(obj.ToString());
                }
            }
        }

        public static void LogNullArguments(params object[] list)
        {
            foreach (object obj in list)
            {
                if (obj == null)
                {
                    Log.Message(ErrorMessage.ArgumentIsNull + " " + obj.GetType().Name);
                }
            }
        }
    }
}
