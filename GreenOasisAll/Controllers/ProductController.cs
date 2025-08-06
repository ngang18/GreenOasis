using DatabaseAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModelClasses;
using ModelClasses.ViewModel;

namespace GreenOasisAll.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _HostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _HostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            var product = _context.Products.Include(u => u.Category).ToList();
            return View(product);
        }

        public IActionResult ProductsByCategory(int? categoryId)
        {
            var categories = _context.Categories.ToList();
            var allProducts = _context.Products.ToList();

            var filteredProducts = allProducts;

            if (categoryId.HasValue)
            {
                filteredProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            var vm = new HomePageVM
            {
                Categories = categories,
                ProductList = filteredProducts,
                AllProducts = allProducts,
                SelectedCategoryId = categoryId,
                TotalProductCount = allProducts.Count
            };

            return View(vm);
        }


        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productsVM = new ProductVM()
            {
                Inventories = new Inventory(),
                PImages = new PImages(),
                CategoriesList = _context.Categories.ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
            };
            return View(productsVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM productVM)
        {
            if (productVM.Images != null && productVM.Images.Any())
            {
                // 1. Lưu ảnh đại diện
                var homeImage = productVM.Images.First();
                string homeImageFileName = UploadFiles(homeImage);
                productVM.Products.HomeImgUrl = homeImageFileName;

                // 2. Lưu sản phẩm
                await _context.Products.AddAsync(productVM.Products);
                await _context.SaveChangesAsync();

                var newProduct = productVM.Products;

                // 3. Lưu vào Inventory
                productVM.Inventories.Name = newProduct.Name;
                productVM.Inventories.Category = _context.Categories.FirstOrDefault(c => c.Id == newProduct.CategoryId)?.Name;
                await _context.Inventories.AddAsync(productVM.Inventories);

                // 4. Lưu ảnh còn lại
                foreach (var image in productVM.Images.Skip(1))
                {
                    string fileName = UploadFiles(image);
                    var imageEntity = new PImages
                    {
                        ImageUrl = fileName,
                        ProductId = newProduct.Id,
                        ProductName = newProduct.Name
                    };
                    await _context.PImages.AddAsync(imageEntity);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Product");
            }

            ModelState.AddModelError("", "Vui lòng chọn ít nhất một ảnh.");
            return View(productVM);
        }

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            ProductVM productsVM = new ProductVM()
            {
                Products = _context.Products.FirstOrDefault(p => p.Id == Id),
                CategoriesList = _context.Categories.ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
            };
            productsVM.Products.ImgUrls = _context.PImages.Where(u => u.ProductId == Id).ToList();

            return View(productsVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM)
        {
            var product = _context.Products.FirstOrDefault(u => u.Id == productVM.Products.Id);
            if (product == null)
                return NotFound();

            // Cập nhật thông tin cơ bản
            product.Name = productVM.Products.Name;
            product.Price = productVM.Products.Price;
            product.Description = productVM.Products.Description;
            product.CategoryId = productVM.Products.CategoryId;

            // Nếu có ảnh mới
            if (productVM.Images != null && productVM.Images.Any())
            {
                // Xoá ảnh đại diện cũ nếu có
                if (!string.IsNullOrEmpty(product.HomeImgUrl))
                {
                    var oldHomePath = Path.Combine(_HostEnvironment.WebRootPath, "Images", product.HomeImgUrl);
                    DeleteAImage(oldHomePath);
                }

                // Ảnh đầu tiên là đại diện mới
                var newHomeImage = productVM.Images.First();
                product.HomeImgUrl = UploadFiles(newHomeImage);

                // Ảnh còn lại -> thêm vào PImages
                foreach (var img in productVM.Images.Skip(1))
                {
                    string fileName = UploadFiles(img);
                    var newImage = new PImages
                    {
                        ImageUrl = fileName,
                        ProductId = product.Id,
                        ProductName = product.Name
                    };
                    _context.PImages.Add(newImage);
                }
            }

            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("Index", "Product");
        }

        [HttpDelete]
        public IActionResult Delete(int Id)
        {
            if (Id != 0)
            {
                var productToDelete = _context.Products.FirstOrDefault(x => x.Id == Id);
                var imagesToDelete = _context.PImages.Where(u => u.ProductId == Id).Select(u => u.ImageUrl);

                foreach (var image in imagesToDelete)
                {
                    var imagePath = Path.Combine(_HostEnvironment.WebRootPath, "Images", image);
                    DeleteAImage(imagePath);
                }

                if (!string.IsNullOrEmpty(productToDelete?.HomeImgUrl))
                {
                    var homeImagePath = Path.Combine(_HostEnvironment.WebRootPath, "Images", productToDelete.HomeImgUrl);
                    DeleteAImage(homeImagePath);
                }

                _context.Products.Remove(productToDelete);
                _context.SaveChanges();
                return Json(new { success = true, message = "Deleted successfully" });
            }

            return Json(new { success = false, message = "Failed to delete the item" });
        }

        public IActionResult DeleteAImg(string Id)
        {
            int routeId = 0;

            if (!string.IsNullOrEmpty(Id))
            {
                if (_context.PImages.Any(p => p.ImageUrl == Id))
                {
                    var img = _context.PImages.First(p => p.ImageUrl == Id);
                    routeId = img.ProductId;
                    _context.PImages.Remove(img);
                }
                else if (_context.Products.Any(p => p.HomeImgUrl == Id))
                {
                    var product = _context.Products.First(p => p.HomeImgUrl == Id);
                    routeId = product.Id;
                    product.HomeImgUrl = "";
                    _context.Products.Update(product);
                }

                var path = Path.Combine(_HostEnvironment.WebRootPath, "Images", Id);
                DeleteAImage(path);
                _context.SaveChanges();

                return Json(new { success = true, message = "Picture was deleted successfully", id = routeId });
            }

            return Json(new { success = false, message = "Failed to delete the item." });
        }

        private void DeleteAImage(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        private string UploadFiles(IFormFile image)
        {
            string fileName = null;
            if (image != null)
            {
                string uploadDir = Path.Combine(_HostEnvironment.WebRootPath, "Images");
                fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }
            }
            return fileName;
        }

    }
}
