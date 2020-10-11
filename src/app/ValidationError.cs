// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.Helium
{
    /// <summary>
    /// Validation Error Class
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="code">Error Code</param>
        /// <param name="target">Error Target</param>
        /// <param name="message">Error Message</param>
        public ValidationError(string code, string target, string message)
        {
            Code = code;
            Target = target;
            Message = message;
        }

        /// <summary>
        /// Gets error Code
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets error Target
        /// </summary>
        public string Target { get; }

        /// <summary>
        /// Gets error Message
        /// </summary>
        public string Message { get; }
    }
}
