using Microsoft.AspNetCore.Mvc;
using ProgrammingPOE.Models;
using ProgrammingPOE.Services;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ProgrammingPOE.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly DataService _dataService;
        private readonly AuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public LecturerController(DataService dataService, AuthService authService,
                                UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _dataService = dataService;
            _authService = authService;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> SubmitClaim()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserHourlyRate = currentUser?.HourlyRate ?? 0;

            var claim = new Claim
            {
                HourlyRate = currentUser?.HourlyRate ?? 0,
                HoursWorked = 0,
                TotalAmount = 0
            };

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile upload)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserHourlyRate = currentUser?.HourlyRate ?? 0;

            // Remove Lecturer from ModelState validation since we set it manually
            ModelState.Remove("Lecturer");
            ModelState.Remove("LecturerUserId");
            ModelState.Remove("TotalAmount");
            ModelState.Remove("SubmissionDate");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the properties that aren't bound from the form
                    claim.LecturerUserId = currentUser.Id;
                    claim.CalculateTotal(); // Auto-calculate the total
                    claim.Status = ClaimStatus.Pending;
                    claim.SubmissionDate = DateTime.Now;

                    _dataService.AddClaim(claim);

                    // Handle file upload
                    if (upload != null && upload.Length > 0)
                    {
                        // Validate file size (5MB limit)
                        if (upload.Length > 5 * 1024 * 1024)
                        {
                            TempData["Error"] = "File is too large. Maximum size is 5MB.";
                            return View(claim);
                        }

                        var fileExtension = Path.GetExtension(upload.FileName).ToLower();
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xlsx", ".xls", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            TempData["Error"] = "File type not allowed. Please upload PDF, Word, Excel, Image, or Text files.";
                            return View(claim);
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await upload.CopyToAsync(fileStream);
                        }

                        var document = new SupportingDocument
                        {
                            ClaimId = claim.ClaimId,
                            FileName = upload.FileName,
                            FilePath = $"/uploads/{uniqueFileName}",
                            FileSize = upload.Length,
                            UploadDate = DateTime.Now
                        };

                        _dataService.AddDocument(document);
                    }

                    TempData["Message"] = $"Claim submitted successfully! Total amount: R {claim.TotalAmount:F2}";
                    return RedirectToAction(nameof(TrackClaims));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"An error occurred while submitting your claim: {ex.Message}";
                    return View(claim);
                }
            }
            else
            {
                // If model state is invalid, show validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Please fix the following errors: " + string.Join(", ", errors);
            }

            return View(claim);
        }

        public async Task<IActionResult> TrackClaims()
        {
            var currentUserId = _userManager.GetUserId(User);
            var claims = _dataService.GetClaimsByUser(currentUserId);

            // Attach documents to each claim
            foreach (var claim in claims)
            {
                claim.Documents = _dataService.GetDocumentsByClaimId(claim.ClaimId);
            }

            return View(claims);
        }
    }
}