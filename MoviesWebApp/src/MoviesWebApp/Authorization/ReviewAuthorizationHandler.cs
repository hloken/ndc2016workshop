using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MoviesLibrary;

namespace MoviesWebApp.Authorization
{
    public class ReviewOperations
    {
        public static readonly OperationAuthorizationRequirement Edit = 
            new OperationAuthorizationRequirement() { Name = "Edit" };
    }

    public class ReviewAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, MovieReview>
    {
        protected override void Handle(
            AuthorizationContext context, 
            OperationAuthorizationRequirement requirement, 
            MovieReview review)
        {
            // TODO: allow admins to perform any operation

            // TODO: allow the user whose sub is the same as the review's UserId property to edit the review
        }
    }
}
