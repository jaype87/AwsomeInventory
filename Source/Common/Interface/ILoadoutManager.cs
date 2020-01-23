using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake_Common
{
    interface ILoadoutManager<T> where T : Thing
    {
        IList<ILoadout<T>> Loadouts
        {
            get;
        }

        void AddLoadout(ILoadout<T> loadout);
    }
}
