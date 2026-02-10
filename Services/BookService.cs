using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookService> _logger;

        public BookService(ApplicationDbContext context, ILogger<BookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            try
            {
                return await _context.Books
                    .OrderBy(b => b.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all books");
                throw;
            }
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            try
            {
                return await _context.Books.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting book with ID {id}");
                throw;
            }
        }

        public async Task<Book?> GetBookByISBNAsync(string isbn)
        {
            try
            {
                return await _context.Books
                    .FirstOrDefaultAsync(b => b.ISBN == isbn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting book with ISBN {isbn}");
                throw;
            }
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllBooksAsync();

                return await _context.Books
                    .Where(b => b.Title.Contains(searchTerm) ||
                                b.Author.Contains(searchTerm) ||
                                b.ISBN.Contains(searchTerm) ||
                                b.Genre.Contains(searchTerm) ||
                                b.Publisher.Contains(searchTerm))
                    .OrderBy(b => b.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while searching books with term: {searchTerm}");
                throw;
            }
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            try
            {
                book.CreatedDate = DateTime.UtcNow;
                book.UpdatedDate = DateTime.UtcNow;
                book.AvailableCopies = book.TotalCopies;

                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new book");
                throw;
            }
        }

        public async Task<Book> UpdateBookAsync(Book book)
        {
            try
            {
                var existingBook = await GetBookByIdAsync(book.Id);
                if (existingBook == null)
                    throw new KeyNotFoundException($"Book with ID {book.Id} not found");

                // Update available copies if total copies changed
                if (existingBook.TotalCopies != book.TotalCopies)
                {
                    var issuedCopies = existingBook.TotalCopies - existingBook.AvailableCopies;
                    book.AvailableCopies = Math.Max(0, book.TotalCopies - issuedCopies);
                }
                else
                {
                    book.AvailableCopies = existingBook.AvailableCopies;
                }

                book.UpdatedDate = DateTime.UtcNow;
                book.CreatedDate = existingBook.CreatedDate;

                _context.Entry(existingBook).CurrentValues.SetValues(book);
                await _context.SaveChangesAsync();
                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating book with ID {book.Id}");
                throw;
            }
        }

        public async Task DeleteBookAsync(int id)
        {
            try
            {
                var book = await GetBookByIdAsync(id);
                if (book == null)
                    throw new KeyNotFoundException($"Book with ID {id} not found");

                // Check if book is currently issued
                var activeTransactions = await _context.Transactions
                    .Where(t => t.BookId == id && t.Status == "Issued")
                    .ToListAsync();

                if (activeTransactions.Any())
                    throw new InvalidOperationException("Cannot delete book that has active transactions");

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting book with ID {id}");
                throw;
            }
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(e => e.Id == id);
        }
    }
}