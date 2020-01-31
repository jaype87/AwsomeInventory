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
    public class RPGILoadout : Outfit, ILoadReferenceable, IExposable, IEnumerable<Thing>
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
        }

        public RPGILoadout(string oldLabel)
        {
            Label = LoadoutManager.GetIncrementalLabel(oldLabel);
            Init();
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
            AddItemFromIEnumerable(pawn.equipment?.AllEquipmentListForReading);
            AddItemFromIEnumerable(pawn.apparel?.WornApparel);
            AddItemFromIEnumerable(pawn.inventory?.innerContainer);
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
                ThingStuffPairWithQuality pair = item.MakeThingStuffPairWithQuality();
                _loadoutDic.Add(pair, new ThingFilterPackage(item, new ThingFilter()
                {
                    AllowedHitPointsPercents = loadout[pair].Filter.AllowedHitPointsPercents,
                    AllowedQualityLevels = loadout[pair].Filter.AllowedQualityLevels
                }));
                _cachedList.Add(item);
            }
            filter = loadout.filter;
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
        public List<Thing> CachedList => _cachedList;

        public void ExposeData()
        {
            base.ExposeData();
            Dictionary<SavePairFilter, ThingFilterPackage> dicToSave = new Dictionary<SavePairFilter, ThingFilterPackage>();
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

        public static RPGILoadout New()
        {
            RPGILoadout temp = new RPGILoadout();
            temp.Init();
            return temp;
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

        private void AddItemFromIEnumerable<T>(IEnumerable<T> list) where T : Thing
        {
            if (list == null)
            {
                return;
            }

            foreach (Thing t in list)
            {
                AddItem(t.DeepCopySimple());
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

    public class ThingFilterPackage : IExposable
    {
        public Thing Thing;
        public ThingFilter Filter;

        public ThingFilterPackage()
        {

        }


        public ThingFilterPackage(Thing thing, ThingFilter filter)
        {
            Thing = thing;
            Filter = filter;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref Filter, "Filter");
            Scribe_Deep.Look(ref Thing, "Thing");
        }
    }
}