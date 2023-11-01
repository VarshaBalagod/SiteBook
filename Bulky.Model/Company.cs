using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Company
    {        
        [Key]       
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company name")]
        public string Name { get; set; } = null!;

        [Display(Name = "Street address")]
        public string? StreetAddress { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        [Display(Name = "Postal code")]
        public string? PostalCode { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }
}
