using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
