// <copyright file="TakeDrug.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory
{
    /// <summary>
    /// A gizmo to order pawns to take drug.
    /// </summary>
    public class TakeDrug : Command_Action_Cacheable
    {
        private Pawn _pawn;

        private CompAwesomeInventoryLoadout _comp;

        private ThingDef _drugDef;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeDrug"/> class.
        /// </summary>
        /// <param name="pawn"> Pawn for this gizmo. </param>
        public TakeDrug(Pawn pawn)
        {
            _pawn = pawn;
            _comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();

            if (_comp != null && _comp.DrugToTake != null)
            {
                _drugDef = _comp.DrugToTake;
                this.icon = _drugDef.uiIcon;
                this.defaultDesc = UIText.Consume.Translate(_drugDef);
            }
            else
            {
                this.icon = TexResource.NoDrug;
                this.defaultDesc = UIText.NoDrugSelected.TranslateSimple();
            }

            this.action = TakeDrugAction;
        }

        /// <summary>
        /// Gets float menu options when right click on the gizmo.
        /// </summary>
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                if (_drugDef != null)
                {
                    yield return new FloatMenuOption(
                        UIText.ClearDrugSelection.TranslateSimple()
                        , () =>
                        {
                            if (_comp?.DrugToTake != null)
                            {
                                _drugDef = _comp.DrugToTake = null;
                                this.defaultDesc = UIText.NoDrugSelected.TranslateSimple();
                                this.icon = TexResource.NoDrug;
                            }
                        });
                }

                foreach (ThingDef def in ThingCategoryDefOf.Drugs.childThingDefs)
                {
                    yield return new FloatMenuOption(
                        def.LabelCap
                        , () =>
                        {
                            _drugDef = _comp.DrugToTake = def;
                            this.defaultDesc = UIText.Consume.Translate(_drugDef);
                            this.icon = _drugDef.uiIcon;
                        }
                        , MenuOptionPriority.Default
                        , extraPartWidth: FloatMenuOption.ExtraPartHeight
                        , extraPartOnGUI: (rect) =>
                        {
                            Widgets.ThingIcon(rect, def);
                            return false;
                        });
                }
            }
        }

        /// <inheritdoc/>
        public override void Refresh()
        {
        }

        private void TakeDrugAction()
        {
            if (_drugDef != null)
            {
                Thing thing = _pawn.inventory.innerContainer.FirstOrDefault(t => t.def == _drugDef);
                if (thing != null)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                    job.count = Math.Min(thing.stackCount, thing.def.ingestible.maxNumToIngestAtOnce);
                    _pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                else
                {
                    Messages.Message(UIText.NoDrugInInventory.Translate(_drugDef.label, _pawn.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                }
            }
        }
    }
}
