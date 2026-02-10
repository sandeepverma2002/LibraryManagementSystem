using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookService _bookService;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ApplicationDbContext context, IBookService bookService, ILogger<TransactionService> logger)
        {
            _context = context;
            _bookService = bookService;
            _logger = logger;
        }

        public async Task<Transaction> IssueBookAsync(int bookId, int userId)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(bookId);
                if (book == null)
                    throw new KeyNotFoundException($"Book with ID {bookId} not found");

                if (book.AvailableCopies <= 0)
                    throw new InvalidOperationException("No copies available for issue");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found");

                // Check if user has already issued this book
                var existingIssue = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.BookId == bookId &&
                                              t.UserId == userId &&
                                              t.Status == "Issued");

                if (existingIssue != null)
                    throw new InvalidOperationException("User has already issued this book");

                // Create transaction
                var transaction = new Transaction
                {
                    BookId = bookId,
                    UserId = userId,
                    IssueDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    Status = "Issued",
                    FineAmount = 0
                };

                // Update book available copies
                book.AvailableCopies--;
                book.UpdatedDate = DateTime.UtcNow;

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while issuing book {bookId} to user {userId}");
                throw;
            }
        }

        public async Task<Transaction> ReturnBookAsync(int transactionId)
        {
            try
            {
                var transaction = await GetTransactionByIdAsync(transactionId);
                if (transaction == null)
                    throw new KeyNotFoundException($"Transaction with ID {transactionId} not found");

                if (transaction.Status == "Returned")
                    throw new InvalidOperationException("Book is already returned");

                var book = await _bookService.GetBookByIdAsync(transaction.BookId);
                if (book == null)
                    throw new KeyNotFoundException($"Book with ID {transaction.BookId} not found");

                // Update transaction
                transaction.ReturnDate = DateTime.UtcNow;
                transaction.Status = "Returned";

                // Calculate fine if overdue
                if (transaction.ReturnDate > transaction.DueDate)
                {
                    var overdueDays = (transaction.ReturnDate.Value - transaction.DueDate).Days;
                    transaction.FineAmount = overdueDays * 10; // $10 per day fine
                    transaction.Status = "Overdue";
                }

                // Update book available copies
                book.AvailableCopies++;
                book.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while returning book for transaction {transactionId}");
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync()
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Include(t => t.User)
                    .OrderByDescending(t => t.IssueDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all transactions");
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId)
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.IssueDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting transactions for user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetOverdueTransactionsAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Include(t => t.User)
                    .Where(t => t.Status == "Issued" && t.DueDate < currentDate)
                    .OrderBy(t => t.DueDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting overdue transactions");
                throw;
            }
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.Book)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting transaction with ID {id}");
                throw;
            }
        }

        public async Task<int> GetTotalIssuedBooksCountAsync()
        {
            try
            {
                return await _context.Transactions
                    .CountAsync(t => t.Status == "Issued");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting total issued books count");
                throw;
            }
        }
    }
}