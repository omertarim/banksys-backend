using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSysAPI.Data;
using BankSysAPI.Models;


[Route("api/[controller]")]
[ApiController]
public class LoanApplicationTypeController : ControllerBase
{
    private readonly BankingDbContext _context;

    public LoanApplicationTypeController(BankingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanApplicationType>>> GetAll()
    {
        return await _context.LoanApplicationTypes
                             .Where(t => t.IsActive)
                             .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult> Create(LoanApplicationType model)
    {
        _context.LoanApplicationTypes.Add(model);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, LoanApplicationType model)
    {
        var type = await _context.LoanApplicationTypes.FindAsync(id);
        if (type == null) return NotFound();

        type.Name = model.Name;
        type.Description = model.Description;
        type.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var type = await _context.LoanApplicationTypes.FindAsync(id);
        if (type == null) return NotFound();

        type.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("active")]
    public IActionResult GetActiveLoanTypes()
    {
        var activeLoanTypes = _context.LoanApplicationTypes
            .Where(x => x.IsActive == true)
            .Select(x => new {
                id = x.Id,
                name = x.Name
            })
            .ToList();

        return Ok(activeLoanTypes);
    }
}
