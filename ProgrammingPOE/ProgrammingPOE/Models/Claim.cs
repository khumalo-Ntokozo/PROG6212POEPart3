using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgrammingPOE.Models
{
    public enum ClaimStatus
    {
        Pending,
        CoordinatorApproved,
        CoordinatorRejected,
        ManagerApproved,
        ManagerRejected
    }

    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        public string LecturerUserId { get; set; }

        [Required]
        [Range(0.1, 1000, ErrorMessage = "Hours worked must be between 0.1 and 1000")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Hourly rate must be between 1 and 1000")]
        public decimal HourlyRate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        // Navigation properties - make them virtual but don't require them
        [ForeignKey("LecturerUserId")]
        public virtual ApplicationUser Lecturer { get; set; }

        public virtual ICollection<SupportingDocument> Documents { get; set; } = new List<SupportingDocument>();

        public void CalculateTotal()
        {
            TotalAmount = HoursWorked * HourlyRate;
        }
    }
}