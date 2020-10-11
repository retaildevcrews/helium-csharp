// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace CSE.Helium.Model
{
    public class Role
    {
        public int Order { get; set; }
        public string ActorId { get; set; }
        public string Name { get; set; }
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public string Category { get; set; }
        public List<string> Characters { get; set; }
    }
}
