using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace RentalApp.Database.Models
{
    public enum RentalStatus
    {
        Requested,
        Approved,
        Rejected,
        OutForRent,
        Overdue,
        Returned,
        Completed
    }

    [Table("rentals")]
    [PrimaryKey(nameof(Id))]
    public class Rental
    {
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public int BorrowerId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public RentalStatus Status { get; set; } = RentalStatus.Requested;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        [ForeignKey(nameof(BorrowerId))]
        public User? Borrower { get; set; }

    }
}
