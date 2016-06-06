# ASP.NET Core Authentication and Authorization

This lab is based on a simple movie review website.
It allows customers to browse and search movies and view movie reviews.
It also allows reviewers to create and edit movie reviews.

## Overview

In this lab you will add cookie-based authentication to the movie review website using the cookie authentication middleware and claims-based identity.
Once users are authenticated, you will then also implement policy-based and resource-based authorization using the ASP.NET Core authorization framework.

### Application notes

_Note_: All the data for the movie review website it kept in-memory, so any changes to data will be lost when the application restarts.

_Users_: The lab pre-defines the concept of five users whose usernames are **user1** through **user5**.
These users' passwords will be the same as their username.
Once these users login to the applicaiton they will have different roles within the application: 
*user1*, *user2* and *user3* are reviewers, 
*user4* is a customer, 
and *user5* is an administrator.
When you login you can choose one of those usernames in order to trigger different behavior in the application.


## Part 1: Cookie-base authentication

In this part you will add the cookie authentication middleware, allow the user to login and logout, and use claims to model the identity of the authenticated user.

* Open the application from the `~/before` folder. 
 * Inspect the code to become familiar with the structure.
 * Run the application to see what it does.
* To authenticate users, we need to add the cookie authentication middleware.
 * Add the cookie authentication middleware NuGet in `project.json`. 
```
    "Microsoft.AspNetCore.Authentication.Cookies": "1.0.0-rc2-final"
```
 * In `Configure` register the cookie authentication middleware after the static file middleware.
 ```
 app.UseCookieAuthentication(new CookieAuthenticationOptions
{
        AuthenticationScheme = "Cookies",
        AutomaticAuthenticate = true,
        AutomaticChallenge = true,
        LoginPath = new PathString("/Account/Login"),
        AccessDeniedPath = new PathString("/Account/Denied")
});
```
* Write the logic to allow users to signin in `~/Controllers/Account.cs`.
 * We don't have a real database of username/passwords, so just check that they are the same.
 * If successful, create a list of `Claim`s and populate it with the `sub` claim with the value of the `username`.
 * Notice there is an `MovieIdentityService` in the `AccountController` -- this allows application specific claims to be loaded based upon the `sub` claim.
   Feel free to look in the implementation to understand the additional claims being loaded for the users.
   Invoke it and merge the claims returned into the claims collection you created.
 * Create `ClaimsIdentity` and `ClaimsPrincipal` from the claims.
 * User the `AuthenticationManager` and issue the cookie from the `ClaimsPrincipal`.
 * Rediriect the user to the `ReturnUrl` (if present), or to the home page. 
```
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
        if (model.Username == model.Password)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", model.Username)
            };
            
            claims.AddRange(_identityService.GetClaimsForUser(model.Username));

            var ci = new ClaimsIdentity(claims, "password", "name", "role");
            var cp = new ClaimsPrincipal(ci);

            await HttpContext.Authentication.SignInAsync("Cookies", cp);

            if (model.ReturnUrl != null)
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid username or password");
        return View();
}
```
* Write the logic to allow a user to signout.
```
public async Task<IActionResult> Logout()
{
    await HttpContext.Authentication.SignOutAsync("Cookies");
    return RedirectToAction("Index", "Home");
}
```
* Run the application and test signing in and signing out.

## Part 2: Policy-based and Resource-based authorization

In this part you will enable authorization. 
There are several pieces to this, including preventing anonymous access to much of the application, only allowing customers to use the search feature, and only allowing reviewers to create and edit reviews.

* The first step to enable authorization is to add the NuGet in `project.json`. 
```
    "Microsoft.AspNetCore.Authorization": "1.0.0-rc2-final"
```
* Next add the authorization services to DI in `ConfigureServices`.
```
    services.AddAuthorization();
```
* Next we want a global authorization filter that prevents anonymous access. 
 * In `ConfigureServices` locate the call to `AddMvc` and the configuration callback. 
 * Create a policy by using a `AuthorizationPolicyBuilder`, 
   and calling `RequireAuthenticatedUser` and `Build`. 
 * Create a new `AuthorizeFilter` using the new policy. 
 * Add the filter to the `MvcOptions`'s `Filters` collection.
```
services.AddMvc(options =>
{
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
});
```

* If you were to run the application now, an anonymous user would not be able to access any page including the login page. 
  We now need to relax the global filter except for the few places where we want to allow anoymous access.
  * Add the `[AllowAnonymous]` attribute to both the `HomeController` and the `AccountController`.
```
[AllowAnonymous]
public class AccountController : Controller
{
        ...
}
```

* Now run the application to test that an anonymous user cannot access the movies, but can login.
If not allowed, you should be redirected to the "access denied" page.

* The next authorization we want to enforce is only customers may use the search feature. 
  This involves building an authorization policy.
 * Locate the call to `AddAuthorization` in `ConfigureServices`.
 * Change the signature to accept an delegate that passes the options.
```
services.AddAuthorization(options =>
{
        ...
});
```
 * In the callback, use the options to `AddPolicy` called `"SearchPolicy"`.
 * Build the policy to `RequireAuthenticatedUser` and `RequireAssertion`. 
   * For the assertion callback check for either the `""Admin"`' or `"Customer"` and if those claims are present return `true`, `false` otherwise.
```
services.AddAuthorization(options =>
{
        options.AddPolicy("SearchPolicy", builder =>
        {
            builder.RequireAuthenticatedUser();
            builder.RequireAssertion(ctx =>
            {
                if (ctx.User.HasClaim("role", "Admin") ||
                    ctx.User.HasClaim("role", "Customer"))
                {
                    return true;
                }
                return false;
            });
        });
});
``` 

* Now apply the `"SearchPolicy"` to the `Search` action method on the `MovieController`.
```
[Authorize("SearchPolicy")]
public IActionResult Search(string searchTerm = null)
{
        ...
}
```

* Run the application to test that only customers or admins (i.e. **user4** or **user5**) are allowed to use the search feature.
If not allowed, you should be redirected to the "access denied" page.

* The final authorization logic we require is to only allow reviewers to create and edit reviews.
We will do this by building authorization handlers. 
 * The start of the authorization handlers are already created for you. 
 They are in the `~/Authorization` folder. Open them and inspect the starter code.
 * For the `MovieAuthorizationHandler` implement the logic that only reviewers are allowed to review movies.
 ```
 protected override void Handle(
        AuthorizationContext context,
        OperationAuthorizationRequirement requirement,
        MovieDetails movie)
{
        if (requirement == MovieOperations.Review)
        {
            if (context.User.HasClaim("role", "Reviewer"))
            {
                context.Succeed(requirement);
            }
        }
}
 ```
  * For the `ReviewAuthorizationHandler` implement the logic that only te reviewer that created the review can edit it.
   Use the `sub` claim on the user and compare it to the `UserId` property on the `MovieReview`.
   Also, allow admins to perform any operation.
```
protected override void Handle(
        AuthorizationContext context, 
        OperationAuthorizationRequirement requirement, 
        MovieReview review)
{
        if (context.User.HasClaim("role", "Admin"))
        {
            context.Succeed(requirement);
        }

        if (requirement == ReviewOperations.Edit)
        {
            var sub = context.User.FindFirst("sub")?.Value;
            if (sub != null && review.UserId == sub)
            {
                context.Succeed(requirement);
            }
        }
}
```

* To use these authorization handlers, they need to be registered in DI in `ConfigureServices`. Do that now.
```
        services.AddTransient<IAuthorizationHandler, ReviewAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MovieAuthorizationHandler>();
```

* Next we want to invoke the authorization logic in the MVC code to protect access.
 * In the `ReviewController` controller change the consructor and inject a `IAuthorizationService` and store it in a member variable.
```
private IAuthorizationService _authorization;
public ReviewController(ReviewService reviews, 
        MovieService movies, IAuthorizationService authorization)
{
        _reviews = reviews;
        _movies = movies;
        _authorization = authorization;
}
```
 * In `New` enforce the authorization for creating a review for the movie.
```
if (!(await _authorization.AuthorizeAsync(
        User, movie, Authorization.MovieOperations.Review)))
{
        return Challenge();
}
```
 * In `Edit` and `Delete` enforce the authorization for editing the review.
```
if (!(await _authorization.AuthorizeAsync(
        User, review, Authorization.ReviewOperations.Edit)))
{
        return Challenge();
}
```
* Run the application and test that only reviewers can create reviews, and that reviewers can only edit their own reviews.  
* Next we want to hide the buttons in the UI if the user is not allowed to create or edit reviews.
 * In `~/Views/Movie/Details.cshtml` notice the `IAuthorizationService` is already being injected.
 * Locate the "create review" button and hide it is the user is not authorized.
```
@if (await authorization.AuthorizeAsync(
        User, Model, MoviesWebApp.Authorization.MovieOperations.Review))
{
        <div class="row search-form">
            <a asp-action="New" asp-controller="Review" 
                asp-route-movieId="@Model.Id" 
                class="btn btn-primary">Write a review</a>
        </div>
}
``` 
 * Locate the "edit review" button and hide it is the user is not authorized.
```
<td>
        @if (await authorization.AuthorizeAsync(
            User, review, MoviesWebApp.Authorization.ReviewOperations.Edit))
        {
            <a asp-action="Edit" asp-controller="Review" 
                asp-route-id="@review.Id" class="btn btn-primary">edit</a>
        }
</td>
```
* Run and test that the buttons are now hidden when appropriate.
* Finally, we have a change in our authorization logic. 
Reviewers are not allowed to create reviews for all movies. Certain reviewers are only allowed to review movies from certain countries.
This logic requires a lookup in a permission database and this is implemented in a class called `ReviewPermisssionService`. 
You will now incorporate this additional logic in the `MovieAuthorizationHandler`.
 *  Change the constructor to accept the `MovieAuthorizationHandler` and store it in a member variable.
```
private ReviewPermisssionService _reviewPermissions;
public MovieAuthorizationHandler(ReviewPermisssionService reviewPermissions)
{
        _reviewPermissions = reviewPermissions;
}
```
 * In `Handle` after the role check, invoke `GetAllowedCountries` on the `ReviewPermisssionService` and compare the movie's `CountryName` to the returned list
  Only if the movie is from an allowed country, then call `Succeed`.
```
protected override void Handle(
            AuthorizationContext context,
            OperationAuthorizationRequirement requirement,
            MovieDetails movie)
{
        if (requirement == MovieOperations.Review)
        {
            if (context.User.HasClaim("role", "Reviewer"))
            {
                var allowed = 
                    _reviewPermissions.GetAllowedCountries(context.User);
                if (allowed.Contains(movie.CountryName))
                {
                    context.Succeed(requirement);
                }
            }
        }
}
```
* Run and test the country-specific authorization. *user1* should be able to create movies from any country, but *user2* cannot create a review for France.
