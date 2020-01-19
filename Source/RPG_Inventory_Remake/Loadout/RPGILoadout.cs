using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake.RPGILoadout
{
    /// <summary>
    /// It holds information about loadout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RPGILoadout<T> : IExposable, IEnumerable<T> where T : Thing, new ()
    {
        // Hashset doesn't have TryGetValue() in .Net 3.5, it could get rid of _loadout
        private HashSet<T> _loadoutHashSet = new HashSet<T>(new LoadoutComparer<T>());
        private Dictionary<T, T> _loadoutDic = new Dictionary<T, T>(new LoadoutComparer<T>());

        #region Constructor

        public RPGILoadout()
        {
        }

        public RPGILoadout(IEnumerable<T> loadoutList)
        {
            _loadoutHashSet = new HashSet<T>(loadoutList, new LoadoutComparer<T>());
            _loadoutDic = loadoutList.ToDictionary((t) => t, new LoadoutComparer<T>());
        }

        #endregion

        #region Methods

        public void ExposeData()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _loadoutHashSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _loadoutHashSet.GetEnumerator();
        }

        public bool TryGetValue(T thing, out T value)
        {
            return _loadoutDic.TryGetValue(thing, out value);
        }

        public void AddItem(ThingDef thingDef)
        {
            T thing = new T() { def = thingDef };
            AddItem(thing);
        }

        public void AddItem(T thing)
        {
            _loadoutHashSet.Add(thing);
            _loadoutDic.Add(thing, thing);
        }

        #endregion
    }
}
