// <copyright file="CompAwesomeInventoryLoadout.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Jobs;
using AwesomeInventory.UI;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Save loadout information and its current state.
    /// </summary>
    /// <remarks>
    ///     Another way to monitor things added or removed is to add a thingComp to every qualified thingDef
    /// and take advantage of the PreAbsorbStack(), PostSplitOff() function, etc..
    ///     The correct initiate state for this class is both Loadout and InventoryTracker are null. After moving
    /// to other states, none of them can be null.
    /// </remarks>
    public class CompAwesomeInventoryLoadout : ThingComp
    {
        private Pawn _pawn;
        private bool _initialized;

        private Dictionary<ThingGroupSelector, ThresholdState> _bottomThresholdLookup;
        private List<ThingGroupSelector> _thingSelectors;
        private List<ThresholdState> _thresholdStates;
        private List<Apparel> _apparelsBeforeChanged;

        private HotSwapState _hotswapActive = HotSwapState.Inactive;
        private AwesomeInventoryCostume _hotswapCostume;
        private AwesomeInventoryLoadout _loadoutBeforeHotSwap;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompAwesomeInventoryLoadout"/> class.
        /// </summary>
        public CompAwesomeInventoryLoadout()
        {
        }

        /// <summary>
        /// States of hot swap.
        /// </summary>
        public enum HotSwapState
        {
            /// <summary>
            /// Hot-swap costume is active.
            /// </summary>
            Active,

            /// <summary>
            /// Hot-swap action is interrupted.
            /// </summary>
            Interuppted,

            /// <summary>
            /// Hot-swap costume is inactive.
            /// </summary>
            Inactive,
        }

        /// <summary>
        /// Gets or sets what drug to take for the Take-Drug gizmo.
        /// </summary>
        public ThingDef DrugToTake { get; set; }

        /// <summary>
        /// Gets a dictionary acts as a inventory margin tracker. If the amount set in loadout is met, the margin is 0.
        /// Excessive amount has a positive margin, and vice versa.
        /// </summary>
        public Dictionary<ThingGroupSelector, int> InventoryMargins { get; private set; }

        /// <summary>
        /// Gets an <see cref="AwesomeInventoryLoadout"/> this comp holds.
        /// </summary>
        public AwesomeInventoryLoadout Loadout { get; private set; }

        /// <summary>
        /// Gets a value indicating whether loadout on this <see cref="Pawn"/> needs to restock.
        /// </summary>
        public bool NeedRestock
        {
            get
            {
                if (!AwesomeInventoryMod.Settings.UseLoadout || _initialized == false)
                {
                    return false;
                }

                if (this.Loadout == null)
                {
                    Log.Error("this.Loadout is out of sync with _initialized in CompAwesomeInventory. This message is harmless.");
                    this.RemoveLoadout();
                    return false;
                }
#if DEBUG
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(((Pawn)parent).Name + "'s inventory");
                stringBuilder.AppendLine();
                foreach (var item in InventoryMargins)
                {
                    stringBuilder.AppendFormat(ErrorMessage.ReportString, nameof(NeedRestock), item.Key.LabelCapNoCount, item.Value);
                    stringBuilder.AppendLine();
                }

                Log.Warning(stringBuilder.ToString());
#endif
                return InventoryMargins.Any(pair => this.ItemNeedsRestock(pair.Key));
            }
        }

        /// <summary>
        /// Gets a list of items that are needed to restock.
        /// </summary>
        public IEnumerable<KeyValuePair<ThingGroupSelector, int>> ItemsToRestock
        {
            get
            {
                if (!NeedRestock)
                {
                    yield break;
                }

                foreach (var item in InventoryMargins)
                {
                    if (this.ItemNeedsRestock(item.Key))
                    {
                        yield return item;
                    }
                }

                yield break;
            }
        }

        /// <summary>
        /// Gets or sets costume for hot swap.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required by design")]
        public AwesomeInventoryCostume HotSwapCostume { get => _hotswapCostume; set => _hotswapCostume = value; }

        /// <summary>
        /// Gets or sets loadout before hot swap.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required by design")]
        public AwesomeInventoryLoadout LoadoutBeforeHotSwap { get => _loadoutBeforeHotSwap; set => _loadoutBeforeHotSwap = value; }

        /// <summary>
        /// Gets or sets a value indicating whether hot-swap is active.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required by design")]
        public HotSwapState HotswapState { get => _hotswapActive; set => _hotswapActive = value; }

        /// <summary>
        /// Called by game code when the game starts.
        /// </summary>
        /// <param name="props"> Properties used for initializing this comp. </param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            _pawn = (Pawn)parent;
        }

        /// <summary>
        /// Called by the game root code post spawn setup.
        /// </summary>
        /// <param name="respawningAfterLoad"> True if the <see cref="ThingComp.parent"/> is respawned after load. </param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if ((respawningAfterLoad || this.Loadout == null) && parent is Pawn pawn)
            {
                if (pawn.outfits?.CurrentOutfit is AwesomeInventoryLoadout loadout)
                {
                    this.UpdateForNewLoadout(loadout, false, true);
                }
            }
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> are added.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that is added. </param>
        public void NotifiedAdded(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            // To avoid a situation where random apparels are assigned to newly created pawns when comps are not initialized.
            if (_initialized)
                this.Restock(thing);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> is added and merged.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has been added and merged. </param>
        /// <param name="mergedAmount"> Number of <paramref name="thing"/> that is added and merged. </param>
        public void NotifiedAddedAndMergedWith(Thing thing, int mergedAmount)
        {
            if (_initialized)
                this.Restock(thing, mergedAmount);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> has been removed.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has been removed. </param>
        public void NotifiedRemoved(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            if (_initialized)
                this.DeleteStock(thing);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> has split off.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has split off. </param>
        /// <param name="count"> Number of split <paramref name="thing"/>. </param>
        public void NotifiedSplitOff(Thing thing, int count)
        {
            if (_initialized)
                this.DeleteStock(thing, count);
        }

        /// <summary>
        /// Update <see cref="CompAwesomeInventoryLoadout.InventoryMargins"/> whenever a new loadout is assigned to pawn.
        /// </summary>
        /// <param name="newLoadout"> The new loadout assigned to pawn. </param>
        /// <param name="delay"> If true, pawn will queue changing costume jobs after the current job. </param>
        /// <param name="respawn"> If true, this method is invoked while the pawn is being respawn. </param>
        /// <param name="forced"> If true, update for <paramref name="newLoadout"/> even though it is the same as current loadout. </param>
        public void UpdateForNewLoadout(AwesomeInventoryLoadout newLoadout, bool delay = false, bool respawn = false, bool forced = false)
        {
            if (newLoadout == null || (this.Loadout == newLoadout && !forced))
                return;

            if (this.InventoryMargins == null)
            {
                this.InventoryMargins = new Dictionary<ThingGroupSelector, int>();
                _bottomThresholdLookup = new Dictionary<ThingGroupSelector, ThresholdState>();
            }
            else
            {
                this.InventoryMargins.Clear();
                _bottomThresholdLookup.Clear();
            }

            this.Loadout?.RemoveAddNewThingGroupSelectorCallback(this.AddNewThingGroupSelectorCallback);
            this.Loadout?.RemoveRemoveThingGroupSelectorCallback(this.RemoveThingGroupSelectorCallback);
            this.Loadout?.RemoveStackCountChangedCallback(this.StackCountChangedCallback);

            this.InitThreshold(newLoadout);
            this.UpdateInventoryMargin(newLoadout);

            newLoadout.AddAddNewThingGroupSelectorCallback(this.AddNewThingGroupSelectorCallback);
            newLoadout.AddRemoveThingGroupSelectorCallback(this.RemoveThingGroupSelectorCallback);
            newLoadout.AddStackCountChangedCallback(this.StackCountChangedCallback);

            AwesomeInventoryLoadout oldLoadout = this.Loadout;
            this.Loadout = newLoadout;
            _initialized = true;

            LoadoutManager.Comps.Add(this);
            this.ChangeCostume(newLoadout, oldLoadout, delay, respawn || forced);
        }

        /// <summary>
        /// Remove AI loadout from this pawn.
        /// </summary>
        public void RemoveLoadout()
        {
            this.Loadout?.RemoveAddNewThingGroupSelectorCallback(this.AddNewThingGroupSelectorCallback);
            this.Loadout?.RemoveRemoveThingGroupSelectorCallback(this.RemoveThingGroupSelectorCallback);
            this.Loadout?.RemoveStackCountChangedCallback(this.StackCountChangedCallback);

            this.Loadout = null;
            this.InventoryMargins = null;
            _bottomThresholdLookup = null;
            _initialized = false;
        }

        /// <summary>
        /// This function is invoked after the thing which owns this comp has finished ExposeData().
        /// </summary>
        public override void PostExposeData()
        {
            ThingDef drugDef = this.DrugToTake;

            base.PostExposeData();
            Scribe_Collections.Look(ref _bottomThresholdLookup, nameof(_bottomThresholdLookup), LookMode.Reference, LookMode.Deep, ref _thingSelectors, ref _thresholdStates);
            Scribe_Collections.Look(ref _apparelsBeforeChanged, nameof(_apparelsBeforeChanged), LookMode.Reference);
            Scribe_Values.Look(ref _hotswapActive, nameof(_hotswapActive), HotSwapState.Inactive);
            Scribe_References.Look(ref _hotswapCostume, nameof(_hotswapCostume));
            Scribe_References.Look(ref _loadoutBeforeHotSwap, nameof(_loadoutBeforeHotSwap));
            Scribe_Defs.Look(ref drugDef, nameof(this.DrugToTake));

            this.DrugToTake = drugDef;
        }

        /// <summary>
        /// Return gizmos(Icon buttons when a pawn/thing is selected).
        /// </summary>
        /// <returns> A list of gizmo. </returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            AwesomeInventorySetting setting = AwesomeInventoryMod.Settings;

            if (setting.UseToggleGizmo)
            {
                if (Find.Selector.SingleSelectedThing is Pawn pawn)
                {
                    if (AwesomeInventoryServiceProvider.TryGetImplementation<AwesomeInventoryTabBase>(out AwesomeInventoryTabBase tab))
                    {
                        if (tab.IsVisible)
                        {
                            ToggleGearTab toggleGearTab = new ToggleGearTab(tab.GetType());
                            yield return toggleGearTab;
                        }
                    }
                }
            }

            if (setting.UseLoadout && setting.UseHotSwap && _pawn.IsColonistPlayerControlled)
                yield return new ChangeCostumeInPlace(_pawn);

            if (setting.UseTakeDrugs && _pawn.IsColonistPlayerControlled)
                yield return Command_Action_Cacheable.Cache<TakeDrug>.Get(_pawn, _pawn);
        }

        /// <summary>
        /// Find <see cref="ThingGroupSelector"/> that allows <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Thing to check if there is any selector fits. </param>
        /// <param name="groupSelectors"> A list of fitting selectors. </param>
        /// <returns> A data packet that contains all information needed to find the best suited selector for <paramref name="thing"/>. </returns>
        public virtual ThingGroupSelectorPool FindPotentialThingGroupSelectors(Thing thing, IEnumerable<ThingGroupSelector> groupSelectors)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            return this.FindPotentialThingGroupSelectors(thing, thing.stackCount, groupSelectors);
        }

        /// <summary>
        /// Find <see cref="ThingGroupSelector"/> that allows <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Thing to check if there is any selector fits. </param>
        /// <param name="stackCount"> Stack count of <paramref name="thing"/>. </param>
        /// <param name="groupSelectors"> A list of fitting selectors. </param>
        /// <returns> A data packet that contains all information needed to find the best suited selector for <paramref name="thing"/>. </returns>
        public virtual ThingGroupSelectorPool FindPotentialThingGroupSelectors(Thing thing, int stackCount, IEnumerable<ThingGroupSelector> groupSelectors)
        {
            ValidateArg.NotNull(groupSelectors, nameof(groupSelectors));

            ThingGroupSelectorPool pool = new ThingGroupSelectorPool()
            {
                Thing = thing,
                StackCount = stackCount,
                OrderedSelectorTuples = new List<Tuple<ThingSelector, ThingGroupSelector>>(),
            };
            foreach (ThingGroupSelector groupSelector in groupSelectors)
            {
                if (groupSelector.Allows(thing, out ThingSelector thingSelector))
                    pool.OrderedSelectorTuples.Add(Tuple.Create(thingSelector, groupSelector));
            }

            if (pool.OrderedSelectorTuples.Count > 1)
                pool.OrderedSelectorTuples = pool.OrderedSelectorTuples.OrderBy(t => t.Item1, ThingSelectorComparer.Instance).ToList();

            return pool;
        }

        /// <summary>
        /// Change getup when switch loadout.
        /// </summary>
        /// <param name="newLoadout"> New loadout pawn switch to. </param>
        /// <param name="oldLoadout"> Loadout that is replaced with <paramref name="newLoadout"/>. </param>
        /// <param name="delay"> If true, pawn will queue changing costume jobs after the current job. </param>
        /// <param name="keepCache"> If false, <see cref="_apparelsBeforeChanged"/> will be set to null. </param>
        protected void ChangeCostume(AwesomeInventoryLoadout newLoadout, AwesomeInventoryLoadout oldLoadout, bool delay, bool keepCache)
        {
            ValidateArg.NotNull(newLoadout, nameof(newLoadout));

            if (newLoadout is AwesomeInventoryCostume costume && newLoadout != oldLoadout)
            {
                if (oldLoadout != null && oldLoadout.GetType() == typeof(AwesomeInventoryLoadout))
                {
                    _apparelsBeforeChanged = new List<Apparel>(_pawn.apparel.WornApparel);
                }
                else if (!keepCache)
                {
                    _apparelsBeforeChanged = null;
                }

                if (ApparelOptionUtility.CapableOfWearing(_pawn))
                {
                    //ConcurrentBag<Apparel> apparelToRemove = new ConcurrentBag<Apparel>();
                    //Parallel.ForEach(
                    //    Partitioner.Create(_pawn.apparel.WornApparel)
                    //    , (Apparel apparel) =>
                    //    {
                    //        if (!_pawn.outfits.forcedHandler.IsForced(apparel) && !costume.CostumeItems.Any(c => c.Allows(apparel, out _)))
                    //        {
                    //            apparelToRemove.Add(apparel);
                    //        }
                    //    });
                    //if (apparelToRemove.Any())
                    //{
                    //    StartUndressJobs(apparelToRemove, _pawn);
                    //}

                    if (costume.CostumeItems.Any())
                    {
                        ConcurrentBag<Thing> things = new ConcurrentBag<Thing>();
                        ConcurrentDictionary<Apparel, byte> apparelsToRemove = new ConcurrentDictionary<Apparel, byte>();

                        Parallel.ForEach(
                            Partitioner.Create(costume.CostumeItems)
                            , (ThingGroupSelector selector) =>
                            {
                                Thing thing = _pawn.inventory.innerContainer.FirstOrDefault(t => selector.Allows(t, out _));
                                if (thing != null)
                                {
                                    foreach (Apparel apparel in _pawn.apparel.WornApparel)
                                    {
                                        if (!ApparelUtility.CanWearTogether(apparel.def, thing.def, BodyDefOf.Human))
                                        {
                                            apparelsToRemove[apparel] = 1;
                                        }
                                    }

                                    things.Add(thing);
                                }
                            });

                        if (apparelsToRemove.Any())
                        {
                            StartUndressJobs(apparelsToRemove.Keys, _pawn);
                        }

                        if (things.Any())
                        {
                            if (!(_pawn.CurJobDef == AwesomeInventory_JobDefOf.AwesomeInventory_Undress || delay))
                                _pawn.jobs.StopAll(true);

                            foreach (Thing thing in things.Distinct())
                            {
                                if (thing.def.IsApparel)
                                {
                                    Job job = new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, thing, false);
                                    if (_pawn.CurJob == null)
                                        _pawn.jobs.StartJob(job);
                                    else
                                        _pawn.jobs.jobQueue.EnqueueLast(job);
                                }
                                else if (thing.def.IsWeapon)
                                {
                                    Job job = JobMaker.MakeJob(AwesomeInventory_JobDefOf.AwesomeInventory_MapEquip, thing);
                                    if (_pawn.CurJob == null)
                                        _pawn.jobs.StartJob(job);
                                    else
                                        _pawn.jobs.jobQueue.EnqueueLast(job);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Messages.Message(UIText.NotCapableChangingApparel.TranslateSimple(), MessageTypeDefOf.NeutralEvent);
                }
            }
            else if (oldLoadout is AwesomeInventoryCostume oldCostume && newLoadout.GetType() == typeof(AwesomeInventoryLoadout))
            {
                if (ApparelOptionUtility.CapableOfWearing(_pawn))
                {
                    if (_apparelsBeforeChanged != null)
                    {
                        ConcurrentBag<Apparel> apparelsToRemove = new ConcurrentBag<Apparel>();
                        Parallel.ForEach(
                            Partitioner.Create(_pawn.apparel.WornApparel)
                            , (Apparel apparel) =>
                            {
                                if (!_apparelsBeforeChanged.Contains(apparel)
                                    && !newLoadout.Any(s => s.Allows(apparel, out _))
                                    && !_pawn.outfits.forcedHandler.IsForced(apparel))
                                    apparelsToRemove.Add(apparel);
                            });

                        if (apparelsToRemove.Any())
                        {
                            StartUndressJobs(apparelsToRemove, _pawn);
                        }

                        if (!(_pawn.CurJobDef == AwesomeInventory_JobDefOf.AwesomeInventory_Undress || delay))
                            _pawn.jobs?.StopAll(true);

                        foreach (Apparel apparel1 in _apparelsBeforeChanged)
                        {
                            if (_pawn.inventory.innerContainer.Contains(apparel1))
                            {
                                Job job = new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, apparel1, false);
                                if (_pawn.CurJob == null)
                                    _pawn.jobs?.StartJob(job);
                                else
                                    _pawn.jobs?.jobQueue.EnqueueLast(job);
                            }
                        }

                        _apparelsBeforeChanged = null;
                    }
                }
                else
                {
                    Messages.Message(UIText.NotCapableChangingApparel.TranslateSimple(), MessageTypeDefOf.NeutralEvent);
                }
            }

            void StartUndressJobs(IEnumerable<Apparel> apparels, Pawn pawn)
            {
                if (!delay)
                    pawn.jobs.StopAll(true);

                foreach (Apparel apparel in apparels)
                {
                    Job job = JobMaker.MakeJob(AwesomeInventory_JobDefOf.AwesomeInventory_Undress, apparel);
                    if (pawn.CurJob == null)
                        pawn.jobs.StartJob(job);
                    else
                        pawn.jobs.jobQueue.EnqueueLast(job);
                }
            }
        }

        /// <summary>
        /// A callback to handle event where a new <see cref="ThingGroupSelector"/> is added to loadout.
        /// </summary>
        /// <param name="groupSelector"> The newly added selector. </param>
        protected virtual void AddNewThingGroupSelectorCallback(ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            if (groupSelector.UseBottomThreshold)
            {
                var state = new ThresholdState()
                {
                    NegBottomThresholdCount = groupSelector.BottomThresoldCount - groupSelector.AllowedStackCount,
                    CanRestock = true,
                };
                _bottomThresholdLookup[groupSelector] = state;
            }

            List<ThingGroupSelector> selectors = this.InventoryMargins
                .Where(pair => ThingDefComparer.Instance.Equals(pair.Key.AllowedThing, groupSelector.AllowedThing))
                .Select(pair => pair.Key)
                .ToList();

            selectors.Add(groupSelector);
            this.UpdateInventoryMargin(selectors);
            this.UpdateThreshold(new[] { groupSelector });
        }

        /// <summary>
        /// A callback to handle event where stack count is changed in <paramref name="groupSelector"/>.
        /// </summary>
        /// <param name="groupSelector"> Whose stack count has changed. </param>
        /// <param name="oldStackCount"> The stack count value before current stack count. </param>
        protected virtual void StackCountChangedCallback(ThingGroupSelector groupSelector, int oldStackCount)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            this.InventoryMargins[groupSelector] += oldStackCount - groupSelector.AllowedStackCount;
            this.UpdateThreshold(new[] { groupSelector });
        }

        /// <summary>
        /// A callback to handle event where a <see cref="ThingGroupSelector"/> is removed from loadout.
        /// </summary>
        /// <param name="groupSelector"> The selector that has been removed. </param>
        protected virtual void RemoveThingGroupSelectorCallback(ThingGroupSelector groupSelector)
        {
            this.InventoryMargins.Remove(groupSelector);
            _bottomThresholdLookup.Remove(groupSelector);
        }

        /// <summary>
        /// Keep <see cref="CompAwesomeInventoryLoadout.InventoryMargins" /> in sync with pawn's inventory.
        /// </summary>
        /// <param name="thing"> Thing that are being restocked. </param>
        protected virtual void Restock(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            if (thing.stackCount <= 0)
            {
                Log.Error(string.Concat(AIDebug.Header, "Thing count is equal to or less than 0, reset to 1"));
                thing.stackCount = 1;
            }

            this.Restock(thing, thing.stackCount);
        }

        /// <summary>
        /// Keep <see cref="CompAwesomeInventoryLoadout.InventoryMargins" /> in sync with pawn's inventory.
        /// </summary>
        /// <param name="thing"> Thing that are being restocked. </param>
        /// <param name="reStockCount"> Stack count to restock. </param>
        protected virtual void Restock(Thing thing, int reStockCount)
        {
            this.Restock(this.FindPotentialThingGroupSelectors(thing, reStockCount, this.InventoryMargins.Keys));
        }

        /// <summary>
        /// Keep <see cref="CompAwesomeInventoryLoadout.InventoryMargins" /> in sync with pawn's inventory.
        /// </summary>
        /// <param name="pool"> A data packet contains all necessary information. </param>
        protected virtual void Restock(ThingGroupSelectorPool pool)
        {
            int restockCount = pool.StackCount;
            foreach (var tuple in pool.OrderedSelectorTuples)
            {
                if (!tuple.Item2.UseBottomThreshold || _bottomThresholdLookup[tuple.Item2].CanRestock)
                {
                    if (this.InventoryMargins[tuple.Item2] + restockCount <= 0)
                    {
                        this.InventoryMargins[tuple.Item2] += restockCount;
                        if (this.InventoryMargins[tuple.Item2] == 0)
                            this.UpdateThreshold(new[] { tuple.Item2 });

                        restockCount = 0;
                        break;
                    }
                    else
                    {
                        restockCount += this.InventoryMargins[tuple.Item2];
                        this.InventoryMargins[tuple.Item2] = 0;
                        this.UpdateThreshold(new[] { tuple.Item2 });
                    }
                }
            }

            if (pool.OrderedSelectorTuples.Any() && restockCount != 0)
            {
                this.InventoryMargins[pool.OrderedSelectorTuples.First().Item2] += restockCount;
            }
        }

        /// <summary>
        /// Remove thing from <see cref="CompAwesomeInventoryLoadout.InventoryMargins"/>.
        /// </summary>
        /// <param name="thing"> Thing to remove. </param>
        protected virtual void DeleteStock(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            this.DeleteStock(thing, thing.stackCount);
        }

        /// <summary>
        /// Remove thing from <see cref="CompAwesomeInventoryLoadout.InventoryMargins"/>.
        /// </summary>
        /// <param name="thing"> Thing to remove. </param>
        /// <param name="stackCountToDelete"> Stack count to remove. </param>
        protected virtual void DeleteStock(Thing thing, int stackCountToDelete)
        {
            ThingGroupSelectorPool pool = this.FindPotentialThingGroupSelectors(thing, stackCountToDelete, this.InventoryMargins.Keys);
            foreach (var tuple in pool.OrderedSelectorTuples)
            {
                int maxNegativeMargin = tuple.Item2.AllowedStackCount * -1;
                if (this.InventoryMargins[tuple.Item2] - pool.StackCount < maxNegativeMargin)
                {
                    pool.StackCount -= this.InventoryMargins[tuple.Item2] - maxNegativeMargin;
                    this.InventoryMargins[tuple.Item2] = maxNegativeMargin;
                    this.UpdateThreshold(new[] { tuple.Item2 });
                }
                else
                {
                    this.InventoryMargins[tuple.Item2] -= pool.StackCount;
                    this.UpdateThreshold(new[] { tuple.Item2 });
                    break;
                }
            }

            // Rebalance inventory margin for cases where bottom threshold in ThingGroupSelectors are set to CanRestock in the code above.
            if (pool.OrderedSelectorTuples.Any(t => t.Item2.UseBottomThreshold && _bottomThresholdLookup[t.Item2].CanRestock))
            {
                this.UpdateInventoryMargin(pool.OrderedSelectorTuples.Select(t => t.Item2));
            }
        }

        private void UpdateInventoryMargin(IEnumerable<ThingGroupSelector> groupSelectors)
        {
            ValidateArg.NotNull(groupSelectors, nameof(groupSelectors));

            if (!groupSelectors.Any())
                return;

            foreach (ThingGroupSelector groupSelector in groupSelectors)
            {
                this.InventoryMargins[groupSelector] = groupSelector.AllowedStackCount * -1;
            }

            ConcurrentBag<ThingGroupSelectorPool> pools = new ConcurrentBag<ThingGroupSelectorPool>();
            Parallel.ForEach(
                Partitioner.Create(InventoryUtility.MakeListForPawnGearAndInventory(_pawn)),
                (Thing thing) =>
                {
                    ThingGroupSelectorPool pool = this.FindPotentialThingGroupSelectors(thing, groupSelectors);

                    if (pool.OrderedSelectorTuples.Any())
                        pools.Add(pool);
                });

            foreach (ThingGroupSelectorPool pool in pools)
            {
                this.Restock(pool);
            }
        }

        // Make sure margin is up-to-date before calling this method.
        private void UpdateThreshold(IEnumerable<ThingGroupSelector> groupSelectors)
        {
            if (!groupSelectors.Any())
                return;

            foreach (ThingGroupSelector groupSelector in groupSelectors)
            {
                if (groupSelector.UseBottomThreshold)
                {
                    if (_bottomThresholdLookup.TryGetValue(groupSelector, out ThresholdState state))
                    {
                        // Only when margin rise from NegBottomThresholdCount to 0 would CanRestock be true,
                        // otherwise, when margin falls from 0 to NegBottomThresholdCount, it is false.
                        if (this.InventoryMargins[groupSelector] >= 0)
                        {
                            state.CanRestock = false;
                            state.NegBottomThresholdCount = groupSelector.BottomThresoldCount - groupSelector.AllowedStackCount;
                            _bottomThresholdLookup[groupSelector] = state;
                        }
                        else
                        {
                            state.NegBottomThresholdCount = groupSelector.BottomThresoldCount - groupSelector.AllowedStackCount;
                            state.CanRestock = state.NegBottomThresholdCount >= this.InventoryMargins[groupSelector] || state.CanRestock;
                            _bottomThresholdLookup[groupSelector] = state;
                        }
                    }
                    else
                    {
                        state = new ThresholdState
                        {
                            NegBottomThresholdCount = groupSelector.BottomThresoldCount - groupSelector.AllowedStackCount,
                        };
                        state.CanRestock = this.InventoryMargins[groupSelector] <= state.NegBottomThresholdCount;
                        _bottomThresholdLookup[groupSelector] = state;
                    }
                }
                else
                {
                    _bottomThresholdLookup.Remove(groupSelector);
                }
            }
        }

        /// <summary>
        /// Initialize bottom thresholds when pawn is spawned.
        /// </summary>
        /// <param name="groupSelectors"> Selectors from loadout. </param>
        /// <remarks> This function is to make old saves compatible. </remarks>
        private void InitThreshold(IEnumerable<ThingGroupSelector> groupSelectors)
        {
            if (_bottomThresholdLookup.Any())
                return;

            foreach (ThingGroupSelector selector in groupSelectors)
            {
                if (selector.UseBottomThreshold)
                {
                    _bottomThresholdLookup[selector] = new ThresholdState()
                    {
                        NegBottomThresholdCount = selector.BottomThresoldCount - selector.AllowedStackCount,
                        CanRestock = true,
                    };
                }
            }
        }

        private bool ItemNeedsRestock(ThingGroupSelector groupSelector)
        {
            return this.InventoryMargins[groupSelector] < 0
                && (_bottomThresholdLookup.TryGetValue(groupSelector, out ThresholdState state)
                   ? state.CanRestock
                   : true);
        }

        private class ThresholdState : IExposable
        {
            /// <summary>
            /// This field is toggle to true when inventory level drops to BottomThresoldCount and
            /// will not toggle to false until corresponding inventory margin reaches 0.
            /// </summary>
            public bool CanRestock;

            public int NegBottomThresholdCount;

            /// <summary>
            /// Save states.
            /// </summary>
            public void ExposeData()
            {
                Scribe_Values.Look(ref this.CanRestock, nameof(this.CanRestock), forceSave: true);
                Scribe_Values.Look(ref this.NegBottomThresholdCount, nameof(this.NegBottomThresholdCount), forceSave: true);
            }
        }

        /// <summary>
        /// A datat structure thtat contains necessary information for <see cref="CompAwesomeInventoryLoadout.InventoryMargins"/> to update.
        /// </summary>
        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not in design.")]
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Convention")]
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Convention")]
        public struct ThingGroupSelectorPool
        {
            /// <summary>
            /// The thing for inventory operation.
            /// </summary>
            public Thing Thing;

            /// <summary>
            /// Count of thing that are being added to or removed from inventory.
            /// </summary>
            public int StackCount;

            /// <summary>
            /// A list sorted by <see cref="ThingSelector"/>'s criteria strictness in desending order.
            /// </summary>
            public List<Tuple<ThingSelector, ThingGroupSelector>> OrderedSelectorTuples;
        }
    }
}