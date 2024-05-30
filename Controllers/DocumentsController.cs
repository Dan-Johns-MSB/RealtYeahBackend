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
    public class DocumentsController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public DocumentsController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
        {
            if (context.Documents == null)
            {
                return NotFound();
            }
            return await context.Documents.ToListAsync();
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            if (context.Documents == null)
            {
                return NotFound();
            }
            var document = await context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            return document;
        }

        // PUT: api/Documents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, Document document)
        {
            if (id != document.DocumentId)
            {
                return BadRequest();
            }

            context.Entry(document).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
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

        // POST: api/Documents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Document>> PostDocument(Document document)
        {
            if (context.Documents == null)
            {
                return Problem("Entity set 'RealtyeahContext.Documents'  is null.");
            }
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetDocument", new { id = document.DocumentId }, document);
        }

        // DELETE: api/Documents/5
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            if (context.Documents == null)
            {
                return NotFound();
            }
            var document = await context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            context.Documents.Remove(document);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool DocumentExists(int id)
        {
            return (context.Documents?.Any(e => e.DocumentId == id)).GetValueOrDefault();
        }
    }
}
