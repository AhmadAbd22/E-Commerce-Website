using ECommerceWebsite.Models;
using ECommerceWebsite.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebsite.Controllers
{
    public class UserHomeController : Controller
    {
        private readonly IBookRepository _bookRepo;

        public UserHomeController(IBookRepository bookRepo)
        {
            _bookRepo = bookRepo;
        }

        public async Task<IActionResult> UserHome()
        {
            var books = await _bookRepo.GetAllBooksAsync();
            return View(books);
        }
    }
}