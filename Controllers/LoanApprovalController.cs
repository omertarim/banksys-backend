using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankSysAPI.Data;
using BankSysAPI.Models;

namespace BankSysAPI.Controllers
{
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
                .Where(l => l.Status == "Pending")
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

            loan.Status = request.NewStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Başvuru {request.NewStatus.ToLower()} olarak güncellendi." });
        }

    }
}
