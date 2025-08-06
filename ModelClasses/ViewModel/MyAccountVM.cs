using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClasses.ViewModel
{
    public class MyAccountVM
    {
        public ApplicationUser ApplicationUser { get; set; }

        public string? StatusMessage { get; set; }

        // Có thể thêm một số trường lặp lại để bind riêng nếu cần
        public string? Email => ApplicationUser?.Email;

        public string? UserName => ApplicationUser?.UserName;
    }
}
