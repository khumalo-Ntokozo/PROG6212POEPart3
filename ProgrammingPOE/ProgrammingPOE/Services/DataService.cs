using ProgrammingPOE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingPOE.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _context;

        public DataService(ApplicationDbContext context)
        {
            _context = context;
        }

        // User methods - PRESERVING YOUR EXISTING INTERFACE
        public ApplicationUser AuthenticateUser(string email, string password)
        {
            // For Identity, we'll handle authentication separately
            // This method is kept for compatibility
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public ApplicationUser GetUserById(string id)
        {
            return _context.Users
                .Include(u => u.Claims)
                .FirstOrDefault(u => u.Id == id);
        }

        public List<ApplicationUser> GetUsersByRole(string role)
        {
            return _context.Users
                .Where(u => u.Role == role)
                .OrderBy(u => u.FullName)
                .ToList();
        }

        public void AddUser(ApplicationUser user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        
        public void AddClaim(Claim claim)
        {
            try
            {
                claim.CalculateTotal(); // Auto-calculate amount
                claim.SubmissionDate = DateTime.Now;
                _context.Claims.Add(claim);
                _context.SaveChanges();
                Console.WriteLine($"Claim added successfully. ID: {claim.ClaimId}, Total: {claim.TotalAmount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding claim: {ex.Message}");
                throw;
            }
        }

        public List<Claim> GetAllClaims()
        {
            return _context.Claims
                .Include(c => c.Lecturer)  
                .Include(c => c.Documents)
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();
        }

        public List<Claim> GetClaimsByUser(string userId)
        {
            return _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.LecturerUserId == userId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();
        }

        public List<Claim> GetPendingClaims()
        {
            return _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.Pending)
                .ToList();
        }

        public List<Claim> GetCoordinatorApprovedClaims()
        {
            return _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.CoordinatorApproved)
                .ToList();
        }

        public Claim GetClaimById(int id)
        {
            return _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Documents)
                .FirstOrDefault(c => c.ClaimId == id);
        }

        public void UpdateClaimStatus(int claimId, ClaimStatus newStatus)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.ClaimId == claimId);
            if (claim != null)
            {
                claim.Status = newStatus;
                _context.SaveChanges();
            }
        }

        // Document methods - PRESERVING YOUR EXISTING INTERFACE
        public void AddDocument(SupportingDocument document)
        {
            document.UploadDate = DateTime.Now;
            _context.SupportingDocuments.Add(document);
            _context.SaveChanges();
        }

        public List<SupportingDocument> GetDocumentsByClaimId(int claimId)
        {
            return _context.SupportingDocuments
                .Where(d => d.ClaimId == claimId)
                .ToList();
        }

        // Additional methods for Entity Framework
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public List<ApplicationUser> GetAllUsers()
        {
            return _context.Users
                .OrderBy(u => u.FullName)
                .ToList();
        }

        public void UpdateUser(ApplicationUser user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
    }
}