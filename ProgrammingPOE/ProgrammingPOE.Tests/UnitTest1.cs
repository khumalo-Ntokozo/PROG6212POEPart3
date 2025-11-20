using Xunit;
using ProgrammingPOE.Models;
using ProgrammingPOE.Tests.Services;


namespace ProgrammingPOE.Tests
{
    
    public class ClaimTests
    {
        [Fact]
        public void CalculateTotalAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 500
            };

            // Act
            var expectedTotal = 10 * 500; // 5000
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            // Assert
            Assert.Equal(5000, claim.TotalAmount);
            Assert.Equal(expectedTotal, claim.TotalAmount);
        }

        [Fact]
        public void ClaimWithNotes_ShouldStoreAndRetrieveNotesCorrectly()
        {
            // Arrange
            var claim = new Claim
            {
                Notes = "Additional teaching hours for workshop preparation"
            };

            // Act & Assert
            Assert.NotNull(claim.Notes);
            Assert.Equal("Additional teaching hours for workshop preparation", claim.Notes);
            Assert.False(string.IsNullOrEmpty(claim.Notes));
        }

        [Fact]
        public void SubmitClaim_WithValidData_ShouldCreateClaimSuccessfully()
        {
            // Arrange
            var dataService = new TestDataService();
            var claim = new Claim
            {
                LecturerUserId = "user123",
                HoursWorked = 8,
                HourlyRate = 450,
                Notes = "Weekly teaching hours",
                Status = ClaimStatus.Pending
            };

            // Act
            dataService.AddClaim(claim);

            // Assert
            var retrievedClaim = dataService.GetClaimById(claim.ClaimId);
            Assert.NotNull(retrievedClaim);
            Assert.Equal(8, retrievedClaim.HoursWorked);
            Assert.Equal(450, retrievedClaim.HourlyRate);
            Assert.Equal(ClaimStatus.Pending, retrievedClaim.Status);
        }

        [Fact]
        public void ClaimSubmissionDate_ShouldBeSetAutomatically()
        {
            // Arrange & Act
            var claim = new Claim();
            var currentTime = DateTime.Now;

            // Assert
            Assert.True(claim.SubmissionDate <= currentTime);
            Assert.True(claim.SubmissionDate > currentTime.AddMinutes(-1));
        }

        [Fact]
        public void ClaimWithZeroHours_ShouldHaveZeroTotalAmount()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 0,
                HourlyRate = 500
            };

            // Act
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            // Assert
            Assert.Equal(0, claim.TotalAmount);
        }
    }

    public class SupportingDocumentTests
    {
        [Fact]
        public void Document_ShouldBeLinkedToClaim()
        {
            // Arrange
            var claim = new Claim { ClaimId = 1 };
            var document = new SupportingDocument
            {
                ClaimId = claim.ClaimId,
                FileName = "timesheet.pdf",
                FilePath = "/uploads/timesheet.pdf"
            };

            // Act & Assert
            Assert.Equal(claim.ClaimId, document.ClaimId);
            Assert.NotNull(document.FileName);
            Assert.NotNull(document.FilePath);
        }

        [Fact]
        public void Document_WithValidFileInfo_ShouldCreateSuccessfully()
        {
            // Arrange
            var document = new SupportingDocument
            {
                FileName = "document.pdf",
                FilePath = "/uploads/document.pdf",
                FileSize = 1024
            };

            // Act & Assert
            Assert.Equal("document.pdf", document.FileName);
            Assert.Equal("/uploads/document.pdf", document.FilePath);
            Assert.Equal(1024, document.FileSize);
        }
    }
}