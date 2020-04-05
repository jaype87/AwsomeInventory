using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using Verse;

namespace AwesomeInventory.UnitTest
{
    public class CallbackCoupling : TestAwesomeInventoryLoadout
    {
        private AwesomeInventoryLoadout _loadout = new AwesomeInventoryLoadout();
        private ThingGroupSelector _groupSelector;

        public override void Setup()
        {
            _groupSelector = new ThingGroupSelector(_thingDef);
            _groupSelector.Add(new SingleThingSelector(_thingDef));
            _loadout.Add(_groupSelector);
        }

        public override void Run(out bool result)
        {
            ThingSelector thingSelector = _loadout.First().First();
            ThingFilter thingFilter = (ThingFilter)typeof(ThingSelector)
                .GetField("_thingFilter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(thingSelector);

            Action settingsChangedCallback =
                (Action)typeof(ThingFilter)
                .GetField("settingsChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(thingFilter);

            Action<ThingFilter> _qualityAndHitpointsChangedCallback =
                (Action<ThingFilter>)typeof(ThingSelector)
                .GetField("_qualityAndHitpointsChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(thingSelector);

            Action<ThingSelector> _addNewThingSelectorCallback =
                (Action<ThingSelector>)typeof(ThingGroupSelector)
                .GetField("_addNewThingSelectorCallback", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_groupSelector);

            Action<ThingSelector> _removeThingSelectorCallback =
                (Action<ThingSelector>)typeof(ThingGroupSelector)
                .GetField("_removeThingSelectorCallback", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_groupSelector);

            Action<ThingGroupSelector, int> _stackCountChangedCallback =
                (Action<ThingGroupSelector, int>)typeof(ThingGroupSelector)
                .GetField("_stackCountChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_groupSelector);

            result =
                true
                &&
                AssertUtility.IsTrue(() => settingsChangedCallback != null, "Callback in ThingFilter should not be null.")
                &&
                AssertUtility.IsTrue(() => _qualityAndHitpointsChangedCallback != null, "Callback in ThingSelector should not be null.")
                &&
                AssertUtility.IsTrue(() => _addNewThingSelectorCallback != null, "_addNewThingSelectorCallback in ThingGroupSelector should not be null.")
                &&
                AssertUtility.IsTrue(() => _removeThingSelectorCallback != null, "_removeThingSelectorCallback in ThingGroupSelector should not be null.")
                &&
                AssertUtility.IsTrue(() => _stackCountChangedCallback != null, "_stackCountChangedCallback in ThingGroupSelector should not be null.");
        }
    }
}
