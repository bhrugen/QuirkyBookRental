using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuirkyBookRental.Models
{
    public class Book
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        [Required]
        [Range(0,1000)]
        public int Avaibility { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:MMM dd yyyy}")]
        public DateTime? DateAdded { get; set; }

        [Required]
        public int GenreId { get; set; }

        public Genre Genre { get; set; }

        [Required]
        public string Publisher { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM dd yyyy}")]
        public DateTime PublicationDate { get; set; }

        [Required]
        [Display(Name ="Pages")]
        public int Pages { get; set; }

        [Required]
        [Display(Name = "Product Dimensions")]
        public string ProductDimensions { get; set; }
    }
}