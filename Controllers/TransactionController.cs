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
        public async Task<IActionResult> GetTransactionsByAccount(
            int accountId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null)
                return Unauthorized("Bu hesaba erişim izniniz yok.");

            var transactionsQuery = _context.Transactions
                .Where(t => t.SenderAccountId == accountId || t.ReceiverAccountId == accountId)
                .AsQueryable();

            if (start.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Timestamp >= start.Value);

            if (end.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Timestamp <= end.Value);

            var transactions = await transactionsQuery
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

            var results = new List<TransactionViewModel>();

            foreach (var t in transactions)
            {
                bool isIncoming = t.ReceiverAccountId == accountId;
                int counterpartyAccountId = isIncoming ? t.SenderAccountId ?? 0 : t.ReceiverAccountId;

                var counterpartyAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == counterpartyAccountId);
                var counterpartyUser = counterpartyAccount != null
                    ? await _context.Users.FirstOrDefaultAsync(u => u.Id == counterpartyAccount.UserId)
                    : null;

                results.Add(new TransactionViewModel
                {
                    Direction = isIncoming ? "Gelen" : "Giden",
                    CounterpartyName = counterpartyUser?.Username ?? "Bilinmiyor",
                    CounterpartyIban = counterpartyAccount?.IBAN ?? "-",
                    Amount = t.Amount,
                    Timestamp = t.Timestamp
                });
            }

            return Ok(results);
        }



    }
}
