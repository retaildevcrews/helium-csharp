using Microsoft.Extensions.DependencyInjection;

namespace CSE.KeyRotation
{
    public static class KeyRotationExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddKeyRotation(this IServiceCollection services)
        {
            services.AddSingleton<IKeyRotation, KeyRotationHelper>();
            return services;
        }
    }
}
