// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;
using CSE.Helium.Validation;

namespace CSE.Helium
{
    /// <summary>
    /// abstract class used to validate query string parameters
    /// </summary>
    public abstract class QueryParameters
    {
        /// <summary>
        /// gets or sets a valid page number
        /// </summary>
        [IntegerRangeValidation(minValue: 1, maxValue: 10000)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// gets or sets a valid page size
        /// </summary>
        [IntegerRangeValidation(minValue: 1, maxValue: 1000)]
        public int PageSize { get; set; } = 100;

        /// <summary>
        /// gets or sets a valid query (search) string
        /// </summary>
        [StringLength(maximumLength: 20, MinimumLength = 2)]
        public string Q { get; set; }

        /// <summary>
        /// Compute the Cosmos DB query offset based on page size / number
        /// </summary>
        /// <returns>Cosmos offset</returns>
        public int GetOffset()
        {
            return PageSize * (PageNumber > 1 ? PageNumber - 1 : 0);
        }
    }
}
