using DatabaseAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelClasses;

namespace GreenOasisAll.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var items = _context.Categories.ToList();

            return View(items);
        }
        [Authorize]
        public IActionResult Upsert(int? id)
        {
            if (id == 0)
            {
                Category category = new Category();
                return View(category);
            }
            else
            {

                var items = _context.Categories.FirstOrDefault(u => u.Id == id);
                return View(items);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(int? id, Category category)
        {
            if (id == null)
            {
                var foundItem = await _context.Categories.FirstOrDefaultAsync(u => u.Name == category.Name);
                if (foundItem != null)
                {
                    TempData["AlertMessage"] = category.Name + " is an existing item found in the list, so not added to the list";
                    return RedirectToAction("Index");
                }
                await _context.Categories.AddAsync(category);
                TempData["AlertMessage"] = category.Name + " has added to the category list";
            }
            else
            {
                var items = await _context.Categories.FirstOrDefaultAsync(u => u.Id == id);
                items.Name = category.Name;
                TempData["AlertMessage"] = category.Name + " has edited to the category list";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var items = _context.Categories.FirstOrDefault(u => u.Id == id);
            return View(items);

        }

        [HttpPost]
        public async Task<IActionResult> Delete(Category category)
        {
            var items = _context.Categories.FirstOrDefault(u => u.Id == category.Id);
            _context.Categories.Remove(items);
            TempData["AlertMessage"] = category.Name + " has deleted to the category list";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
