using DatabaseAccess;
using GreenOasisAll.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModelClasses.ViewModel;
using ModelClasses;
using Microsoft.EntityFrameworkCore;

namespace GreenOasisAll.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public CartController(ApplicationDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [Authorize]
        public IActionResult CartIndex()
        {
            var claim = _signInManager.IsSignedIn(User);
            if (claim)
            {
                var userId = _userManager.GetUserId(User);
                CartIndexVM cartIndexVM = new CartIndexVM()
                {
                    productList = _db.Carts.Include(u => u.product).Where(u => u.userId.Contains(userId)).ToList(),
                };
                var count = _db.Carts.Where(u => u.userId.Contains(userId)).Count();
                HttpContext.Session.SetInt32(cartCount.sessionCount, count);

                return View(cartIndexVM);
            }
            return View();
        }

        [Authorize]
       
        public async Task<IActionResult> AddToCart(int productId, string? returnUrl)
        {
            var productAddToCart = await _db.Products.FirstOrDefaultAsync(u => u.Id == productId);
            var CheckIfUserSignedInOrNot = _signInManager.IsSignedIn(User);
            if (CheckIfUserSignedInOrNot)
            {
                var user = _userManager.GetUserId(User);
                if (user != null)
                {
                    //Check if the signed user has any cart or not?
                    var getTheCartIfAnyExistForTheUser = await _db.Carts.Where(u => u.userId.Contains(user)).ToListAsync();
                    if (getTheCartIfAnyExistForTheUser.Count() > 0)
                    {
                        //Check if the item is already in the cart or not
                        var getTheQuantity = getTheCartIfAnyExistForTheUser.FirstOrDefault(p => p.ProductId == productId);
                        if (getTheQuantity != null)
                        {
                            //if the item is already in the cart just increase the quantity by 1 and update the cart
                            getTheQuantity.Quantity = getTheQuantity.Quantity + 1;
                            _db.Carts.Update(getTheQuantity);
                        }
                        else
                        {
                            //User has a cart but adding a new item to the existing cart
                            Cart newItemToCart = new Cart
                            {
                                ProductId = productId,
                                userId = user,
                                Quantity = 1,
                            };
                            await _db.Carts.AddAsync(newItemToCart);
                        }
                    }
                    else
                    {
                        //User has no cart. Adding a brand new cart for the user
                        Cart newItemToCart = new Cart
                        {
                            ProductId = productId,
                            userId = user,
                            Quantity = 1,
                        };
                        await _db.Carts.AddAsync(newItemToCart);
                    }
                    await _db.SaveChangesAsync();

                }
            }
            if (returnUrl != null)
            {
                return RedirectToAction("CartIndex", "Cart");
            }
            return RedirectToAction("Index", "Home");
        }
        [Authorize]
       
        public IActionResult MinusAnItem(int productId)
        {
            var userId = _userManager.GetUserId(User);
            //Get the item which we need to minus a quantity
            var itemToMinus = _db.Carts.FirstOrDefault(u => u.ProductId == productId);
            if (itemToMinus != null)
            {
                if (itemToMinus.Quantity - 1 == 0)
                {
                    _db.Carts.Remove(itemToMinus);
                }
                else
                {
                    itemToMinus.Quantity -= 1;
                    _db.Carts.Update(itemToMinus);
                }
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(CartIndex));
        }
        [Authorize]
        public IActionResult DeleteAnItem(int productId)
        {
            //Get the item which we need to minus a quantity
            var itemToRemove = _db.Carts.FirstOrDefault(u => u.ProductId == productId);
            if (itemToRemove != null)
            {
                _db.Carts.Remove(itemToRemove);

                _db.SaveChanges();
            }
            return RedirectToAction(nameof(CartIndex));
        }
    }
}
