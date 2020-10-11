// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Helium.Validation;

namespace CSE.Helium
{
    /// <summary>
    /// Movie ID query string validation class
    /// </summary>
    public sealed class MovieIdParameter
    {
        /// <summary>
        /// gets or sets a valid movie ID
        /// </summary>
        [IdValidation(startingCharacters: "tt", minimumCharacters: 7, maximumCharacters: 11, false)]
        public string MovieId { get; set; }
    }
}
