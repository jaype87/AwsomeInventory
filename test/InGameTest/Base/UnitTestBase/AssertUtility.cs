using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AwesomeInventory.UnitTest
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

        public static bool Contains<T>(ICollection<T> owner, T thing, string nameofOwner, string nameofThing)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            if (owner.Contains(thing))
            {
                return true;
            }
            else
            {
                Log.Error(
                    string.Format(
                        StringResource.ExpectedString
                        , string.Format(StringResource.ThingHas, nameofOwner, nameofThing)
                        , true
                        , false)
                    , true);
                return false;
            }
        }

        public static bool AreEqual<T>(T A, T B, string nameA, string nameB)
        {
            if (EqualityComparer<T>.Default.Equals(A, B))
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
