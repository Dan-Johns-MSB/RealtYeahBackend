using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtYeahBackend.Entities;
using RealtYeahBackend.Models;

namespace RealtYeahBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class EmployeesStatusController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public EmployeesStatusController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/EmployeesStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeesStatus>>> GetEmployeesStatuses()
        {
            if (context.EmployeesStatuses == null)
            {
                return NotFound();
            }
            return await context.EmployeesStatuses.ToListAsync();
        }

        // GET: api/EmployeesStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeesStatus>> GetEmployeesStatus(string id)
        {
            if (context.EmployeesStatuses == null)
            {
                return NotFound();
            }
            var employeesStatus = await context.EmployeesStatuses.FindAsync(id);

            if (employeesStatus == null)
            {
                return NotFound();
            }

            return employeesStatus;
        }

        // PUT: api/EmployeesStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeesStatus(string id, EmployeesStatus employeesStatus)
        {
            if (id != employeesStatus.Status)
            {
                return BadRequest();
            }

            context.Entry(employeesStatus).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeesStatusExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/EmployeesStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPost]
        public async Task<ActionResult<EmployeesStatus>> PostEmployeesStatus(EmployeesStatus employeesStatus)
        {
            if (context.EmployeesStatuses == null)
            {
                return Problem("Entity set 'RealtyeahContext.EmployeesStatuses'  is null.");
            }
            context.EmployeesStatuses.Add(employeesStatus);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EmployeesStatusExists(employeesStatus.Status))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEmployeesStatus", new { id = employeesStatus.Status }, employeesStatus);
        }

        // DELETE: api/EmployeesStatus/5
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeesStatus(string id)
        {
            if (context.EmployeesStatuses == null)
            {
                return NotFound();
            }
            var employeesStatus = await context.EmployeesStatuses.FindAsync(id);
            if (employeesStatus == null)
            {
                return NotFound();
            }

            context.EmployeesStatuses.Remove(employeesStatus);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeesStatusExists(string id)
        {
            return (context.EmployeesStatuses?.Any(e => e.Status == id)).GetValueOrDefault();
        }
    }
}
