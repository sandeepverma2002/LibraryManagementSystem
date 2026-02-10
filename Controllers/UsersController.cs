using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;

                if (!string.IsNullOrEmpty(searchString))
                {
                    var users = await _context.Users
                        .Where(u => u.FirstName.Contains(searchString) ||
                                   u.LastName.Contains(searchString) ||
                                   u.Email.Contains(searchString) ||
                                   u.MembershipNumber.Contains(searchString))
                        .OrderBy(u => u.LastName)
                        .ToListAsync();
                    return View(users);
                }

                var allUsers = await _context.Users
                    .OrderBy(u => u.LastName)
                    .ToListAsync();
                return View(allUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading users");
                TempData["ErrorMessage"] = "An error occurred while loading users.";
                return View(new List<User>());
            }
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Phone")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "A user with this email already exists.");
                        return View(user);
                    }

                    // Generate unique membership number
                    var lastUser = await _context.Users
                        .OrderByDescending(u => u.Id)
                        .FirstOrDefaultAsync();

                    var nextNumber = lastUser != null ?
                        int.Parse(lastUser.MembershipNumber.Substring(3)) + 1 : 1;
                    user.MembershipNumber = $"MEM{nextNumber:D3}";

                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"User added successfully! Membership Number: {user.MembershipNumber}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating user");
                    ModelState.AddModelError("", "An error occurred while creating the user: " + ex.Message);
                }
            }
            return View(user);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Transactions)
                .ThenInclude(t => t.Book)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}