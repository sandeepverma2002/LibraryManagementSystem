using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IBookService _bookService;
        private readonly ApplicationDbContext _context;

        public TransactionsController(
            ITransactionService transactionService,
            IBookService bookService,
            ApplicationDbContext context)
        {
            _transactionService = transactionService;
            _bookService = bookService;
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsAsync();
                return View(transactions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading transactions.";
                return View(new List<Transaction>());
            }
        }

        // GET: Transactions/Issue
        //public async Task<IActionResult> Issue()
        //{
        //    try
        //    {
        //        ViewBag.Books = await _context.Books
        //            .Where(b => b.AvailableCopies > 0)
        //            .OrderBy(b => b.Title)
        //            .ToListAsync();

        //        ViewBag.Users = await _context.Users
        //            .OrderBy(u => u.FirstName)
        //            .ToListAsync();

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "An error occurred while loading data.";
        //        return View();
        //    }
        //}

        // POST: Transactions/Issue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int bookId, int userId)
        {
            try
            {
                var transaction = await _transactionService.IssueBookAsync(bookId, userId);
                TempData["SuccessMessage"] = $"Book issued successfully! Due date: {transaction.DueDate:yyyy-MM-dd}";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while issuing the book: " + ex.Message;
            }

            return RedirectToAction(nameof(Issue));
        }

        // GET: Transactions/Return/5
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id.Value);
                if (transaction == null)
                {
                    return NotFound();
                }

                return View(transaction);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading transaction details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Transactions/Return/5
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(int id)
        {
            try
            {
                var transaction = await _transactionService.ReturnBookAsync(id);

                if (transaction.FineAmount > 0)
                {
                    TempData["SuccessMessage"] = $"Book returned successfully! Fine amount: ${transaction.FineAmount}";
                }
                else
                {
                    TempData["SuccessMessage"] = "Book returned successfully!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while returning the book: " + ex.Message;
            }

            return RedirectToAction(nameof(Return), new { id });
        }

        // GET: Transactions/Issue
        public async Task<IActionResult> Issue()
        {
            try
            {
                var books = await _context.Books
                    .Where(b => b.AvailableCopies > 0)
                    .OrderBy(b => b.Title)
                    .ToListAsync();

                var users = await _context.Users
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();

                var model = (Books: books, Users: users);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading data.";
                return View();
            }
        }

        // GET: Transactions/Overdue
        public async Task<IActionResult> Overdue()
        {
            try
            {
                var overdueTransactions = await _transactionService.GetOverdueTransactionsAsync();
                return View(overdueTransactions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading overdue transactions.";
                return View(new List<Transaction>());
            }
        }

        // GET: Dashboard/Statistics
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalBooks = await _context.Books.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var issuedBooks = await _transactionService.GetTotalIssuedBooksCountAsync();
                var overdueBooks = await _context.Transactions.CountAsync(t => t.Status == "Overdue");

                var viewModel = new DashboardViewModel
                {
                    TotalBooks = totalBooks,
                    TotalUsers = totalUsers,
                    IssuedBooks = issuedBooks,
                    OverdueBooks = overdueBooks,
                    AvailableBooks = totalBooks - issuedBooks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading dashboard statistics.";
                return View(new DashboardViewModel());
            }
        }
    }

    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int IssuedBooks { get; set; }
        public int OverdueBooks { get; set; }
        public int AvailableBooks { get; set; }
    }
}