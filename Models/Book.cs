using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(150, ErrorMessage = "Author name cannot exceed 150 characters")]
        public string Author { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
        public string ISBN { get; set; }

        [StringLength(150, ErrorMessage = "Publisher name cannot exceed 150 characters")]
        public string? Publisher { get; set; }

        [Range(1000, 9999, ErrorMessage = "Please enter a valid year")]
        public int? PublishedYear { get; set; }

        [StringLength(100, ErrorMessage = "Genre cannot exceed 100 characters")]
        public string? Genre { get; set; }

        [Required(ErrorMessage = "Total copies is required")]
        [Range(1, 1000, ErrorMessage = "Total copies must be between 1 and 1000")]
        public int TotalCopies { get; set; } = 1;

        [Required(ErrorMessage = "Available copies is required")]
        [Range(0, 1000, ErrorMessage = "Available copies must be between 0 and 1000")]
        public int AvailableCopies { get; set; } = 1;

       public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
