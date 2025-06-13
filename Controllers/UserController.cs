using Microsoft.AspNetCore.Mvc;
using BankSysAPI.Data;
using BankSysAPI.Models;

namespace BankSysAPI.Controllers
{
    //Bu sınıf api controller olacakk
    [ApiController] 
    [Route("api/[controller]")]
    public class UserController : ControllerBase //MVC olmasın-> UI yok yani
    {
        //veritabanına ulaşabilmek için
        private readonly BankingDbContext _context;
        public UserController(BankingDbContext context)
        {
            _context = context;
        }

        //register request atacağım burada
        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user); //200 ok döndğrürüm burada
        }

        //userları fetchlemek için:
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }
    }
}
