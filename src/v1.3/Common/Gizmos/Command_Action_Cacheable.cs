// <copyright file="Command_Action_Cacheable.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// A gizmo that can be cached.
    /// </summary>
    public abstract class Command_Action_Cacheable : Command_Action
    {
        /// <summary>
        /// Gets or sets a value indicating whether the gizmo needs to update.
        /// </summary>
        public virtual bool Dirty { get; set; } = false;

        /// <summary>
        /// It is called whenever this gizmo is retrieved from <see cref="Cache{T}"/>.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Cache for gizmo of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> Type of gizmo. </typeparam>
        public static class Cache<T>
            where T : Command_Action_Cacheable
        {
#pragma warning disable CA1000 // Do not declare static members on generic types
            private static Dictionary<Thing, T> _cache = new Dictionary<Thing, T>();

            /// <summary>
            /// Try to retrieve gizmo of type <typeparamref name="T"/> for <paramref name="thing"/>.
            /// </summary>
            /// <param name="thing"> Selected thing. </param>
            /// <param name="value"> Gizmo for pawn. </param>
            /// <returns> Returns true if a gizmo exists for <paramref name="thing"/>. </returns>
            public static bool TryGet(Thing thing, out T value)
            {
                return _cache.TryGetValue(thing, out value);
            }

            /// <summary>
            /// Save gizmo of type <typeparamref name="T"/> for <paramref name="thing"/>.
            /// </summary>
            /// <param name="thing"> Selected thing. </param>
            /// <param name="value"> Gizmo of type <typeparamref name="T"/>. </param>
            public static void Save(Thing thing, T value)
            {
                _cache[thing] = value;
            }

            /// <summary>
            /// Get gizmo of type <typeparamref name="T"/> for <paramref name="thing"/>.
            /// </summary>
            /// <param name="thing"> Seleted thing. </param>
            /// <param name="ctorArgs"> Constructor parameters for <typeparamref name="T"/>. </param>
            /// <returns> Gizmo of type <typeparamref name="T"/>. </returns>
            public static T Get(Thing thing, params object[] ctorArgs)
            {
                if (TryGet(thing, out T value))
                    value.Refresh();
                else
                    value = _cache[thing] = (T)Activator.CreateInstance(typeof(T), ctorArgs);

                return value;
            }
#pragma warning restore CA1000 // Do not declare static members on generic types
        }
    }
}
