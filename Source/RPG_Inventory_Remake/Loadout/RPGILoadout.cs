using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RPGIResource;
using RPG_Inventory_Remake_Common;
using System.Diagnostics.CodeAnalysis;

namespace RPG_Inventory_Remake.Loadout
{
    /// <summary>
    /// It inherits from the "Outfit" class, is added to outfitDatabase and holds information about loadout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>It uses LoadoutCaomparer to generate hash value for keys</remarks>
    public class RPGILoadout : Outfit, ILoadReferenceable, IExposable, IEnumerable<Thing>, ICollection<Thing>
    {
        [Flags]
        enum DirtyBits
        {
            Read = 1,
            ReStock = 2,
            All = Read | ReStock
        }

        // Sole purpose of the _cachedList is for reordering in loadout window
        private float _weight = -1;
        private DirtyBits _dirty = DirtyBits.All;
        private List<Thing> _cachedList = new List<Thing>();
        private Dictionary<ThingStuffPairWithQuality, ThingFilterAll> _loadoutDic = new Dictionary<ThingStuffPairWithQuality, ThingFilterAll>();

        public delegate void CallBack(Thing thing, bool removed);
        public List<CallBack> CallbacksOnAddOrRemove = new List<CallBack>();

        #region Constructor

        public RPGILoadout()
        {
        }

        public RPGILoadout(string oldLabel)
        {
            if (oldLabel.NullOrEmpty())
            {
                throw new ArgumentNullException(nameof(oldLabel));
            }
            label = LoadoutManager.GetIncrementalLabel(oldLabel);
            Init();
        }

        /// <summary>
        /// Create loadout from pawn's current outfit
        /// </summary>
        /// <param name="pawn"></param>
        public RPGILoadout(Pawn pawn)
            : this(LoadoutManager.GetIncrementalLabel(pawn.GetLoadout() != null ? pawn.GetLoadout().label : pawn.GetDefaultLoadoutName()))
        {
            if (pawn == null)
            {
                throw new ArgumentNullException(nameof(pawn));
            }
            AddItemFromIEnumerable(pawn.equipment?.AllEquipmentListForReading);
            AddItemFromIEnumerable(pawn.apparel?.WornApparel);
            AddItemFromIEnumerable(pawn.inventory?.innerContainer);
        }

        /// <summary>
        /// Copy constructor, except the label is different.
        /// </summary>
        /// <param name="loadout"></param>
        public RPGILoadout(RPGILoadout loadout) : this(loadout?.label)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }
            _loadoutDic = new Dictionary<ThingStuffPairWithQuality, ThingFilterAll>();
            foreach (Thing thing in loadout.CachedList)
            {
                Thing item = thing.DeepCopySimple();
                ThingStuffPairWithQuality pair = item.MakeThingStuffPairWithQuality();
                _loadoutDic.Add(pair, new ThingFilterAll(item, loadout[pair]));
                _cachedList.Add(item);
            }
            filter.CopyAllowancesFrom(loadout.filter);
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
        public List<Thing> CachedList => _cachedList;

        public int Count => _cachedList.Count;

        public bool IsReadOnly => false;

        public new void ExposeData()
        {
            base.ExposeData();
            Dictionary<SavePairFilter, ThingFilterAll> dicToSave = new Dictionary<SavePairFilter, ThingFilterAll>();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var pair in _loadoutDic)
                {
                    dicToSave.Add(new SavePairFilter(pair.Key), pair.Value);
                }

            }

            Scribe_Collections.Look(ref dicToSave, nameof(dicToSave), LookMode.Deep, LookMode.Deep);
            Scribe_Collections.Look(ref _cachedList, nameof(_cachedList), LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (var pair in dicToSave)
                {
                    _loadoutDic.Add(pair.Key.Restore(), pair.Value);
                }
            }
            // TEST
#if TEST
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (var pair in _loadoutDic.ToList())
                {
                    if (pair.Key != pair.Value.Thing.MakeThingStuffPairWithQuality())
                    {
                        Log.Message("Mismatch: " + pair.Key.thing + " " + pair.Key.stuff + " " + pair.Key.Quality);
                    }
                }
                foreach (Thing thing in _cachedList)
                {
                    if (!_loadoutDic.TryGetValue(thing.MakeThingStuffPairWithQuality(), out _))
                    {
                        Log.Message("Can't find thing");
                    }
                }
            }
#endif
        }

        public IEnumerator<Thing> GetEnumerator()
        {
            return _loadoutDic.Values.Select(i => i.Thing).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _loadoutDic.Values.Select(i => i.Thing).GetEnumerator();
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
        public ThingFilterAll this[ThingStuffPairWithQuality thing]
        {
            get => _loadoutDic[thing];
        }


        [SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers", Justification = "<Pending>")]
        public ThingFilterAll this[Thing thing]
        {
            get => _loadoutDic[thing.MakeThingStuffPairWithQuality()];
        }

        public bool TryGetThing(ThingStuffPairWithQuality pair, out Thing value)
        {
            bool result = _loadoutDic.TryGetValue(pair, out ThingFilterAll package);
            if (result)
            {
                value = package.Thing;
            }
            else
            {
                value = null;
            }
            return result;
        }

        /// <summary>
        ///     Called when user chooses items from loadout window
        /// </summary>
        /// <param name="thingDef"></param>
        public void Add(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                throw new ArgumentNullException(nameof(thingDef));
            }
            Thing thing = LoadoutUtility.MakeThingSimple(new ThingStuffPairWithQuality(thingDef, null, QualityCategory.Normal));
            Add(thing, true);
        }

        /// <summary>
        ///     Implementation of adding item to loadout
        /// </summary>
        /// <param name="thing">the thing to add</param>
        /// <param name="owned">if the thing is owned by this loadout</param>
        public void Add(Thing thing, bool owned)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }
            ThingStuffPairWithQuality pair = thing.MakeThingStuffPairWithQuality();
            if (_loadoutDic.ContainsKey(pair))
            {
                _loadoutDic[pair].Thing.stackCount += thing.stackCount;
            }
            else
            {
                Thing toAdd = owned ? thing : thing.DeepCopySimple(true);

                _loadoutDic.Add(pair, new ThingFilterAll(toAdd));
                _cachedList.Add(toAdd);
            }
            _dirty = DirtyBits.All;
            InvokeCallbacks(thing, false);
        }

        public void Add(Thing item)
        {
            Add(item, false);
        }

        private void Add(ThingFilterAll thingFilter, int index)
        {
            _loadoutDic.Add(thingFilter.Thing.MakeThingStuffPairWithQuality(), thingFilter);
            _cachedList.Insert(index, thingFilter.Thing);
            InvokeCallbacks(thingFilter.Thing, false);
        }

        /// <summary>
        ///     Remove item from loadout regardless of stack count
        /// </summary>
        /// <param name="thing"></param>
        public bool Remove(Thing thing)
        {
            return Remove(thing, out Thing _);
        }

        public bool Remove(Thing thing, out Thing removed)
        {
            if (thing == null || !Contains(thing))
            {
                removed = null;
                return false;
            }
            removed = this[thing].Thing;
            _loadoutDic.Remove(thing.MakeThingStuffPairWithQuality());
            _cachedList.Remove(removed);
            _dirty = DirtyBits.All;

            InvokeCallbacks(removed, true);
            return true;
        }

        /// <summary>
        ///     Update quality and stuff. Stackcount is updated in Dialog_ManageLoadouts.cs
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="target"></param>
        public void UpdateItem(Thing thing, object target)
        {
            if (!Contains(thing))
            {
                throw new ArgumentOutOfRangeException(nameof(thing));
            }

            ThingFilterAll package = this[thing];
            Thing innerThing = package.Thing;
            int index = _cachedList.IndexOf(innerThing);

            if (target is QualityCategory qualityCategory)
            {
                if (innerThing.TryGetQuality(out QualityCategory qc))
                {
                    if (qualityCategory == qc)
                    {
                        return;
                    }
                    Remove(innerThing);
                    package.AllowedQualityLevelsWrapper
                        = new QualityRange(qualityCategory, package.AllowedQualityLevels.max);
                    Add(package, index);
                }
            }
            else if (target is ThingDef thingDef && thingDef.IsStuff)
            {
                Remove(innerThing);
                innerThing.SetStuffDirect(thingDef);
                innerThing.HitPoints = innerThing.MaxHitPoints;
                Add(package, index);
            }
            else
            {
                throw new ArgumentException(ErrorMessage.WrongArgumentType, nameof(target));
            }
        }

        private void AddItemFromIEnumerable<T>(IEnumerable<T> list) where T : Thing
        {
            if (list == null)
            {
                return;
            }

            foreach (Thing t in list)
            {
                Add(t, false);
            }
        }

        private void Init()
        {
            List<Outfit> outfits = Current.Game.outfitDatabase.AllOutfits;
            int id = (!outfits.Any()) ? 1 : (outfits.Max((Outfit o) => o.uniqueId) + 1);
            uniqueId = id;
            filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
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

        private void InvokeCallbacks(Thing thing, bool removed)
        {
            if (CallbacksOnAddOrRemove.Any())
            {
                foreach(var callback in CallbacksOnAddOrRemove)
                {
                    callback(thing, removed);
                }
            }
        }

        public void Clear()
        {
            _loadoutDic.Clear();
            _cachedList.Clear();
            CallbacksOnAddOrRemove.Clear();
        }

        public void CopyTo(Thing[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(array));
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentOutOfRangeException(nameof(array));

            for(int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = _cachedList[i];
            }
        }

        #endregion Method

        /// <summary>
        /// Used to save information in ThingStuffPairWithQuality
        /// </summary>
        private class SavePairFilter : IExposable
        {
            private ThingDef _thing;
            private ThingDef _stuff;
            private QualityCategory? _qualityCategory;

            public SavePairFilter()
            {

            }


            public void ExposeData()
            {
                Scribe_Defs.Look(ref _thing, nameof(_thing));
                Scribe_Defs.Look(ref _stuff, nameof(_stuff));
                Scribe_Values.Look(ref _qualityCategory, nameof(_qualityCategory));
            }

            public SavePairFilter(ThingStuffPairWithQuality pair)
            {
                _thing = pair.thing;
                _stuff = pair.stuff;
                _qualityCategory = pair.quality;
            }

            public ThingStuffPairWithQuality Restore()
            {
                return new ThingStuffPairWithQuality
                {
                    thing = _thing,
                    stuff = _stuff,
                    quality = _qualityCategory
                };
            }
        }
    }

    public class ThingFilterAll : ThingFilter
    {
        public Thing Thing;

        public ThingDef ThingDef
        {
            get => Thing.def;
            set
            {
                Thing.def = value;
            }

        }

        public ThingDef Stuff
        {
            get => Thing.Stuff;
            set
            {
                Thing.SetStuffDirect(value);
            }
        }

        public QualityCategory? Quality
        {
            get
            {
                if (Thing.TryGetQuality(out QualityCategory qc))
                {
                    return qc;
                }
                return null;
            }
            set
            {
                if (Thing.TryGetQuality(out QualityCategory _))
                {
                    if (value != null)
                    {
                        Thing.TryGetComp<CompQuality>().SetQuality((QualityCategory)value, ArtGenerationContext.Outsider);
                        AllowedQualityLevels = new QualityRange((QualityCategory)value, AllowedQualityLevels.max);
                    }
                    else
                    {
                        Thing.TryGetComp<CompQuality>().SetQuality(QualityCategory.Awful, ArtGenerationContext.Outsider);
                        AllowedQualityLevels = QualityRange.All;
                    }
                }
                else
                {
                    AllowedQualityLevels = QualityRange.All;
                }
            }
        }

        /// <summary>
        /// Keep AllowedQualityLevles and the quality set on thing in sync
        /// </summary>
        public QualityRange? AllowedQualityLevelsWrapper
        {
            get
            {
                if (Thing.def.FollowQualityThingFilter())
                {
                    return AllowedQualityLevels;
                }
                return null;
            }
            set
            {
                if (Thing.def.FollowQualityThingFilter())
                {
                    if (value != null)
                    {
                        QualityRange qualityRange = (QualityRange)value;
                        AllowedQualityLevels = qualityRange;
                        Thing.TryGetComp<CompQuality>()?.SetQuality(qualityRange.min, ArtGenerationContext.Outsider);
                    }
                }
                else
                {
                    AllowedQualityLevels = QualityRange.All;
                    Thing.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Awful, ArtGenerationContext.Outsider);
                }
            }
        }

        public ThingFilterAll()
        {

        }


        /// <summary>
        /// Initialize ThingDef, Stuff and Quality from values in thing.
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="filterToCopy"></param>
        /// <param name="pair"></param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public ThingFilterAll(Thing thing, ThingFilter filterToCopy)
        {
            ThrowHelper.ArgumentNullException(thing, filterToCopy);

            Thing = thing;
            CopyAllowancesFrom(filterToCopy);
            if (thing.TryGetQuality(out QualityCategory qc))
            {
                if (qc != filterToCopy.AllowedQualityLevels.min)
                {
                    throw new ArgumentException(string.Format(ErrorMessage.ValueNotMatch, qc, filterToCopy.AllowedQualityLevels.min));
                }
                AllowedQualityLevels = new QualityRange(qc, filterToCopy.AllowedQualityLevels.max);
            }
        }

        public ThingFilterAll(Thing thing)
        {
            Thing = thing ?? throw new ArgumentNullException(nameof(thing));

            if (thing.TryGetQuality(out QualityCategory qc))
            {
                AllowedQualityLevels = new QualityRange(qc, AllowedQualityLevels.max);
            }
        }

        public bool Allows(Thing thing, bool filterStuff = true)
        {
            if (thing == null)
            {
                Log.Error(string.Format(ErrorMessage.ArgumentIsNull, nameof(thing)));
                return false;
            }
            if (filterStuff)
            {
                return base.Allows(thing) && Stuff == thing.Stuff;
            }
            return base.Allows(thing);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Thing, "Thing");
        }
    }
}