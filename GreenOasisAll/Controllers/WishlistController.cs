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
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public WishlistController(ApplicationDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [Authorize]
        public IActionResult WishlistIndex()
        {
            var claim = _signInManager.IsSignedIn(User);
            if (claim)
            {
                var userId = _userManager.GetUserId(User);
                WishlistIndexVM wishlistIndexVM = new WishlistIndexVM()
                {
                    productList = _db.Wishlists.Include(u => u.product).Where(u => u.userId.Contains(userId)).ToList(),
                };
                var count = _db.Carts.Where(u => u.userId.Contains(userId)).Count();
                HttpContext.Session.SetInt32(wishlistCount.seCount, count);

                return View(wishlistIndexVM);
            }
            return View();
        }
        [Authorize]
        public async Task<IActionResult> AddToWishlist(int productId, string? returnUrl)
        {
            var productAddToWishlist = await _db.Wishlists.FirstOrDefaultAsync(u => u.Id == productId);
            var CheckIfUserSignedInOrNot = _signInManager.IsSignedIn(User);
            if (CheckIfUserSignedInOrNot)
            {
                var user = _userManager.GetUserId(User);
                if (user != null)
                {
                    //Check if the signed user has any cart or not?
                    var getTheWishlistIfAnyExistForTheUser = await _db.Wishlists.Where(u => u.userId.Contains(user)).ToListAsync();
                    if (getTheWishlistIfAnyExistForTheUser.Count() > 0)
                    {
                        //Check if the item is already in the cart or not
                        var getTheQty = getTheWishlistIfAnyExistForTheUser.FirstOrDefault(p => p.ProductId == productId);
                        if (getTheQty != null)
                        {
                            //if the item is already in the cart just increase the quantity by 1 and update the cart
                            getTheQty.Quantity = getTheQty.Quantity + 1;
                            _db.Wishlists.Update(getTheQty);
                        }
                        else
                        {
                            //User has a cart but adding a new item to the existing cart
                            Wishlist newItemToWishlist = new Wishlist
                            {
                                ProductId = productId,
                                userId = user,
                                Quantity = 1,
                            };
                            await _db.Wishlists.AddAsync(newItemToWishlist);
                        }
                    }
                    else
                    {
                        //User has no cart. Adding a brand new cart for the user
                        Wishlist newItemToWishlist = new Wishlist
                        {
                            ProductId = productId,
                            userId = user,
                            Quantity = 1,
                        };
                        await _db.Wishlists.AddAsync(newItemToWishlist);
                    }
                    await _db.SaveChangesAsync();

                }
            }
            if (returnUrl != null)
            {
                return RedirectToAction("WishlistIndex", "Wishlist");
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult RemoveWishlist(int productId)
        {
            //Get the item which we need to minus a quantity
            var removewishlist = _db.Wishlists.FirstOrDefault(u => u.ProductId == productId);
            if (removewishlist != null)
            {
                _db.Wishlists.Remove(removewishlist);

                _db.SaveChanges();
            }
            return RedirectToAction(nameof(WishlistIndex));
        }
    }
}
