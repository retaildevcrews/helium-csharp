// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.Helium.Validation
{
    public class ValidationProblemDetails
    {
        private readonly List<ValidationError> validationErrors = new List<ValidationError>();

        public string Type { get; }

        public string Title { get; }

        public string Detail { get; }

        public int Status { get; }

        public string Instance { get; }

        public ICollection<ValidationError> ValidationErrors => validationErrors;

        public ValidationProblemDetails(string type, string title, string detail, int status, string instance)
        {
            Type = type;
            Title = title;
            Detail = detail;
            Status = status;
            Instance = instance;
        }
    }
}
