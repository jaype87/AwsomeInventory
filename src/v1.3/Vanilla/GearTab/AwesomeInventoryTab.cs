// <copyright file="AwesomeInventoryTab.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;

namespace AwesomeInventory.UI
{
    /// <inheritdoc/>
    [RegisterService(typeof(AwesomeInventoryTabBase), typeof(AwesomeInventoryTab))]
    public class AwesomeInventoryTab : AwesomeInventoryTabBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryTab"/> class.
        /// </summary>
        [Obsolete(ErrorText.NoDirectCall, false)]
        public AwesomeInventoryTab()
            : base()
        {
            _drawGearTab = new VanillaGearTabWorker(this);
        }
    }
}
