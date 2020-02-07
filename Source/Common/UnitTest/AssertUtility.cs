using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public static class AssertUtility
    {
        public static bool Expect<T>(T A, T B, string nameA)
        {
            if (EqualityComparer<T>.Default.Equals(A, B))
            {
                return true;
            }
            else
            {
                Log.Error(
                    string.Format(
                        StringResource.ExpectedString, nameA, B, A)
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
