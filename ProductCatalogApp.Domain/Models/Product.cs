using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogApp.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime StartDate { get; set; }
        public int DurationInDays { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        // Fields for update logging
        public DateTime? LastUpdatedDateTime { get; set; } // Tracks the last update time
        public string? LastUpdatedByUserId { get; set; }  // Tracks the user who last updated the product

    }
}
