// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "need to catch all exceptions", Scope = "type", Target = "CSE.Helium.App")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "need to catch all exceptions", Scope = "namespaceanddescendants", Target = "CSE.Helium.Controllers")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "need to catch all exceptions", Scope = "namespaceanddescendants", Target = "CSE.Helium.DataAccessLayer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "need to catch all exceptions", Scope = "type", Target = "CSE.Helium.CosmosHealthCheck")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "need to catch all exceptions", Scope = "namespaceanddescendants", Target = "Middleware")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1303:Do not pass literals as localized parameters", Justification = "method name for logging", Scope = "type", Target = "CSE.Helium.App")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1303:Do not pass literals as localized parameters", Justification = "method name for logging", Scope = "namespaceanddescendants", Target = "CSE.Helium.Controllers")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1303:Do not pass literals as localized parameters", Justification = "method name for logging", Scope = "namespaceanddescendants", Target = "CSE.Helium.DataAccessLayer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1303:Do not pass literals as localized parameters", Justification = "method name for logging", Scope = "type", Target = "CSE.Helium.CosmosHealthCheck")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "search string has to be lower case", Scope = "namespaceanddescendants", Target = "CSE.Helium.DataAccessLayer")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IDispose", Scope = "type", Target = "CSE.Helium.App")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IDispose", Scope = "type", Target = "CSE.Helium.DataAccessLayer.DAL")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "needed for json deserialization", Scope = "namespaceanddescendants", Target = "CSE.Helium.Model")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("IDE", "IDE0051:Private member GetXmlCommentsPath is unused", Justification = "can be used if configured", Scope = "type", Target = "CSE.Helium.Startup")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "KeyVault SDK expects string", Scope = "type", Target = "CSE.Helium.App")]
