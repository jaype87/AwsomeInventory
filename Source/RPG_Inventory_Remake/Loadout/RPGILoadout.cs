using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RPGIResource;
using System.Diagnostics.CodeAnalysis;

namespace RPG_Inventory_Remake.Loadout
{
    /// <summary>
    /// It inherits from the "Outfit" class, is added to outfitDatabase and holds information about loadout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>It uses LoadoutCaomparer to generate hash value for keys</remarks>
    public class RPGILoadout : Outfit, IExposable, IEnumerable<Thing>
    {
        [Flags]
        enum DirtyBits
        {
            Read = 1,
            ReStash = 2,
            All = Read | ReStash
        }

        // Sole purpose of the _cachedList is for reordering in loadout window
        private List<Thing> _cachedList = new List<Thing>();
        private Dictionary<ThingStuffPairWithQuality, ThingFilterPackage> _loadoutDic = new Dictionary<ThingStuffPairWithQuality, ThingFilterPackage>();

        private float _weight = -1;
        private DirtyBits _dirty = DirtyBits.All;


        #region Constructor

        public RPGILoadout()
        {
#if (TEST == false)
            List<Outfit> outfits = Current.Game.outfitDatabase.AllOutfits;
            int id = (!outfits.Any()) ? 1 : (outfits.Max((Outfit o) => o.uniqueId) + 1);
            uniqueId = id;
            filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
            outfits.Add(this);
#endif
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
            AddItemFromIEnumerable(pawn.equipment?.AllEquipmentListForReading as IEnumerable<Thing>);
            AddItemFromIEnumerable(pawn.apparel?.WornApparel as IEnumerable<Thing>);
            AddItemFromIEnumerable(pawn.inventory?.innerContainer as IEnumerable<Thing>);
        }

        /// <summary>
        /// Copy constructor, except the label is different.
        /// </summary>
        /// <param name="loadout"></param>
        public RPGILoadout(RPGILoadout loadout) : this(LoadoutManager.GetIncrementalLabel(loadout.Label))
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }
            _loadoutDic = new Dictionary<ThingStuffPairWithQuality, ThingFilterPackage>();
            foreach (Thing thing in loadout.CachedList)
            {
                Thing item = thing.DeepCopySimple();
                _loadoutDic.Add(item.MakeThingStuffPairWithQuality(), new ThingFilterPackage(item, new ThingFilter()));
                _cachedList.Add(item);
            }
        }

        #endregion

        public ThingStuffPairWithQuality First()
        {
            return _loadoutDic.Keys.First();
        }

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
        public List<Thing> CachedList => _cachedList;

        public void ExposeData()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Thing> GetEnumerator()
        {
            return _loadoutDic.Values.Select(i => i.Thing).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _loadoutDic.Keys.GetEnumerator();
        }

        /// <summary>
        /// Return false if thing is null
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public bool Contains(Thing thing)
        {
            if (thing == null)
            {
                return false;
            }
            return Contains(thing.MakeThingStuffPairWithQuality());
        }

        public bool Contains(ThingStuffPairWithQuality pairWithQuality)
        {
            return _loadoutDic.ContainsKey(pairWithQuality);
        }

        public void SetDirtyAll()
        {
            _dirty = DirtyBits.All;
        }

        #region CRUD operation

        [SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers", Justification = "<Pending>")]
        public ThingFilterPackage this[ThingStuffPairWithQuality thing]
        {
            get => _loadoutDic[thing];
        }

        public bool TryGetThing(ThingStuffPairWithQuality pair, out Thing value)
        {
            bool result = _loadoutDic.TryGetValue(pair, out ThingFilterPackage package);
            value = package.Thing;
            return result;
        }

        public void AddItem(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                throw new ArgumentNullException(nameof(thingDef));
            }
            Thing thing = UtilityLoadouts.MakeThingSimple(new ThingStuffPairWithQuality(thingDef, null, QualityCategory.Normal));
            AddItem(thing);
        }

        public void AddItem(ThingStuffPairWithQuality pair)
        {
            _loadoutDic.Add(pair, new ThingFilterPackage(pair.MakeThing(), new ThingFilter()));
        }

        public void AddItem(Thing thing)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }
            ThingStuffPairWithQuality pair = thing.MakeThingStuffPairWithQuality();
            if (_loadoutDic.ContainsKey(pair))
            {
                _loadoutDic[pair].Thing.stackCount++;
            }
            else
            {
                _loadoutDic.Add(pair, new ThingFilterPackage(thing, new ThingFilter()));
                _cachedList.Add(thing);
            }
            _dirty = DirtyBits.All;
        }

        public void AddPackage(ThingFilterPackage package, int index = -1)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }
            ThingStuffPairWithQuality pair = package.Thing.MakeThingStuffPairWithQuality();
            if (_loadoutDic.ContainsKey(pair))
            {
                AddItem(package.Thing);
                return;
            }
            _loadoutDic.Add(pair, package);
            if (index > -1)
            {
                _cachedList.Insert(index, package.Thing);
                return;
            }
            _cachedList.Add(package.Thing);
        }

        public void RemoveItem(Thing thing)
        {
            if (thing == null)
            {
                return;
            }
            RemoveItem(thing.MakeThingStuffPairWithQuality(), thing);
        }

        public void RemoveItem(ThingStuffPairWithQuality pair)
        {
            if (!_loadoutDic.ContainsKey(pair))
            {
                return;
            }
            RemoveItem(pair, _loadoutDic[pair].Thing);
        }

        private void RemoveItem(ThingStuffPairWithQuality pair, Thing thing)
        {
            _loadoutDic.Remove(pair);
            _cachedList.Remove(thing);
            _dirty = DirtyBits.All;
        }

        public void UpdateItem(Thing thing, object target)
        {
            if (!Contains(thing))
            {
                throw new ArgumentOutOfRangeException(nameof(thing));
            }
            int index = _cachedList.IndexOf(thing);
            ThingFilterPackage package = _loadoutDic[thing.MakeThingStuffPairWithQuality()];
            if (target is QualityCategory qualityCategory)
            {
                RemoveItem(thing);
                thing.TryGetComp<CompQuality>()?.SetQuality(qualityCategory, default);
                AddPackage(package, index);
            }
            else if (target is ThingDef thingDef && thingDef.IsStuff)
            {
                RemoveItem(thing);
                thing.SetStuffDirect(thingDef);
                thing.HitPoints = thing.MaxHitPoints;
                AddPackage(package, index);
            }
            else
            {
                throw new ArgumentException(ErrorMessage.WrongArgumentType, nameof(target));
            }
        }

        private void AddItemFromIEnumerable(IEnumerable<Thing> list)
        {
            if (list == null)
            {
                return;
            }

            foreach (Thing t in list)
            {
                AddItem(UtilityLoadouts.DeepCopySimple(t));
            }
        }

        #endregion CRUD operation

        private void CheckDirtyForRead()
        {
            if ((_dirty & DirtyBits.Read) == DirtyBits.Read)
            {
                _weight = _loadoutDic.Values.Sum(i => i.Thing.GetStatValue(StatDefOf.Mass) * i.Thing.stackCount);
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
