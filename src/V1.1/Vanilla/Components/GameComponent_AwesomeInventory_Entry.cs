// <copyright file="GameComponent_AwesomeInventory_Entry.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AwesomeInventory.Jobs;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Initialize AwesomeInventory before a game start.
    /// </summary>
    public class GameComponent_AwesomeInventory_Entry : GameComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameComponent_AwesomeInventory_Entry"/> class.
        /// </summary>
        /// <param name="game"> Game about to start. </param>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Interface requirement. ")]
        public GameComponent_AwesomeInventory_Entry(Game game)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the game has loaded the SimpleSidearm mod.
        /// </summary>
        public static bool HasSimpleSidearm { get; private set; } = false;

        /// <summary>
        /// This method is called just before the game is ready to play.
        /// </summary>
        public override void FinalizeInit()
        {
            if (LoadedModManager.RunningModsListForReading.Any(m => m.Name == "Simple sidearms"))
            {
                HasSimpleSidearm = true;
            }
        }
    }
}
