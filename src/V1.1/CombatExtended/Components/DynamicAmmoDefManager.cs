// <copyright file="DynamicAmmoDefManager.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Manager for dynamically generated ammo def.
    /// </summary>
    public class DynamicAmmoDefManager : GameComponent
    {
        private List<GenericAmmo> _genericAmmoDefs = new List<GenericAmmo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicAmmoDefManager"/> class.
        /// </summary>
        /// <param name="game"> Game that is loaded. </param>
        public DynamicAmmoDefManager(Game game)
        {
        }

        /// <summary>
        /// Save/load dynamically generated ammo def.
        /// </summary>
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _genericAmmoDefs = DefDatabase<AIGenericDef>.AllDefs.OfType<GenericAmmo>().ToList();
            }

            Scribe_Collections.Look(ref _genericAmmoDefs, nameof(_genericAmmoDefs), LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (GenericAmmo genericAmmo in _genericAmmoDefs)
                {
                    if (!DefDatabase<AIGenericDef>.AllDefs.Any(def => def.defName == genericAmmo.defName))
                    {
                        DefDatabase<AIGenericDef>.Add(_genericAmmoDefs);
                    }
                }
            }
        }
    }
}
