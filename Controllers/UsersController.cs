using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using AutoMapper;
using RealtYeahBackend.Entities;
using RealtYeahBackend.Helpers;
using RealtYeahBackend.Models;
using RealtYeahBackend.Services;
using System.Diagnostics;

namespace RealtYeahBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [RequireHttps]
    public class UsersController : ControllerBase
    {
        private readonly RealtyeahContext context;
        private readonly UserService userService;
        private readonly IMapper autoMapper;

        public UsersController(RealtyeahContext context, UserService userService, IMapper mapper)
        {
            this.context = context;
            this.userService = userService;
            this.autoMapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] EncModel encModel)
        {
            AuthenticationModel model;

            try
            {                             
                model = JsonConvert.DeserializeObject<AuthenticationModel>(userService.Decrypt(encModel.encModel));
            }
            catch (Exception ex) {
                return BadRequest(ex.Message); 
            }

            var user = userService.Authenticate(model.Login, model.Password);

            if (user == null)
            {
                return NotFound(new { 
                                    message = "Username or password is wrong" 
                                });
            }
                
            if (user.Login == "Incorrect password")
            {
                return Unauthorized();
            }
                
            return Ok(new
            {
                user.Login,
                user.Role,
                user.EmployeeId,
                Token = userService.GenerateToken(user)
            });
        }

        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPost("register")]
        public IActionResult Register([FromBody] EncModel encModel)
        {
            RegisterModel model;

            try
            {
                model = JsonConvert.DeserializeObject<RegisterModel>(userService.Decrypt(encModel.encModel));
                Debug.WriteLine(model.ToString());
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }

            var user = autoMapper.Map<User>(model);

            user.Role = Role.Agent;

            try
            {
                userService.Create(user, model.Password);
                return Ok(new
                {
                    user.Login,
                    user.Role,
                    user.EmployeeId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Users
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (context.Users == null)
            {
                return NotFound();
            }

            return await context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (context.Users == null)
            {
                return NotFound();
            }
            var user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] EncModel encModel)
        {
            UserUpdateModel user;

            try
            {
                user = JsonConvert.DeserializeObject<UserUpdateModel>(userService.Decrypt(encModel.encModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }

            if (!UserExists(id))
            {
                return NotFound();
            }

            try
            {
                userService.Update(user);
            } 
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }          

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (context.Users == null)
            {
                return Problem("Entity set 'Users' is null.");
            }
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (context.Users == null)
            {
                return NotFound();
            }
            var user = await context.Users.FindAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("role/{id}")]
        public async Task<ActionResult<string>> GetEmployeeUserRole(int id)
        {
            if (context.Users == null)
            {
                return NotFound();
            }

            User user = await context.Users.Where(user => user.EmployeeId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok("\"" + user.Role + "\"");
        }

        [HttpGet("byEmployee/{id}")]
        public async Task<ActionResult<User>> GetEmployeeUser(int id)
        {
            if (context.Users == null)
            {
                return NotFound();
            }

            User user = await context.Users.Where(user => user.EmployeeId == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        private bool UserExists(int id)
        {
            return (context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
