using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BankSysAPI.Data;
using BankSysAPI.Models;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly BankingDbContext _context;

        public TransactionController(BankingDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-account/{accountId}")]
        [Authorize]
        public async Task<IActionResult> GetTransactionsByAccount(int accountId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Bu kullanıcıya ait mi kontrolü
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
            if (account == null)
                return Unauthorized("Bu hesaba erişim izniniz yok.");

            // Bu hesaba gelen tüm transferleri bul
            var transactions = await _context.Transactions
                .Where(t => t.ReceiverAccountId == accountId)
                .Join(_context.Accounts,
                      t => t.SenderAccountId,
                      a => a.Id,
                      (t, senderAccount) => new
                      {
                          t.Id,
                          Amount = t.Amount,
                          Timestamp = t.Timestamp,
                          SenderFullName = _context.Users
                              .Where(u => u.Id == senderAccount.UserId)
                              .Select(u => u.Username)
                              .FirstOrDefault()
                      })
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
