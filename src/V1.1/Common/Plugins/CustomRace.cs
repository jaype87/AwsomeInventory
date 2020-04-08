// <copyright file="CustomRace.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// A wrapper allows modders to define logic to identify colonist for their races.
    /// </summary>
    public abstract class CustomRace : Plugin
    {
        /// <inheritdoc/>
        public override int ID { get; } = AwesomeInventoryServiceProvider.GetNextAvailablePluginID();

        /// <inheritdoc/>
        public override string DisplayName => string.Empty;

        /// <summary>
        /// Check if <paramref name="pawn"/> is a colonist.
        /// </summary>
        /// <param name="pawn"> Pawn to check. </param>
        /// <returns> Returns true if <paramref name="pawn"/> is a colonist. </returns>
        /// <remarks> It controls if the RPG-Style gear tab is visible to players. </remarks>
        public virtual bool IsColonist(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            return pawn.Faction != null && pawn.Faction.IsPlayer;
        }

        /// <summary>
        /// Check if <paramref name="pawn"/> is a colonist and can be controlled by player.
        /// </summary>
        /// <param name="pawn"> Pawn to check. </param>
        /// <returns> Returns true if <paramref name="pawn"/> is a colonist and can be controlled by player. </returns>
        /// <remarks> It controls if the Unload Now feature is available to pawns. </remarks>
        public virtual bool IsColonistPlayerControlled(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (this.IsColonist(pawn))
            {
                return pawn.HostFaction == null;
            }

            return false;
        }

        /// <summary>
        /// Register <paramref name="customRace"/> to Awesome Inventory's service provider.
        /// </summary>
        /// <param name="customRace"> Plugin to register. </param>
        protected static void Register(CustomRace customRace)
        {
            AwesomeInventoryServiceProvider.AddPlugIn(customRace);
        }
    }
}
