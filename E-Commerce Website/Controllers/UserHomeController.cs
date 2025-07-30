using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebsite.Controllers
{
    public class UserHomeController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly ICategoryRepository _categoryRepo;

        public UserHomeController(IBookRepository bookRepo, ICategoryRepository categoryRepo)
        {
            _bookRepo = bookRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> UserHome()
        {
            var books = await _bookRepo.GetAllBooksAsync();
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewData["Categories"] = categories;
            return View(books);
        }
    }
}