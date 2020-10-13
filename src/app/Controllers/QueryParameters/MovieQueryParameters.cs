// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;
using CSE.Helium.Validation;

namespace CSE.Helium
{
    public sealed class MovieQueryParameters : QueryParameters
    {
        [IdValidation(startingCharacters: "nm", minimumCharacters: 7, maximumCharacters: 11, true)]
        public string ActorId { get; set; }

        [StringLength(maximumLength: 20, MinimumLength = 3)]
        public string Genre { get; set; }

        [YearValidation]
        public int Year { get; set; }

        [Range(minimum: 0.0, maximum: 10.0)]
        public double Rating { get; set; }
    }
}
