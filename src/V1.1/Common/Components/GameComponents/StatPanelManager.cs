// <copyright file="StatPanelManager.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.UI;
using RimWorld;
using RimWorldUtility;
using Verse;

namespace AwesomeInventory
{
    public class StatPanelManager : GameComponent
    {
        private static bool _firstUse = true;

        public static CollectionNotifier<StatDef, HashSet<StatDef>> SelectedDefs = new CollectionNotifier<StatDef, HashSet<StatDef>>();

        public static CacheTable<StatCacheKey, CacheableTick<float>> StatCache =
            new CacheTable<StatCacheKey, CacheableTick<float>>(new StatCacheMaker());

        public StatPanelManager(Game game)
        {
        }

        public StatPanelManager()
        {
        }

        public static List<StatDef> StatDefs { get; }
            = DefDatabase<StatDef>.AllDefsListForReading
                .Where(ShowForPawn)
                .ToList();

        public static IEnumerable<StatCacheKey> GetStatCacheKeysFor(Pawn pawn)
        {
            return SelectedDefs.Data.Select(def => new StatCacheKey(pawn, def));
        }

        #region Overrides of GameComponent

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (_firstUse)
            {
                SelectedDefs.Add(StatDefOf.CarryingCapacity);
                _firstUse = false;
            }

            StatCache.Clear();
        }

        #endregion

        #region Implementation of IExposable

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                SelectedDefs.Data.Clear();

            HashSet<StatDef> selectedDefs = SelectedDefs.Data;

            Scribe_Collections.Look(ref selectedDefs, nameof(selectedDefs), LookMode.Def);
            Scribe_Values.Look(ref _firstUse, nameof(_firstUse));
            Scribe_Values.Look(ref StatPanel.InitSize, nameof(StatPanel.InitialSize));
            Scribe_Values.Look(ref StatPanel.IsOpen, nameof(StatPanel.IsOpen));

            if (Scribe.mode != LoadSaveMode.Saving)
                SelectedDefs.Data.AddRange(selectedDefs);
        }

        #endregion

        private static bool ShowForPawn(StatDef statDef)
        {
            StatCategoryDef category = statDef.category;
            return category == StatCategoryDefOf.PawnCombat
                   || category == StatCategoryDefOf.PawnMisc
                   || category == StatCategoryDefOf.PawnSocial
                   || category == StatCategoryDefOf.PawnWork;
        }
    }
}
