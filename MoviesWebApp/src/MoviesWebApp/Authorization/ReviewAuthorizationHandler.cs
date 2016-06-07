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
            if (context.User.HasClaim("role", "Admin"))
            {
                context.Succeed(requirement);
            }

            // TODO: allow the user whose sub is the same as the review's UserId property to edit the review
            if (requirement == ReviewOperations.Edit)
            {
                var sub = context.User.FindFirst("sub")?.Value;
                if (sub != null && review.UserId == sub)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
