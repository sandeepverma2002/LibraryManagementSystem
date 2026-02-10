using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public interface ITransactionService
    {
        Task<Transaction> IssueBookAsync(int bookId, int userId);
        Task<Transaction> ReturnBookAsync(int transactionId);
        Task<IEnumerable<Transaction>> GetTransactionsAsync();
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId);
        Task<IEnumerable<Transaction>> GetOverdueTransactionsAsync();
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<int> GetTotalIssuedBooksCountAsync();
    }
}