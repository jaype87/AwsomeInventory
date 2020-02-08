using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public static class AssertUtility
    {
        public static bool Expect<T>(T Actual, T expected, string nameofActual)
        {
            if (EqualityComparer<T>.Default.Equals(Actual, expected))
            {
                return true;
            }
            else
            {
                Log.Error(
                    string.Format(
                        StringResource.ExpectedString, nameofActual, expected, Actual)
                    , true);
                return false;
            }
        }

        public static bool Contains<T>(T owner, object thing, string nameofOwner) where T : ICollection<object>
        {
            if (owner.Contains(thing))
            {
                return true;
            }
            else
            {
                Log.Error(
                    string.Format(
                        StringResource.ThingHas, nameofOwner, thing.GetType())
                    , true);
                return false;
            }
        }

        public static bool AreEqual(object A, object B, string nameA, string nameB)
        {
            if (A == B)
            {
                return true;
            }
            else
            {
                Log.Error(
                    string.Format(
                        StringResource.ObjectsAreNotEqual, nameA, A, nameB, B)
                    , true);
                return false;
            }
        }
    }
}
