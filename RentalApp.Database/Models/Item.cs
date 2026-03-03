using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;



namespace RentalApp.Database.Models
{
    [Table("items")]
    [PrimaryKey(nameof(Id))]
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DailyRate { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required]
        public int OwnerId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(OwnerId))]
        public User? Owner { get; set; }

        public List<Rental> Rentals { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();

        [NotMapped]
        public string Category { get; set; } = string.Empty;
    }
}
