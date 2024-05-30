using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtYeahBackend.Entities;

namespace RealtYeahBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class ObjectsStatusController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public ObjectsStatusController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/ObjectsStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ObjectsStatus>>> GetObjectsStatuses()
        {
            if (context.ObjectsStatuses == null)
            {
                return NotFound();
            }
            return await context.ObjectsStatuses.ToListAsync();
        }

        // GET: api/ObjectsStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ObjectsStatus>> GetObjectsStatus(string id)
        {
            if (context.ObjectsStatuses == null)
            {
                return NotFound();
            }
            var objectsStatus = await context.ObjectsStatuses.FindAsync(id);

            if (objectsStatus == null)
            {
                return NotFound();
            }

            return objectsStatus;
        }

        // PUT: api/ObjectsStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutObjectsStatus(string id, ObjectsStatus objectsStatus)
        {
            if (id != objectsStatus.Status)
            {
                return BadRequest();
            }

            context.Entry(objectsStatus).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectsStatusExists(id))
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

        // POST: api/ObjectsStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ObjectsStatus>> PostObjectsStatus(ObjectsStatus objectsStatus)
        {
            if (context.ObjectsStatuses == null)
            {
                return Problem("Entity set 'RealtyeahContext.ObjectsStatuses'  is null.");
            }
            context.ObjectsStatuses.Add(objectsStatus);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ObjectsStatusExists(objectsStatus.Status))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetObjectsStatus", new { id = objectsStatus.Status }, objectsStatus);
        }

        // DELETE: api/ObjectsStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObjectsStatus(string id)
        {
            if (context.ObjectsStatuses == null)
            {
                return NotFound();
            }
            var objectsStatus = await context.ObjectsStatuses.FindAsync(id);
            if (objectsStatus == null)
            {
                return NotFound();
            }

            context.ObjectsStatuses.Remove(objectsStatus);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ObjectsStatusExists(string id)
        {
            return (context.ObjectsStatuses?.Any(e => e.Status == id)).GetValueOrDefault();
        }
    }
}
