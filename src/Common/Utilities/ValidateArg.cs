// <copyright file="ValidateArg.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace AwesomeInventory
{
    /// <summary>
    /// A helper for validating arguments.
    /// </summary>
    public static class ValidateArg
    {
        /// <summary>
        /// Throws <see cref="ArgumentNullException"/> if argument is null.
        /// </summary>
        /// <param name="arg"> Argument to check. </param>
        /// <param name="argName"> Name of <paramref name="arg"/>. </param>
        /// <exception cref="ArgumentNullException"> Throws if arg is null. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object arg, string argName)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        /// <summary>
        /// Check if argument is null or empty.
        /// </summary>
        /// <param name="arg"> Argument to check. </param>
        /// <param name="argName"> Name of <paramref name="arg"/>. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmpty(object arg, string argName)
        {
            ValidateArg.NotNull(arg, argName);

            switch (arg)
            {
                case string str:
                    if (string.IsNullOrEmpty(str))
                        throw new ArgumentEmtpyException(argName);
                    break;
                case IEnumerable items:
                    if (items.GetEnumerator().MoveNext())
                        throw new ArgumentEmtpyException(argName);
                    break;
            }
        }
    }
}
