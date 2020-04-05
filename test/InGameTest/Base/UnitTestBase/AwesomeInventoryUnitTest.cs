// <copyright file="AwesomeInventoryUnitTest.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AwesomeInventory.UnitTest
{
    public abstract class AwesomeInventoryUnitTest
    {
        string FullName;
        public List<AwesomeInventoryUnitTest> Tests = new List<AwesomeInventoryUnitTest>();
        public Dictionary<string, bool> TestResults = new Dictionary<string, bool>();

        public AwesomeInventoryUnitTest()
        {
            FullName = GetType().FullName;
        }

        public virtual void Setup() { }
        public virtual void Cleanup() { }

        public virtual void Run(out bool result)
        {
            if (Tests.Any())
            {
                foreach (AwesomeInventoryUnitTest test in Tests)
                {
                    result = test.Start();
                    TestResults.Add(test.GetType().FullName, result);
                }
            }
            if (TestResults.Any() && TestResults.Values.All(b => b == true))
            {
                result = true;
            }
            else
            {
                result = false;
            }
        }

        public virtual bool Start()
        {
            try
            {
                Setup();
                Run(out bool result);
                Cleanup();

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.Message + e.StackTrace);
            }
            return false;
        }

        public static int Report(AwesomeInventoryUnitTest tests, ref StringBuilder sb, string indent = "")
        {
            int num = 0;
            if (tests == null || sb == null)
            {
                return 0;
            }

            if (!tests.Tests.Any())
            {
                return 1;
            }

            sb.Append(indent);
            sb.AppendLine(string.Format(StringResource.NumberOfChildrenTests, tests.FullName, tests.Tests.Count));
            foreach (AwesomeInventoryUnitTest test in tests.Tests)
            {
                sb.Append(indent);
                sb.Append(StringResource.Indent);
                sb.AppendLine(string.Format(StringResource.KeyValuePair, test.FullName, tests.TestResults[test.FullName]));
                num += Report(test, ref sb, string.Concat(indent, StringResource.Indent));
            }

            sb.AppendLine();
            if (indent.NullOrEmpty())
            {
                sb.AppendLine(string.Format(StringResource.TotalTests, num));
            }

            return num;
        }
    }
}