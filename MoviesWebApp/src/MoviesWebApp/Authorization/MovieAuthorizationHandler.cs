using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MoviesLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebApp.Authorization
{
    public class MovieOperations
    {
        public static readonly OperationAuthorizationRequirement Review = 
            new OperationAuthorizationRequirement { Name = "Review" };
    }

    public class MovieAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, MovieDetails>
    {
        private readonly ReviewPermisssionService _reviewPermissionService;

        public MovieAuthorizationHandler(ReviewPermissionService reviewPermissionService)
        {
            _reviewPermissionService = reviewPermissionService;
        }

        protected override void Handle(
            AuthorizationContext context,
            OperationAuthorizationRequirement requirement,
            MovieDetails movie)
        {
            if (requirement == MovieOperations.Review)
            {
                if (context.User.HasClaim("role", "Reviewer"))
                {
                    var allowed = _reviewPermissionService.GetAllowedCountries(context.User);

                    if (allowed.Contains(movie.CountryName))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}
