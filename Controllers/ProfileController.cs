using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserProfileManagement.Data;
using UserProfileManagement.Models;

namespace UserProfileManagement.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ApplicationDbContext context,
            ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ======================================================
        // Display User Profile (Supports ID parameter & profile list)
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            try
            {
                _logger.LogInformation("Profile page requested. Specified ID: {UserId}", id);

                // Fetch all profiles for profile switcher UI
                var allProfiles = await _context.UserProfiles
                                                .AsNoTracking()
                                                .OrderBy(u => u.UserId)
                                                .ToListAsync();

                ViewBag.Profiles = allProfiles;

                if (!allProfiles.Any())
                {
                    _logger.LogWarning("No user profiles found in database.");
                    return View("NoProfile");
                }

                // If ID is specified, load that profile; otherwise default to first profile
                UserProfile? user = null;
                if (id.HasValue && id.Value > 0)
                {
                    user = allProfiles.FirstOrDefault(u => u.UserId == id.Value);
                }

                if (user == null)
                {
                    user = allProfiles.First();
                }

                _logger.LogInformation("Profile loaded successfully for UserId {UserId}.", user.UserId);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while loading profile.");
                TempData["ErrorMessage"] = "Something went wrong while loading profiles. Please try again.";
                return View("NoProfile");
            }
        }

        // ======================================================
        // Open Create User Profile Page
        // ======================================================
        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Create profile page requested.");
            return View();
        }

        // ======================================================
        // Create New User Profile (HttpPost)
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,Email,PhoneNumber,DateOfBirth,Address")]
            UserProfile form)
        {
            // Check for duplicate email
            if (!string.IsNullOrWhiteSpace(form.Email) &&
                await _context.UserProfiles.AnyAsync(u => u.Email.ToLower() == form.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "This email address is already registered.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed while creating new user profile.");
                return View(form);
            }

            try
            {
                form.FullName = form.FullName.Trim();
                form.Email = form.Email.Trim();
                form.PhoneNumber = form.PhoneNumber.Trim();
                form.Address = string.IsNullOrWhiteSpace(form.Address) ? null : form.Address.Trim();
                form.CreatedAt = DateTime.Now;
                form.UpdatedAt = DateTime.Now;

                _context.UserProfiles.Add(form);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user profile created successfully with UserId {UserId}.", form.UserId);

                TempData["SuccessMessage"] = $"Profile for '{form.FullName}' created successfully!";
                return RedirectToAction(nameof(Index), new { id = form.UserId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while creating profile.");
                TempData["ErrorMessage"] = "Database error occurred while creating profile. Please try again.";
                return View(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating profile.");
                TempData["ErrorMessage"] = "Something went wrong while creating profile. Please try again.";
                return View(form);
            }
        }

        // ======================================================
        // Open Edit Page
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("Edit profile page requested for UserId {UserId}.", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid profile ID provided: {UserId}.", id);
                    return NotFound();
                }

                var user = await _context.UserProfiles
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    _logger.LogWarning("Profile not found for UserId {UserId}.", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit profile page opened successfully for UserId {UserId}.", id);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while opening edit page for UserId {UserId}.", id);
                TempData["ErrorMessage"] = "Something went wrong. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ======================================================
        // Update User Profile (HttpPost with Security & Validation)
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("UserId,FullName,Email,PhoneNumber,DateOfBirth,Address")]
            UserProfile form)
        {
            if (id <= 0 || id != form.UserId)
            {
                _logger.LogWarning("Invalid profile ID or mismatch detected. Provided ID: {UserId}, Form ID: {FormUserId}.", id, form.UserId);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed while updating profile for UserId {UserId}.", form.UserId);
                return View(form);
            }

            try
            {
                var user = await _context.UserProfiles.FindAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("Profile not found for update for UserId {UserId}.", id);
                    return NotFound();
                }

                user.FullName = form.FullName.Trim();
                user.Email = form.Email.Trim();
                user.PhoneNumber = form.PhoneNumber.Trim();
                user.DateOfBirth = form.DateOfBirth;
                user.Address = string.IsNullOrWhiteSpace(form.Address) ? null : form.Address.Trim();
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Profile successfully updated for UserId {UserId}.", id);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index), new { id = user.UserId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating profile for UserId {UserId}.", id);
                TempData["ErrorMessage"] = "Something went wrong while updating your profile. Please try again.";
                return View(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occurred while updating profile for UserId {UserId}.", id);
                TempData["ErrorMessage"] = "Something went wrong while updating your profile. Please try again.";
                return View(form);
            }
        }
    }
}