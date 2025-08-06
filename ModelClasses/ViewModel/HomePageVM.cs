using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClasses.ViewModel
{
    public class HomePageVM
    {
        public IEnumerable<Product> ProductList { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public string searchByName { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int TotalProductCount { get; set; }
        public IEnumerable<Product> AllProducts { get; set; }

        public IEnumerable<Wishlist> Wishlists { get; set; }
        public IEnumerable<Inventory> Inventories { get; set; }
        public IEnumerable<Cart> CartList { get; set; }
    }
}
