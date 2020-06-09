// <copyright file="ErrorText.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory
{
    /// <summary>
    /// Provide strings for Error messages.
    /// </summary>
    public static class ErrorText
    {
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public const string ArmorStatNotInitialized = "Armor stat is not initiated.";

        public const string DrawHelperIsMissing = "Draw helper service is not found.";

        public const string SetMoreThanOneThingAllowed = "Setting thing selector to allow more than one thing.";

        public const string InventoryStackcountLessThanZero = "Stackcount of {0} in inventory is less than 0.";

        public const string NonExclusiveGenericStuffSource = "ThingGroupSelector has a generic stuff source and other stuff sources.";

        public const string CompareThingGroupSelectorOfTwoSpecifiedThing = "It is not supported to compare two ThingGroupSelectors with different AllowedThing other than generic defs.";

        public const string WrongTypeParentThinkNode = "Think node has a wrong type of parent node.";

        public const string NoDirectCall = "Should only call via AwesomeInventoryServiceProvider";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
    }
}
