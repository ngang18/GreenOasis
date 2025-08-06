using DatabaseAccess;
using GreenOasisAll.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using ModelClasses.ViewModel;
using ModelClasses;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace GreenOasisAll.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        [BindProperty]
        public OrderDetailsVM OrderDetailsVM { get; set; }
        public OrderController(ApplicationDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult orderDetailPreview()
        {
            var claim = _signInManager.IsSignedIn(User);
            if (claim)
            {
                var userId = _userManager.GetUserId(User);
                var currentUser = _db.applicationUser.FirstOrDefault(x => x.Id == userId);
                SummaryVM summaryVM = new SummaryVM()
                {
                    CartList = _db.Carts.Include(u => u.product).Where(u => u.userId.Contains(userId)).ToList(),
                    orderSummary = new UserOrderHeader(),
                    cartUserId = userId,
                };

                if (currentUser != null)
                {
                    //assigning the user's info from database as default address
                    summaryVM.orderSummary.DeliveryStreetAddress = currentUser.Address;
                    summaryVM.orderSummary.City = currentUser.City;
                    summaryVM.orderSummary.PostalCode = currentUser.PostalCode;
                    summaryVM.orderSummary.PhoneNumber = currentUser.PhoneNumber;
                    summaryVM.orderSummary.Name = currentUser.FirstName + " " + currentUser.LastName;
                }
                var count = _db.Carts.Where(u => u.userId.Contains(_userManager.GetUserId(User))).ToList().Count;
                HttpContext.Session.SetInt32(cartCount.sessionCount, count);
                return View(summaryVM);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summary(SummaryVM summaryVMFromView)
        {
            var claim = _signInManager.IsSignedIn(User);
            if (claim)
            {
                var userId = _userManager.GetUserId(User);
                var currentUser = _db.applicationUser.FirstOrDefault(x => x.Id == userId);
                SummaryVM summaryVM = new SummaryVM()
                {
                    CartList = _db.Carts.Include(u => u.product).Where(u => u.userId.Contains(userId)).ToList(),
                    orderSummary = new UserOrderHeader(),
                };

                if (currentUser != null)
                {
                    //assigning the user's info from database as default address
                    summaryVM.orderSummary.Name = summaryVMFromView.orderSummary.Name;
                    summaryVM.orderSummary.DeliveryStreetAddress = summaryVMFromView.orderSummary.DeliveryStreetAddress;
                    summaryVM.orderSummary.City = summaryVMFromView.orderSummary.City;
                    summaryVM.orderSummary.PostalCode = summaryVMFromView.orderSummary.PostalCode;
                    summaryVM.orderSummary.PhoneNumber = summaryVMFromView.orderSummary.PhoneNumber;
                    summaryVM.orderSummary.DateOfOrder = DateTime.Now;
                    summaryVM.orderSummary.OrderStatus = "Pending";
                    summaryVM.orderSummary.PaymentStatus = "Not paid";
                    summaryVM.orderSummary.UserId = summaryVMFromView.cartUserId;
                    summaryVM.orderSummary.TotalOrderAmount = summaryVMFromView.orderSummary.TotalOrderAmount;
                    await _db.AddAsync(summaryVM.orderSummary);
                    await _db.SaveChangesAsync();
                }

                if (summaryVMFromView.orderSummary.TotalOrderAmount > 0)
                {
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = "https://localhost:44351/Order/OrderSuccess/" + summaryVM.orderSummary.Id,
                        CancelUrl = "https://localhost:44351/Order/OrderCancel/" + summaryVM.orderSummary.Id,
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                    };
                    foreach (var item in summaryVM.CartList)
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = Convert.ToInt32(Math.Round(item.product.Price)),
                                //UnitAmount = Convert.ToInt32(Math.Round((decimal)(item.product.Price), 2) * 100),
                                Currency = "vnd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.product.Name,
                                    Description = item.product.Description,
                                }
                            },
                            Quantity = item.Quantity,
                        };
                        options.LineItems.Add(sessionLineItem);
                    }

                    var service = new SessionService();
                    Session session = service.Create(options);
                    var loadtheNewOrder = _db.OrderHeaders.FirstOrDefault(u => u.Id == summaryVM.orderSummary.Id);
                    loadtheNewOrder.StripeSessionId = session.Id;
                    loadtheNewOrder.StripePaymentIntentId = session.PaymentIntentId;
                    _db.OrderHeaders.Update(loadtheNewOrder);
                    _db.SaveChanges();
                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult OrderCancel(int id)
        {
            var orderProcessedCanceled = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            _db.OrderHeaders.Remove(orderProcessedCanceled);
            _db.SaveChanges();
            return RedirectToAction("CartIndex", "Cart");
        }

        public IActionResult OrderSuccess(int id)
        {

            var userId = _userManager.GetUserId(User);
            var UserCartRemove = _db.Carts.Where(u => u.userId.Contains(userId)).ToList();
            var orderProcessed = _db.OrderHeaders.FirstOrDefault(h => h.Id == id);
            //Update Payment status
            if (orderProcessed != null)
            {
                if (orderProcessed.PaymentStatus == UpdateOrderStatus.PaymentStatusNotPaid)
                {
                    var service = new SessionService();
                    Session session = service.Get(orderProcessed.StripeSessionId);
                    if (session.PaymentStatus.ToLower() == UpdateOrderStatus.PaymentStatusPaid.ToLower())
                    {
                        orderProcessed.PaymentStatus = UpdateOrderStatus.PaymentStatusPaid;
                        orderProcessed.PaymentProcessDate = DateTime.Now;
                        orderProcessed.StripePaymentIntentId = session.PaymentIntentId;

                        _db.OrderHeaders.Update(orderProcessed);
                        _db.SaveChanges();
                    }
                }
            }
            //Add the items from cart to Order Details table
            foreach (var list in UserCartRemove)
            {
                OrderDetails orderReceived = new OrderDetails()
                {
                    OrderHeaderId = orderProcessed.Id,
                    ProductId = (int)list.ProductId,
                    Count = list.Quantity,
                };

                _db.OrderDetails.Add(orderReceived);
            }
            ViewBag.OrderId = id;
            //Removed item from cart for the current user after successfully completing the payment process
            _db.Carts.RemoveRange(UserCartRemove);
            _db.SaveChanges();
            HttpContext.Session.Clear();

            return View();
        }

        public IActionResult OrderHistory(string? status)
        {
            //var userId = _userManager.GetUserId(User);
            //List<UserOrderHeader> orderLists = new List<UserOrderHeader>();
            //if (status != null && status != "All")
            //{
            //    if (User.IsInRole("Admin"))
            //    {
            //        orderLists = _db.OrderHeaders.Where(u => u.OrderStatus == status).ToList();
            //    }
            //    else
            //    {
            //        orderLists = _db.OrderHeaders.Where(u => u.OrderStatus == status && u.UserId == userId).ToList();
            //    }
            //}
            //else
            //{
            //    if (User.IsInRole("Admin"))
            //    {
            //        orderLists = _db.OrderHeaders.ToList();
            //    }
            //    else
            //    {
            //        orderLists = _db.OrderHeaders.Where(u => u.UserId == userId).ToList();
            //    }
            //}
            return View();
        }

        public IActionResult OrderListAll(string? status)
        {
            var userId = _userManager.GetUserId(User);
            List<UserOrderHeader> orderLists = new List<UserOrderHeader>();
            if (status != null && status != "All")
            {
                if (User.IsInRole("Admin"))
                {
                    orderLists = _db.OrderHeaders.Where(u => u.OrderStatus == status).ToList();
                }
                else
                {
                    orderLists = _db.OrderHeaders.Where(u => u.OrderStatus == status && u.UserId == userId).ToList();
                }
            }
            else
            {
                if (User.IsInRole("Admin"))
                {
                    orderLists = _db.OrderHeaders.ToList();
                }
                else
                {
                    orderLists = _db.OrderHeaders.Where(u => u.UserId == userId).ToList();
                }
            }
            return Json(new { data = orderLists });
        }

        public IActionResult OrderDetails(int id)
        {
            OrderDetailsVM = new OrderDetailsVM();
            OrderDetailsVM.orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            OrderDetailsVM.userProductList = _db.OrderDetails.Include(u => u.Product).Where(u => u.OrderHeaderId == id).ToList();
            return View(OrderDetailsVM);
        }

        public IActionResult InProcess()
        {
            var orderToUpdate = _db.OrderHeaders.FirstOrDefault(u => u.Id == OrderDetailsVM.orderHeader.Id);
            if (orderToUpdate != null)
            {
                orderToUpdate.OrderStatus = UpdateOrderStatus.OrderStatusInProcess;
                _db.OrderHeaders.Update(orderToUpdate);
                _db.SaveChanges();
            }
            return RedirectToAction("OrderDetails", new { id = OrderDetailsVM.orderHeader.Id });
        }

        public IActionResult Shipped()
        {
            var orderToUpdate = _db.OrderHeaders.FirstOrDefault(u => u.Id == OrderDetailsVM.orderHeader.Id);
            if (orderToUpdate != null)
            {
                orderToUpdate.OrderStatus = UpdateOrderStatus.OrderStatusShipped;
                orderToUpdate.Carrier = OrderDetailsVM.orderHeader.Carrier;
                orderToUpdate.TrackingNumber = OrderDetailsVM.orderHeader.TrackingNumber;
                orderToUpdate.DateofShipped = DateTime.Now;
                _db.OrderHeaders.Update(orderToUpdate);
                _db.SaveChanges();
            }
            return RedirectToAction("OrderDetails", new { id = OrderDetailsVM.orderHeader.Id });
        }

        public IActionResult Delivered()
        {
            var orderToUpdate = _db.OrderHeaders.FirstOrDefault(u => u.Id == OrderDetailsVM.orderHeader.Id);
            if (orderToUpdate != null)
            {
                orderToUpdate.OrderStatus = UpdateOrderStatus.OrderStatusCompleted;
                _db.OrderHeaders.Update(orderToUpdate);
                _db.SaveChanges();
            }
            return RedirectToAction("OrderDetails", new { id = OrderDetailsVM.orderHeader.Id });
        }

        public async Task<IActionResult> Canceled(int Id)
        {
            var orderToUpdate = _db.OrderHeaders.FirstOrDefault(u => u.Id == Id);
            if (orderToUpdate != null)
            {
                if (orderToUpdate.PaymentStatus == UpdateOrderStatus.PaymentStatusPaid)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderToUpdate.StripePaymentIntentId,
                    };
                    var service = new RefundService();
                    Refund refund = service.Create(options);
                    orderToUpdate.OrderStatus = UpdateOrderStatus.OrderStatusCanceled;
                    orderToUpdate.PaymentStatus = UpdateOrderStatus.PaymentStatusRefunded;
                }
                _db.OrderHeaders.Update(orderToUpdate);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Refund successfully" });
            }
            return Json(new { success = false, message = "Refund failed" });
        }

        public IActionResult Dashboard()
        {
            var OrderStatusInfo = _db.OrderHeaders.Where(u => u.DateOfOrder >= DateTime.Now.AddDays(-60) && u.DateOfOrder <= DateTime.Now).ToList();
            ViewBag.Shipped = OrderStatusInfo.Where(u => u.OrderStatus == Utility.UpdateOrderStatus.OrderStatusShipped).Count();
            ViewBag.Canceled = OrderStatusInfo.Where(u => u.OrderStatus == Utility.UpdateOrderStatus.OrderStatusCanceled).Count();
            ViewBag.Completed = OrderStatusInfo.Where(u => u.OrderStatus == Utility.UpdateOrderStatus.OrderStatusCompleted).Count();
            ViewBag.Pending = OrderStatusInfo.Where(u => u.OrderStatus == Utility.UpdateOrderStatus.OrderStatusPending).Count();
            ViewBag.InProcess = OrderStatusInfo.Where(u => u.OrderStatus == Utility.UpdateOrderStatus.OrderStatusInProcess).Count();
            return View();
        }

        public List<object> getSalesData()
        {
            List<object> chartData = new List<object>();
            var salesReport = _db.OrderHeaders.Where(u => u.DateOfOrder >= DateTime.Now.AddDays(-30) && u.DateOfOrder <= DateTime.Now).ToList();
            var newSalesReport = salesReport.GroupBy(u => u.DateOfOrder.DayOfWeek.ToString()).Select(u => new
            {
                u.Key,
                total = u.Sum(i => i.TotalOrderAmount),
            }).ToList();
            chartData.Add(newSalesReport.Select(u => u.Key).ToList());
            chartData.Add(newSalesReport.Select(u => u.total).ToList());
            return chartData;
        }

        public List<object> getPopularProductList()
        {
            List<object> chartData = new List<object>();
            var productTop = _db.OrderDetails.Include(u => u.Product).
                Select(u => new
                {
                    ProductName = u.Product.Name,
                    qty = u.Count,
                });

            var mostPopularItems = productTop.GroupBy(u => u.ProductName).Select(u => new
            {
                u.Key,
                totalQuantity = u.Sum(i => i.qty),
            }).OrderByDescending(u => u.totalQuantity).Take(5).ToList();

            chartData.Add(mostPopularItems.Select(u => u.Key).ToList());
            chartData.Add(mostPopularItems.Select(u => u.totalQuantity).ToList());
            return chartData;
        }

        public List<object> getTopBuyerList()
        {
            List<object> chartData = new List<object>();
            var topBuyer = _db.OrderHeaders.Include(u => u.ApplicationUser).Where(u => u.UserId == u.ApplicationUser.Id).
                GroupBy(u => u.ApplicationUser.LastName).Select(u => new
                {
                    u.Key,
                    totalsAmount = u.Sum(i => i.TotalOrderAmount),
                }).OrderByDescending(u => u.totalsAmount).Take(5).ToList();

            chartData.Add(topBuyer.Select(u => u.Key).ToList());
            chartData.Add(topBuyer.Select(u => u.totalsAmount).ToList());
            return chartData;
        }
    }
}
