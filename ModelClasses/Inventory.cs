using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClasses
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }
        [Required]

        public string Name { get; set; }
        [Required]
        [Range(1, 99999999, ErrorMessage = "Price must be between 1 and 99,999,999.99")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Only 0 to 2 decimal places are allowed. Example: 100 or 100.00")]
        [Column(TypeName = "decimal(18,2)")]
        public double Purchase_Price { get; set; }

        public string Category { get; set; }
        public int Quantity { get; set; }
    }
}
