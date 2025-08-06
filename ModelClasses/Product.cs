using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ModelClasses
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]

        public string Name { get; set; }

        [Range(1, 99999999, ErrorMessage = "Price must be between 1 and 99,999,999.99")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Only 0 to 2 decimal places are allowed. Example: 100 or 100.00")]
        [Column(TypeName = "decimal(18,2)")]
        public double Price { get; set; }

        [Required]
        [MaxLength(2000, ErrorMessage = "Length can not exist more than 30 characters")]
        public string Description { get; set; }

        public ICollection<PImages>? ImgUrls { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        public string? HomeImgUrl { get; set; }
    }
}
