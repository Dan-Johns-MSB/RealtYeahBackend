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
    public class OperationsStatusController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public OperationsStatusController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/OperationsStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OperationsStatus>>> GetOperationsStatuses()
        {
            if (context.OperationsStatuses == null)
            {
                return NotFound();
            }
            return await context.OperationsStatuses.ToListAsync();
        }

        // GET: api/OperationsStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OperationsStatus>> GetOperationsStatus(string id)
        {
            if (context.OperationsStatuses == null)
            {
                return NotFound();
            }
            var operationsStatus = await context.OperationsStatuses.FindAsync(id);

            if (operationsStatus == null)
            {
                return NotFound();
            }

            return operationsStatus;
        }

        // PUT: api/OperationsStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOperationsStatus(string id, OperationsStatus operationsStatus)
        {
            if (id != operationsStatus.Status)
            {
                return BadRequest();
            }

            context.Entry(operationsStatus).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperationsStatusExists(id))
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

        // POST: api/OperationsStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<OperationsStatus>> PostOperationsStatus(OperationsStatus operationsStatus)
        {
            if (context.OperationsStatuses == null)
            {
                return Problem("Entity set 'RealtyeahContext.OperationsStatuses'  is null.");
            }
            context.OperationsStatuses.Add(operationsStatus);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OperationsStatusExists(operationsStatus.Status))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOperationsStatus", new { id = operationsStatus.Status }, operationsStatus);
        }

        // DELETE: api/OperationsStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOperationsStatus(string id)
        {
            if (context.OperationsStatuses == null)
            {
                return NotFound();
            }
            var operationsStatus = await context.OperationsStatuses.FindAsync(id);
            if (operationsStatus == null)
            {
                return NotFound();
            }

            context.OperationsStatuses.Remove(operationsStatus);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool OperationsStatusExists(string id)
        {
            return (context.OperationsStatuses?.Any(e => e.Status == id)).GetValueOrDefault();
        }
    }
}
