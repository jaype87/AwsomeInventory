// <copyright file="ChangeCostumeInPlace.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AwesomeInventory
{
    using HotSwapState = AwesomeInventory.Loadout.CompAwesomeInventoryLoadout.HotSwapState;

    /// <summary>
    /// A gizmo allows pawn to change to a predetermine costume.
    /// </summary>
    public class ChangeCostumeInPlace : Command_Action
    {
        private Pawn _pawn;
        private CompAwesomeInventoryLoadout _comp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeCostumeInPlace"/> class.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        public ChangeCostumeInPlace(Pawn pawn)
        {
            _pawn = pawn;

            _comp = _pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            if (_comp != null)
            {
                this.action = ChangeCostume;
                switch (_comp.HotswapState)
                {
                    case HotSwapState.Active:
                        this.icon = TexResource.ChangeClothActive;
                        break;
                    case HotSwapState.Inactive:
                        this.icon = TexResource.ChangeClothInactive;
                        break;
                    case HotSwapState.Interuppted:
                        this.icon = TexResource.ChangeClothInterrupted;
                        break;
                }

                this.defaultDesc = _comp.HotSwapCostume == null
                                ? UIText.CurrentHotSwapCostume.Translate(UIText.None.TranslateSimple())
                                : UIText.CurrentHotSwapCostume.Translate(_comp.HotSwapCostume.label);

                if (_comp.HotswapState == HotSwapState.Interuppted)
                {
                    this.defaultDesc = string.Concat(this.defaultDesc, Environment.NewLine, UIText.HotSwapInterrupted.TranslateSimple());
                }
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                if (_comp != null)
                {
                    if (_comp.HotSwapCostume != null && _comp.HotswapState == HotSwapState.Inactive && ApparelOptionUtility.CapableOfWearing(_pawn))
                    {
                        yield return new FloatMenuOption(
                            UIText.ForceHotSwap.TranslateSimple()
                            , () =>
                            {
                                ApparelOptionUtility.StopDressingJobs(_pawn);
                                _pawn.SetLoadout(_comp.HotSwapCostume);
                                _comp.HotswapState = HotSwapState.Active;
                            });
                    }

                    if (Find.Selector.SingleSelectedThing is Pawn && _comp.Loadout != null)
                    {
                        IEnumerable<AwesomeInventoryCostume> costumes = _comp.Loadout is AwesomeInventoryCostume aiCostume
                                                                         ? aiCostume.Base.Costumes
                                                                         : _comp.Loadout.Costumes;

                        foreach (AwesomeInventoryCostume costume in costumes)
                        {
                            yield return new FloatMenuOption(
                                costume.label
                                , () => _comp.HotSwapCostume = costume);
                        }

                        if (!costumes.Any())
                        {
                            yield return new FloatMenuOption(UIText.NoCostumeForHotSwap.TranslateSimple(), null);
                        }
                    }
                }
                else
                {
                    yield break;
                }
            }
        }

        private void ChangeCostume()
        {
            if (!ApparelOptionUtility.CapableOfWearing(_pawn))
            {
                Messages.Message(UIText.NotCapableChangingApparel.Translate(_pawn.NameShortColored), MessageTypeDefOf.NeutralEvent);
                return;
            }

            if (_comp.HotSwapCostume != null)
            {
                if (_comp.HotswapState == HotSwapState.Inactive)
                {
                    if (_comp.HotSwapCostume == _comp.Loadout)
                    {
                        Messages.Message(UIText.SameCostumeForHotSwap.TranslateSimple(), MessageTypeDefOf.NeutralEvent);
                        return;
                    }

                    if (_comp.Loadout != null)
                        _comp.LoadoutBeforeHotSwap = _comp.Loadout;

                    ApparelOptionUtility.StopDressingJobs(_pawn);
                    _pawn.SetLoadout(_comp.HotSwapCostume, true);
                    _pawn.jobs.jobQueue.EnqueueLast(JobMaker.MakeJob(AwesomeInventory_JobDefOf.AwesomeInventory_HotSwapStateChecker));

                    if (_pawn.jobs.jobQueue.Any() && _pawn.CurJobDef == JobDefOf.Wait_Combat)
                        _pawn.jobs.EndCurrentJob(JobCondition.Succeeded);

                    AwesomeInventorySoundDefOf.Interact_Rifle.PlayOneShot(new TargetInfo(_pawn.Position, _pawn.Map));
                    _comp.HotswapState = HotSwapState.Active;
                }
                else if (_comp.HotswapState == HotSwapState.Active)
                {
                    ApparelOptionUtility.StopDressingJobs(_pawn);
                    if (_comp.LoadoutBeforeHotSwap == null)
                        _comp.RemoveLoadout();
                    else
                        _pawn.SetLoadout(_comp.LoadoutBeforeHotSwap);

                    _comp.HotswapState = HotSwapState.Inactive;
                }
                else
                {
                    _pawn.SetLoadout(_comp.HotSwapCostume, forced: true);
                    if (_pawn.jobs.jobQueue.Any() && _pawn.CurJobDef == JobDefOf.Wait_Combat)
                        _pawn.jobs.EndCurrentJob(JobCondition.Succeeded);

                    AwesomeInventorySoundDefOf.Interact_Rifle.PlayOneShot(new TargetInfo(_pawn.Position, _pawn.Map));
                    _comp.HotswapState = HotSwapState.Active;
                }
            }
        }
    }
}
