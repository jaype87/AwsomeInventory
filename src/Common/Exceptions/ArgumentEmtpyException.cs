// <copyright file="ArgumentEmtpyException.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace AwesomeInventory
{
    /// <summary>
    /// Exception to be thrown if an argument has no item/element in it.
    /// </summary>
    [Serializable]
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Not required.")]
    public sealed class ArgumentEmtpyException : Exception
    {
        private const string _message = "Argument {0} is empty.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentEmtpyException"/> class.
        /// </summary>
        /// <param name="argName"> Name of the empty argument. </param>
        public ArgumentEmtpyException(string argName)
            : base(string.Format(CultureInfo.InvariantCulture, _message, argName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentEmtpyException"/> class.
        /// </summary>
        /// <param name="serializationInfo"> All the info needed to deserialize the exception. </param>
        /// <param name="streamingContext"> Describes source and destination of this exception. </param>
        private ArgumentEmtpyException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
