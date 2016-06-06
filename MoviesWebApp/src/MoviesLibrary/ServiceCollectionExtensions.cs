using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MoviesLibrary;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMoviesLibrary(this IServiceCollection services)
        {
            services.AddTransient<MovieService>();
            services.AddTransient<ReviewService>();
            services.AddTransient<MovieIdentityService>();
            services.AddTransient<ReviewPermisssionService>();

            return services;
        }
    }
}
