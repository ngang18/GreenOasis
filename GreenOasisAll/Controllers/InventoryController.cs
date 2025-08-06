using DatabaseAccess;
using Microsoft.AspNetCore.Mvc;

namespace GreenOasisAll.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult getInventoryList()
        {
            var inventoryList = _context.Inventories.ToList();
            return Json(new {data = inventoryList});
        }
    }
}
