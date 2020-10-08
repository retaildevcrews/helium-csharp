// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace CSE.KeyRotation
{
    public static class KeyRotationExtension
    {
        /// <summary>
        /// Adds the KeyRotation to services via DI.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>The IServiceCollection</returns>
        public static IServiceCollection AddKeyRotation(this IServiceCollection services)
        {
            services.AddSingleton<IKeyRotation, KeyRotationHelper>();
            return services;
        }
    }
}
