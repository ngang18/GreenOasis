using System.Diagnostics;
using DatabaseAccess;
using GreenOasisAll.Models;
using GreenOasisAll.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelClasses;
using ModelClasses.ViewModel;

namespace GreenOasisAll.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index(string? searchByName, string? searchByCategory)
        {
            var claim = _signInManager.IsSignedIn(User);
            if (claim)
            {
                var userId = _userManager.GetUserId(User);
                var count = _db.Carts.Where(u => u.userId.Contains(userId)).Count();
                HttpContext.Session.SetInt32(cartCount.sessionCount, count);
            }


            HomePageVM vm = new HomePageVM();
            if (searchByName != null)
            {
                vm.ProductList = _db.Products.Where(productName => EF.Functions.Like(productName.Name, $"%{searchByName}%")).ToList();
                vm.Categories = _db.Categories.ToList();
            }
            else if (searchByCategory != null)
            {
                var searchByCategoryName = _db.Categories.FirstOrDefault(u => u.Name == searchByCategory);
                vm.ProductList = _db.Products.Where(u => u.CategoryId == searchByCategoryName.Id).ToList();
                vm.Categories = _db.Categories.Where(u => u.Name.Contains(searchByCategory));
            }
            else
            {
                vm.ProductList = _db.Products.ToList();
                vm.Categories = _db.Categories.ToList();
            }


            return View(vm);
        }

        public ActionResult Blog()
        {
            return View();
        }
        public ActionResult AboutUs()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult FAQ()
        {
            return View();
        }
        public IActionResult Wishlist()
        {
            return View();
        }
        public IActionResult SingleProduct()
        {
            var model = new HomePageVM
            {
                ProductList = new List<Product>(), // Initialize with data  
                Categories = new List<Category>()
            };
            return View(model);
        }

        public IActionResult Compare()
        {
            return View();
        }

        public IActionResult BlogDetails()
        {
            return View();
        }

        public IActionResult ThankYou()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
