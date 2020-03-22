// <copyright file="UpdateQualityNoMerge.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class UpdateQualityNoMerge : Test_UpdateItem
    {
        private int _count;
        private Thing _innerThing;
        private List<Thing> _thingsNotUpdated;

        public override void Setup()
        {
            _innerThing = loadoutInstance[things[0]].Thing;
            _count = loadoutInstance.Count;
            _thingsNotUpdated = things.Where(t => t != things[0]).ToList();
        }

        public override void Run(out bool result)
        {
            loadoutInstance.UpdateItem(_innerThing, QualityCategory.Awful);
            _innerThing.TryGetQuality(out QualityCategory qc);

            result =
                // Check count
                AssertUtility.Expect(loadoutInstance.Count, _count, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                && // Check if _innerThing is re-inserted
                AssertUtility.Contains(loadoutInstance, _innerThing, nameof(loadoutInstance), _innerThing.LabelCapNoCount)
                && // Check quality is set correctly
                AssertUtility.Expect(qc, QualityCategory.Awful, nameof(qc))
                && // Check quality setting is in sync
                AssertUtility.AreEqual(qc, loadoutInstance[_innerThing].AllowedQualityLevels.min, nameof(qc), typeof(QualityRange).Name);
                ;

            // Check integrity of the rest
            foreach (Thing thing in _thingsNotUpdated)
            {
                result &= AssertUtility.Contains(loadoutInstance, thing, nameof(loadoutInstance), thing.LabelCapNoCount);
            }
        }

        public override void Cleanup()
        {
            loadoutInstance.Remove(_innerThing);
            loadoutInstance.Add(things[0]);
        }
    }
}
#endif