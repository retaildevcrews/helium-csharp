// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "need to catch all exceptions")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "KeyVault SDK expects string", Scope = "namespaceanddescendants", Target = "CSE.KeyVault")]
[assembly: SuppressMessage("Design", "CA1303:Do not pass literals as localized parameters", Justification = "method name for logging")]
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "search string has to be lower case", Scope = "namespaceanddescendants", Target = "CSE.Helium.DataAccessLayer")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IDispose", Scope = "type", Target = "~T:CSE.Helium.App")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "IDispose", Scope = "namespaceanddescendants", Target = "CSE.Helium.DataAccessLayer")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "needed for json deserialization", Scope = "namespaceanddescendants", Target = "CSE.Helium.Model")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Creation of Cosmos Client", Scope = "member", Target = "~M:CSE.Helium.DataAccessLayer.DAL.OpenAndTestCosmosClient(System.Uri,System.String,System.String,System.String,Microsoft.Azure.Cosmos.CosmosClient)~System.Threading.Tasks.Task{Microsoft.Azure.Cosmos.CosmosClient}")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Cosmos Client is used as singleton. Cannot be disposed", Scope = "member", Target = "~M:Helium.DataAccessLayer.CosmosClientExtension.AddCosmosClient(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Uri,System.String)~Microsoft.Extensions.DependencyInjection.IServiceCollection")]
