// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "breaks json serialization")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "lower case is needed for genre key", Scope = "namespaceanddescendants", Target = "Helium.DataAccessLayer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literal string", Justification = "logging is not globalized", Scope = "namespaceanddescendants", Target = "Helium.DataAccessLayer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2000:IDispose", Justification = "IDispose is implemented", Scope = "namespaceanddescendants", Target = "Helium.DataAccessLayer")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1031:Catch Exception", Justification = "need to catch all exceptions", Scope = "namespaceanddescendants", Target = "Helium.Controllers")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literal string", Justification = "logging is not globalized", Scope = "namespaceanddescendants", Target = "Helium.Controllers")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1031:Catch Exception", Justification = "need to catch all exceptions", Scope = "type", Target = "Helium.App")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literal string", Justification = "logging is not globalized", Scope = "type", Target = "Helium.App")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1062:Literal string", Justification = "logging is not globalized", Scope = "type", Target = "Helium.App")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2000:IDispose", Justification = "IDispose is implemented", Scope = "type", Target = "Helium.App")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Required for DI", Scope = "type", Target = "Helium.Startup")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1031:Catch Exception", Justification = "need to catch all exceptions", Scope = "type", Target = "Helium.CosmosHealthCheck")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literal string", Justification = "logging is not globalized", Scope = "type", Target = "Helium.CosmosHealthCheck")]
