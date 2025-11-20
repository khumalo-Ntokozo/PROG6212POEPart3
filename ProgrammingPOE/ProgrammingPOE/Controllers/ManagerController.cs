using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgrammingPOE.Models;
using ProgrammingPOE.Services;
using ProgrammingPOE.ViewModels;

namespace ProgrammingPOE.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        public ManagerController(DataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        public IActionResult FinalApproval()
        {
            if (!_authService.IsInRole("Manager"))
                return RedirectToAction("Login", "Account");

            var claims = _dataService.GetCoordinatorApprovedClaims();

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
            if (!_authService.IsInRole("Manager"))
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, ClaimStatus.ManagerApproved);
            TempData["Message"] = $"Claim {id} approved by Manager.";
            return RedirectToAction(nameof(FinalApproval));
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            if (!_authService.IsInRole("Manager"))
                return RedirectToAction("Login", "Account");

            _dataService.UpdateClaimStatus(id, ClaimStatus.ManagerRejected);
            TempData["Message"] = $"Claim {id} rejected by Manager.";
            return RedirectToAction(nameof(FinalApproval));
        }
    }
}