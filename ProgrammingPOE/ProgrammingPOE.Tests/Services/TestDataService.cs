using ProgrammingPOE.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProgrammingPOE.Tests.Services
{
    public class TestDataService
    {
        private List<Claim> _claims = new List<Claim>();
        private int _nextClaimId = 1;

        public void AddClaim(Claim claim)
        {
            claim.ClaimId = _nextClaimId++;
            _claims.Add(claim);
        }

        public Claim GetClaimById(int claimId)
        {
            return _claims.FirstOrDefault(c => c.ClaimId == claimId);
        }

        public List<Claim> GetClaimsByUser(string userId)
        {
            return _claims.Where(c => c.LecturerUserId == userId).ToList();
        }

        public List<Claim> GetClaimsByStatus(ClaimStatus status)
        {
            return _claims.Where(c => c.Status == status).ToList();
        }
    }
}