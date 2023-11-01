using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter category name.")]
        [MaxLength(30, ErrorMessage = "Category name must be 30 charachter.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = null!;

        [Display(Name = "Display Order")]
        [Range(1, 100, ErrorMessage = "Please enter display order between 1 to 100.")]
        public int DisplayOrder { get; set; }
    }
}
