// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.Helium
{
    public class ValidationError
    {
        public string Code { get; }

        public string Target { get; }

        public string Message { get; }

        public ValidationError(string code, string target, string message)
        {
            Code = code;
            Target = target;
            Message = message;
        }
    }
}
