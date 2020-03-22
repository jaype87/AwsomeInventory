// <copyright file="RPGIIntegrationTest.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    [Flags]
    public enum TestFlags : uint
    {
        Zero = 0b_0000_0000_0000_0000,
        I    = 0b_0000_0000_0000_0001,
        II   = 0b_0000_0000_0000_0010,
        III  = 0b_0000_0000_0000_0100,
        IV   = 0b_0000_0000_0000_1000,
        V    = 0b_0000_0000_0001_0000,
        VI   = 0b_0000_0000_0010_0000,
        VII  = 0b_0000_0000_0100_0000,
        VIII = 0b_0000_0000_1000_0000,
        IX   = 0b_0000_0001_0000_0000,
        X    = 0b_0000_0010_0000_0000,
        XI   = 0b_0000_0100_0000_0000,
        XII  = 0b_0000_1000_0000_0000,
        XIII = 0b_0001_0000_0000_0000,
        XIV  = 0b_0010_0000_0000_0000,
        XV   = 0b_0100_0000_0000_0000,
        XVI  = 0b_1000_0000_0000_0000
    }
    public abstract class RPGIIntegrationTest : RPGIUnitTest
    {
        private TestFlags _flags = TestFlags.Zero;
        private TestFlags _flagsSet = TestFlags.Zero;
        private Dictionary<TestFlags, object> _results = new Dictionary<TestFlags, object>();

        #region Methods

        public void InitFlags(TestFlags flags)
        {
            _flags |= flags;
        }

        public void SetFlag(TestFlags flag, object value)
        {
            if (flag == TestFlags.Zero)
            {
                throw new ArgumentException(StringResource.SettingFlagZero);
            }

            ulong flagValue = (ulong)flag;
            while (flagValue % 2 == 0)
            {
                flagValue /= 2;
            }
            if (flagValue != 1)
            {
                throw new ArgumentException(StringResource.MoreThanOneFlag);
            }

            _flagsSet |= flag;
            _results[flag] = value;

            Callback(flag, value);
        }

        public bool isResultReady
        {
            get => _flags == _flagsSet ? true : false;
        }

        public abstract void Callback(TestFlags flag, object value);

        #endregion
    }
}
#endif