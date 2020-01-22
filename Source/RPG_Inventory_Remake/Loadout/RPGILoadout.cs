using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake.Loadout
{
    /// <summary>
    /// It holds information about loadout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RPGILoadout<T> : IExposable, IEnumerable<T> where T : Thing, new()
    {
        enum DirtyBits
        {
            Read = 1,
            ReStash = 2,
            All = 3
        }
        
        // Hashset doesn't have TryGetValue() in .Net 3.5, it could get rid of _loadout
        // It acts as a cache for quick access to enumerator
        private HashSet<T> _loadoutHashSet = new HashSet<T>(new LoadoutComparer<T>());
        private Dictionary<T, T> _loadoutDic = new Dictionary<T, T>(new LoadoutComparer<T>());
        private float _weight = -1;
        private DirtyBits _dirty = DirtyBits.All;

        public string Label;

        #region Constructor

        public RPGILoadout()
        {

        }

        /// <summary>
        /// Create loadout from pawn's current outfit
        /// </summary>
        /// <param name="pawn"></param>
        public RPGILoadout(Pawn pawn)
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }
            AddItemFromIEnumerable(pawn.equipment?.AllEquipmentListForReading as IEnumerable<T>);
            AddItemFromIEnumerable(pawn.apparel?.WornApparel as IEnumerable<T>);
            AddItemFromIEnumerable(pawn.inventory?.innerContainer as IEnumerable<T>);

            Label = LoadoutManager.GetIncrementalLabel(pawn.GetLoadout() != null ? pawn.GetLoadout().Label : pawn.GetDefaultLoadoutName());
        }

        /// <summary>
        /// Copy constructor, except the label is different.
        /// </summary>
        /// <param name="loadout"></param>
        public RPGILoadout(RPGILoadout<T> loadout)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }
            _loadoutHashSet = new HashSet<T>(loadout, new LoadoutComparer<T>());
            _loadoutDic = loadout.ToDictionary((t) => t, new LoadoutComparer<T>());
            Label = LoadoutManager.GetIncrementalLabel(loadout.Label);
        }

        #endregion

        #region Properties

        public float Weight
        {
            get
            {
                CheckDirtyForRead();
                return _weight;
            }
        }

        #endregion

        #region Methods

        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static implicit operator HashSet<T>(RPGILoadout<T> t) => t._loadoutHashSet;

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

        /// <summary>
        /// Intersect the current loadout with other loadout
        /// </summary>
        /// <param name="other"></param>
        /// <returns>the ExceptWith result of current hashset to the other</returns>
        public IEnumerable<T> IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> exceptWith = new HashSet<T>(_loadoutHashSet, new LoadoutComparer<T>());
            exceptWith.ExceptWith(other);
            foreach (T thing in exceptWith)
            {
                _loadoutDic.Remove(thing);
            }

            _loadoutHashSet.IntersectWith(other);
            return exceptWith;
        }

        public void AddItem(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                throw new ArgumentNullException(nameof(thingDef));
            }
            T thing = new T() { def = thingDef };
            if (thingDef.useHitPoints)
            {
                thing.HitPoints = thing.MaxHitPoints;
            }
            AddItem(thing);
        }

        public void AddItem(T thing)
        {
            T item = _loadoutHashSet.FirstOrDefault(t => LoadoutComparer.isEqual(t, thing));
            if (item != default)
            {
                ++item.stackCount;
            }
            else
            {
                _loadoutHashSet.Add(thing);
                _loadoutDic.Add(thing, thing);
            }
            _dirty = DirtyBits.All;
        }

        public void RemoveItem(T thing)
        {
            _loadoutHashSet.Remove(thing);
            _loadoutDic.Remove(thing);
            _dirty = DirtyBits.All;
        }

        private void AddItemFromIEnumerable(IEnumerable<T> list)
        {
            if (list == null)
            {
                return;
            }

            foreach (T t in list)
            {
                AddItem(t);
            }
        }

        private void CheckDirtyForRead()
        {
            if ((_dirty & DirtyBits.Read) == DirtyBits.Read)
            {
                _weight = _loadoutHashSet.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
                _dirty ^= DirtyBits.Read;
            }
        }

        #endregion
    }
}
