// <copyright file="AwesomeInventoryTabBase.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using AwesomeInventory.Common.HarmonyPatches;
using AwesomeInventory.HarmonyPatches;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Replace RimWorld's default gear tab.
    /// </summary>
    public abstract class AwesomeInventoryTabBase : ITab_Pawn_Gear
    {
        public static MethodInfo InterfaceDrop { get; } =
            typeof(ITab_Pawn_Gear).GetMethod("InterfaceDrop", _nonPublicInstance);

        public static MethodInfo InterfaceIngest { get; } =
            typeof(ITab_Pawn_Gear).GetMethod("InterfaceIngest", _nonPublicInstance);

        public static PropertyInfo CanControlColonist { get; } =
            typeof(ITab_Pawn_Gear).GetProperty("CanControlColonist", _nonPublicInstance);

        /// <summary>
        /// Retrieve private property SelPawnForGear from <see cref="ITab_Pawn_Gear"/>.
        /// </summary>
        protected static readonly PropertyInfo _getPawn =
            typeof(ITab_Pawn_Gear).GetProperty("SelPawnForGear", _nonPublicInstance);

        /// <summary>
        /// Gets or sets a value indicating whether apparel has chagned.
        /// </summary>
        protected bool _apparelChanged;

        /// <summary>
        /// Draw contents in gear tab.
        /// </summary>
        protected IDrawGearTab _drawGearTab;

        private const BindingFlags _nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        // TODO set a static constructor to queue up unload jobs after game restarts
        private static bool _isJealous = true;
        private static bool _isGreedy = false;
        private static bool _isAscetic = false;

        private Pawn _selPawn;
        private object _apparelChangedLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryTabBase"/> class.
        /// </summary>
        public AwesomeInventoryTabBase()
        {
            this.size = new Vector2(550f, 500f);
            this.labelKey = "TabGear";
            this.tutorTag = "Gear";
            Pawn_ApparelTracker_ApparelChanged_Patch.ApparelChangedEvent +=
                (bool changed) =>
                {
                    lock (_apparelChangedLock)
                    {
                        _apparelChanged = changed;
                    }
                };
        }

        /// <summary>
        /// Action to take when the unload button is pressed.
        /// </summary>
        /// <param name="thing"> thing to unload. </param>
        /// <param name="pawn"> Pawn who carries <paramref name="thing"/>. </param>
        public static void InterfaceUnloadNow(ThingWithComps thing, Pawn pawn)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            // TODO examine HaulToContainer code path
            // If there is no comps in def, AllComps will always return an empty list
            // Can't add new comp if the parent class has no comp to begin with
            // The .Any() is not a fool proof test if some mods use it as a dirty way to
            // comps to things that should not have comps
            // Check ThingWithComps for more information
            if (thing.AllComps.Any())
            {
                CompRPGIUnload comp = thing.GetComp<CompRPGIUnload>();
                if (comp == null)
                {
                    thing.AllComps.Add(new CompRPGIUnload(true));
                    JobGiver_AwesomeInventory_Unload.QueueJob(pawn, JobGiver_AwesomeInventory_Unload.TryGiveJobStatic(pawn, thing));
                }
                else if (comp.Unload == true)
                {
                    // Check JobGiver_AwesomeInventory_Unload for more information
                    comp.Unload = false;
                    if (pawn.CurJob?.targetA.Thing == thing && pawn.CurJobDef == AwesomeInventory_JobDefOf.AwesomeInventory_Unload)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    QueuedJob queuedJob = pawn.jobs.jobQueue.FirstOrDefault(
                            j => j.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Fake &&
                            j.job.targetA.Thing == thing
                        );

                    if (queuedJob != null)
                    {
                        pawn.jobs.jobQueue.Extract(queuedJob.job);
                    }
                }
                else if (comp.Unload == false)
                {
                    comp.Unload = true;
                    JobGiver_AwesomeInventory_Unload.QueueJob(pawn, JobGiver_AwesomeInventory_Unload.TryGiveJobStatic(pawn, thing));
                }
            }
        }

        /// <summary>
        /// Run only once when the tab is toggle to open.
        /// Details in <see cref="InspectPaneUtility"/>.ToggleTab .
        /// </summary>
        /// <remarks>
        ///     The same instance is used when switch pawns with tab open.
        /// </remarks>
        public override void OnOpen()
        {
            base.OnOpen();
            _drawGearTab.RestScrollPosition();
        }

        /// <summary>
        /// It is called right before the tab is drawn.
        /// </summary>
        protected override void UpdateSize()
        {
        }

        /// <summary>
        /// Draw the tab.
        /// </summary>
        protected override void FillTab()
        {
            Pawn selPawn = _getPawn.GetValue((ITab_Pawn_Gear)this) as Pawn;

            // Reset scroll position if a new pawn is selected.
            if (_selPawn != selPawn)
            {
                _selPawn = selPawn;
                _drawGearTab.RestScrollPosition();
                lock (_apparelChangedLock)
                {
                    _apparelChanged = true;
                }
            }

            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Draw checkbox option for Jealous
            string translatedText = UIText.JealousTab.Translate();
            Rect headerRect = GetHeaderRect(GenUI.Gap, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isJealous))
            {
                _isGreedy = _isAscetic = false;
                _isJealous = true;
            }

            // Draw checkbox option for Greedy
            translatedText = UIText.GreedyTab.Translate();
            headerRect = GetHeaderRect(headerRect.xMax + GenUI.GapWide, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isGreedy))
            {
                _isJealous = _isAscetic = false;
                _isGreedy = true;
            }

            // Draw checkbox option for Ascetic
            translatedText = UIText.AsceticTab.Translate();
            headerRect = GetHeaderRect(headerRect.xMax + GenUI.GapWide, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isAscetic))
            {
                _isJealous = _isGreedy = false;
                _isAscetic = true;
            }

            lock (_apparelChangedLock)
            {
                if (Event.current.type != EventType.Layout)
                {
                    if (_isJealous)
                    {
                        AIDebug.Timer.Start();
                        _drawGearTab.DrawJealous(_selPawn, new Rect(0, headerRect.yMax, size.x, size.y - headerRect.yMax), _apparelChanged);
                        AIDebug.Timer.Stop(AIDebug.Header + Event.current.type.ToString());
                    }
                    else if (_isGreedy)
                    {
                        _drawGearTab.DrawGreedy(_selPawn, new Rect(0, headerRect.yMax, size.x, size.y - headerRect.yMax), _apparelChanged);
                    }
                    else if (_isAscetic)
                    {
                    }
                    else
                    {
                        throw new InvalidOperationException(Resources.ErrorMessage.NoDisplayOptionChosen);
                    }

                    _apparelChanged = false;
                }
            }
        }

        private Rect GetHeaderRect(float x, string translatedText)
        {
            float width = GenUI.GetWidthCached(translatedText) + Widgets.RadioButtonSize + GenUI.GapSmall;
            return new Rect(x, GenUI.GapSmall, width, GenUI.ListSpacing);
        }
    }
}
