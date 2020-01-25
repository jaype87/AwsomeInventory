using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake_Common
{
    interface ILoadout<T> where T : Thing
    {
        float Weight
        {
            get;
        }
        void AddItem(T thing);
        void RemoveItem(T thing);
        bool TryGetValue(T thing, out T value);
    }
}
