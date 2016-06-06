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
        // TODO: inject and store the ReviewPermisssionService
        public MovieAuthorizationHandler()
        {
        }

        protected override void Handle(
            AuthorizationContext context,
            OperationAuthorizationRequirement requirement,
            MovieDetails movie)
        {
            // TODO: allow reviewers to review movies


            // TODO: only allow reviewers to edit the movie if the movie is from the  
            // country the review is allowed to review, as indicated by the ReviewPermisssionService
        }
    }
}
