using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesLibrary;
using MoviesWebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebApp.Controllers
{
    public class ReviewController : Controller
    {
        private MovieService _movies;
        private readonly IAuthorizationService _authorizationService;
        private ReviewService _reviews;

        // TODO: inject and store the authorization service
        public ReviewController(ReviewService reviews, MovieService movies, IAuthorizationService authorizationService)
        {
            _reviews = reviews;
            _movies = movies;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<IActionResult> New(int movieId)
        {
            var movie = _movies.GetDetails(movieId);
            if (movie == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            if (!(await _authorizationService.AuthorizeAsync(User, movie, Authorization.MovieOperations.Review)))
            {
                return Challenge();
            }

            return View(new NewReviewModel() {
                MovieId = movieId,
                MovieTitle = movie.Title
            });
        }

        [HttpPost]
        public async Task<IActionResult> New(NewReviewModel model)
        {
            var movie = _movies.GetDetails(model.MovieId);
            if (movie == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            // TODO: authorize the user to create reviews for the movie

            if (ModelState.IsValid)
            {
                var result = _reviews.Create(model.MovieId, model.Stars, model.Comment, "test");
                if (result.Succeeded)
                {
                    return View("Success", new ReviewSuccessViewModel { MovieId = model.MovieId, Action = "Created" });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            model.MovieTitle = movie.Title;

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var review = _reviews.Get(id);
            if (review == null)
            {
                return RedirectToAction("Index", "Movie");
            }

            if (!(await _authorizationService.AuthorizeAsync(User, review, Authorization.ReviewOperations.Edit)))
            {
                return Challenge();
            }

            var movie = _movies.GetDetails(review.MovieId);

            var model = new EditReviewModel
            {
                ReviewId = id,
                Comment = review.Comment,
                Stars = review.Stars,
                MovieTitle = movie.Title,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditReviewModel model)
        {
            var review = _reviews.Get(model.ReviewId);
            if (review == null)
            {
                return RedirectToAction("Index", "Movie");
            }

            if (!(await _authorizationService.AuthorizeAsync(User, review, Authorization.ReviewOperations.Edit)))
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                var result = _reviews.Update(model.ReviewId, model.Stars, model.Comment);
                if (result.Succeeded)
                {
                    return View("Success", new ReviewSuccessViewModel { MovieId = review.MovieId, Action = "Updated" });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            var movie = _movies.GetDetails(review.MovieId);
            model.MovieTitle = movie.Title;

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var review = _reviews.Get(reviewId);
            if (review == null)
            {
                return RedirectToAction("Index", "Movie");
            }

            // TODO: authorize the user to edit the review

            var result = _reviews.Delete(reviewId);
            if (result.Succeeded)
            {
                return View("Success", new ReviewSuccessViewModel { MovieId = review.MovieId, Action = "Deleted" });
            }

            return RedirectToAction("Edit", new { id = reviewId });
        }
    }
}
