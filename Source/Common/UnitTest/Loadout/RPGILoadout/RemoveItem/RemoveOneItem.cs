#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RPG_Inventory_Remake.Loadout;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class RemoveOneItem : Test_RemoveItem
    {
        private readonly int stackcount = loadoutInstance.Count;

        public override void Setup()
        {
        }

        public override void Run(out bool result)
        {
            result = true;
            for (int i = 0; i < things.Count; i++)
            {
                loadoutInstance.Remove(things[i], out Thing removed);
                List<Thing> remainings = things.Where(t => t != things[i]).ToList();

                result &=
                        // Check count
                        AssertUtility.Expect(loadoutInstance.Count, stackcount - 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                        &&
                        AssertUtility.Expect(loadoutInstance.CachedList.Count, stackcount - 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)))
                        && // Check integrity
                        AssertUtility.Expect(loadoutInstance.Contains(things[i]), false, string.Format(StringResource.ThingHas, nameof(loadoutInstance), things[i].LabelNoCount))
                        &&
                        AssertUtility.Expect(loadoutInstance.CachedList.Contains(removed), false, string.Format(StringResource.ThingHas, nameof(loadoutInstance.CachedList), things[i].LabelNoCount))
                        ;

                for (int j = 0; j < remainings.Count; j++)
                {
                    result &=
                        // Check integrity
                        AssertUtility.Expect(loadoutInstance.Contains(remainings[j]), true, string.Format(StringResource.ThingHas, nameof(loadoutInstance), remainings[j].LabelNoCount))
                        &&
                        AssertUtility.Expect(loadoutInstance.CachedList.Contains(loadoutInstance[remainings[j]].Thing), true, string.Format(StringResource.ThingHas, nameof(loadoutInstance.CachedList), remainings[j].LabelNoCount))
                        && // Check stackcount
                        AssertUtility.Expect(loadoutInstance[remainings[j]].Thing.stackCount, remainings[j].stackCount * 2, string.Format(StringResource.ExpectedString, remainings[j].LabelNoCount, 2, loadoutInstance[remainings[j]].Thing.stackCount))
                        ;
                }
                loadoutInstance.Add(things[i], false);
                loadoutInstance.Add(things[i], false);
            }
        }

        public override void Cleanup()
        {
        }
    }
}
#endif