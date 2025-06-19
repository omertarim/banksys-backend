using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankSysAPI.Data;
using BankSysAPI.Models;
using System.Security.Claims;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly BankingDbContext _context;

        public AccountController(BankingDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}/balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance(int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return NotFound("Bu kullanıcıya ait hesap bulunamadı.");

            return Ok(new { account.IBAN, account.Balance });
        }

        [Authorize]
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Kullanıcı kimliği bulunamadı.");

            int userId = int.Parse(userIdClaim.Value);

            var senderAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            var receiverAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.IBAN == request.ReceiverIban);

            if (senderAccount == null || receiverAccount == null)
                return NotFound("Gönderici veya alıcı hesap bulunamadı.");

            if (senderAccount.Balance < request.Amount)
                return BadRequest("Yetersiz bakiye.");

            senderAccount.Balance -= request.Amount;
            receiverAccount.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                SenderAccountId = senderAccount.Id,
                ReceiverAccountId = receiverAccount.Id,
                Amount = request.Amount,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok("Transfer başarılı.");
        }

        [Authorize]
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Kullanıcı kimliği bulunamadı.");

            int userId = int.Parse(userIdClaim.Value);
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return NotFound("Hesap bulunamadı.");

            if (request.Amount <= 0)
                return BadRequest("Yatırılan miktar pozitif olmalıdır.");

            account.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                SenderAccountId = null, // dış kaynak
                ReceiverAccountId = account.Id,
                Amount = request.Amount,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok("Bakiye başarıyla yüklendi.");
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Benzersiz IBAN oluştur
            string iban = "TR" + Guid.NewGuid().ToString("N")[..24].ToUpper();

            // Hesap numarası belirle
            int existingCount = await _context.Accounts.CountAsync(a => a.UserId == userId);
            string accountNumber = $"{userId:0000}-{existingCount + 1:000}";

            var newAccount = new Account
            {
                UserId = userId,
                IBAN = iban,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                AccountType = request.AccountType,
                Currency = request.Currency,
                AccountNumber = accountNumber
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Ok(new { newAccount.Id, newAccount.IBAN, newAccount.AccountNumber });
        }
        [Authorize]
        [HttpGet("balance")]
        public async Task<IActionResult> GetMyBalance()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
                return NotFound("Bu kullanıcıya ait hesap bulunamadı.");

            return Ok(new { account.IBAN, account.Balance });
        }
        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetUserAccounts()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => new
                {
                    a.Id,
                    a.AccountNumber,
                    a.IBAN,
                    a.Balance,
                    a.AccountType,
                    a.Currency
                })
                .ToListAsync();

            return Ok(accounts);
        }


        [Authorize]
        [HttpPost("admin/deposit")]
        public async Task<IActionResult> AdminDeposit([FromBody] AdminDepositRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var adminUser = await _context.Users.FindAsync(userId);

            if (adminUser == null || !adminUser.IsAdmin)
                return Forbid("Sadece adminler bu işlemi yapabilir.");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.IBAN == request.IBAN);
            if (account == null)
                return NotFound("IBAN'a ait hesap bulunamadı.");

            if (request.Amount <= 0)
            return BadRequest("Yüklenecek miktar pozitif olmalıdır.");

            account.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                SenderAccountId = null,
                ReceiverAccountId = account.Id,
                Amount = request.Amount,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok("Bakiye başarıyla yüklendi.");
        }



    }
}
