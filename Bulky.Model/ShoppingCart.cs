using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class ShoppingCart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShopCrtId { get; set; }

        public int ProductId {  get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; } = null!;



        [Range(1,1000,ErrorMessage ="Please enter a value between 1 and 1000")]
        public int Count { get; set; }



        public string ApplicationUserId { get; set; } = null!;
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; } = null!;

        [NotMapped]
        public double TPrice { get; set; }
        [ValidateNever]
        [NotMapped]
        public string CoverImage { get; set; }
    }
}
