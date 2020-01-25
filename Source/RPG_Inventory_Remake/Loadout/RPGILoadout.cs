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
    /// It inherits from the "Outfit" class, is added to outfitDatabase and holds information about loadout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>It uses LoadoutCaomparer to generate hash value for keys</remarks>
    public class RPGILoadout<T> : Outfit, IExposable, ILoadout<T>, IEnumerable<T> where T : Thing, new()
    {
        [Flags]
        enum DirtyBits
        {
            Read = 1,
            ReStash = 2,
            All = Read | ReStash
        }

        // Sole purpose of the _cachedList is for reordering in loadout window
        private List<T> _cachedList = new List<T>();
        private Dictionary<T, ThingFilterPackage> _loadoutDic = new Dictionary<T, ThingFilterPackage>(new LoadoutComparer<T>());

        private float _weight = -1;
        private DirtyBits _dirty = DirtyBits.All;


        #region Constructor

        public RPGILoadout()
        {
            List<Outfit> outfits = Current.Game.outfitDatabase.AllOutfits;
            int id = (!outfits.Any()) ? 1 : (outfits.Max((Outfit o) => o.uniqueId) + 1);
            uniqueId = id;
            filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
            outfits.Add(this);
        }

        public RPGILoadout(string oldLabel) : this()
        {
            Label = LoadoutManager.GetIncrementalLabel(oldLabel);
        }

        /// <summary>
        /// Create loadout from pawn's current outfit
        /// </summary>
        /// <param name="pawn"></param>
        public RPGILoadout(Pawn pawn)
            : this(LoadoutManager.GetIncrementalLabel(pawn.GetLoadout() != null ? pawn.GetLoadout().Label : pawn.GetDefaultLoadoutName()))
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }
            AddItemFromIEnumerable(pawn.equipment?.AllEquipmentListForReading as IEnumerable<T>);
            AddItemFromIEnumerable(pawn.apparel?.WornApparel as IEnumerable<T>);
            AddItemFromIEnumerable(pawn.inventory?.innerContainer as IEnumerable<T>);
        }

        /// <summary>
        /// Copy constructor, except the label is different.
        /// </summary>
        /// <param name="loadout"></param>
        public RPGILoadout(RPGILoadout<T> loadout) : this(LoadoutManager.GetIncrementalLabel(loadout.Label))
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }
            _loadoutDic = new Dictionary<T, ThingFilterPackage>(new LoadoutComparer<T>());
            foreach (Thing thing in loadout.CachedList)
            {
                T item = thing.DeepCopySimple() as T;
                _loadoutDic.Add(item, new ThingFilterPackage(item, new ThingFilter()));
                _cachedList.Add(item);
            }
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

        public string Label
        {
            get => label;
            set => label = value;
        }

        #endregion

        #region Methods
        public List<T> CachedList => _cachedList;

        public void ExposeData()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _loadoutDic.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _loadoutDic.Keys.GetEnumerator();
        }

        public bool Contains(T thing)
        {
            return _loadoutDic.ContainsKey(thing);
        }

        #region CRUD operation

        [SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers", Justification = "<Pending>")]
        public ThingFilterPackage this[Thing thing]
        {
            get => _loadoutDic[thing as T];
        }

        public bool TryGetValue(T thing, out T value)
        {
            bool result = _loadoutDic.TryGetValue(thing, out ThingFilterPackage package);
            value = package.Thing as T;
            return result;
        }

        public void AddItem(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                throw new ArgumentNullException(nameof(thingDef));
            }
            T thing = UtilityLoadouts.MakeThingSimple(thingDef, null) as T;
            AddItem(thing);
        }

        public void AddItem(T thing)
        {
            if (_loadoutDic.ContainsKey(thing))
            {
                _loadoutDic[thing].Thing.stackCount++;
            }
            else
            {
                _loadoutDic.Add(thing, new ThingFilterPackage(thing, new ThingFilter()));
                _cachedList.Add(thing);
            }
            _dirty = DirtyBits.All;
        }

        public void AddPackage(ThingFilterPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }
            if (_loadoutDic.ContainsKey(package.Thing as T))
            {
                AddItem(package.Thing as T);
                return;
            }
            _loadoutDic.Add(package.Thing as T, package);
            _cachedList.Add(package.Thing as T);
        }

        public void RemoveItem(T thing)
        {
            _loadoutDic.Remove(thing);
            _cachedList.Remove(thing);
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

        #endregion CRUD operation

        private void CheckDirtyForRead()
        {
            if ((_dirty & DirtyBits.Read) == DirtyBits.Read)
            {
                _weight = _loadoutDic.Keys.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
                _dirty ^= DirtyBits.Read;
            }
        }

        #endregion
    }

    public class ThingFilterPackage
    {
        public Thing Thing;
        public ThingFilter Filter;
        public ThingFilterPackage(Thing thing, ThingFilter filter)
        {
            this.Thing = thing;
            this.Filter = filter;
        }
    }
}
