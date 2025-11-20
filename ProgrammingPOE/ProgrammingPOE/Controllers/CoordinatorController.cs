using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgrammingPOE.Models;
using ProgrammingPOE.Services;
using ProgrammingPOE.ViewModels;

namespace ProgrammingPOE.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        public CoordinatorController(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        public IActionResult ReviewClaims()
        {
            // Remove the manual role check - the [Authorize] attribute handles this
            var claims = _dataService.GetPendingClaims();

            // Create view models with lecturer names
            var claimViewModels = new List<ClaimViewModel>();
            foreach (var claim in claims)
            {
                var user = _dataService.GetUserById(claim.LecturerUserId);
                claim.Documents = _dataService.GetDocumentsByClaimId(claim.ClaimId);

                claimViewModels.Add(new ClaimViewModel
                {
                    Claim = claim,
                    LecturerName = user?.FullName ?? "Unknown Lecturer"
                });
            }

            return View(claimViewModels);
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            // Remove the manual role check
            _dataService.UpdateClaimStatus(id, ClaimStatus.CoordinatorApproved);
            TempData["Message"] = $"Claim {id} approved by Coordinator.";
            return RedirectToAction(nameof(ReviewClaims));
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            // Remove the manual role check
            _dataService.UpdateClaimStatus(id, ClaimStatus.CoordinatorRejected);
            TempData["Message"] = $"Claim {id} rejected by Coordinator.";
            return RedirectToAction(nameof(ReviewClaims));
        }
    }
}