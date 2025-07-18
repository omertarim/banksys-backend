using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankSysAPI.Data;
using BankSysAPI.Models;

namespace BankSysAPI.Controllers
{


     public class LoanStatusUpdateRequest
    {
        public string NewStatus { get; set; }
    }
    [ApiController]
    [Route("api/admin/loans")]
    [Authorize(Roles = "Admin")]
    public class LoanApprovalController : ControllerBase
    {
        private readonly BankingDbContext _context;

        public LoanApprovalController(BankingDbContext context)
        {
            _context = context;
        }

        // GET: Tüm bekleyen kredi başvurularını getir
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApplications()
        {
            var pendingLoans = await _context.LoanApplications
                .Include(l => l.User)
                .Where(l => l.LoanStatusId == -1) // Use LoanStatusId instead of Status
                .ToListAsync();

            return Ok(pendingLoans);
        }




        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateLoanStatus(int id, [FromBody] LoanStatusUpdateRequest request)
        {
            var loan = await _context.LoanApplications.FindAsync(id);
            if (loan == null)
                return NotFound("Başvuru bulunamadı.");

            if (request.NewStatus != "Approved" && request.NewStatus != "Rejected")
                return BadRequest("Geçersiz durum.");

      

            // Update LoanStatusId based on the new status
            if (request.NewStatus == "Approved")
            {
                loan.LoanStatusId = -2; // -2 is "Approved"
                var targetAccount = await _context.Accounts.FindAsync(loan.TargetAccountId);
                if (targetAccount == null)
                    return NotFound("Hedef hesap bulunamadı.");

                targetAccount.Balance += loan.Amount;
            }
            else if (request.NewStatus == "Rejected")
            {
                loan.LoanStatusId = -3; // -3 is "Rejected"
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Başvuru {request.NewStatus.ToLower()} olarak güncellendi." });
        }

    }
}
