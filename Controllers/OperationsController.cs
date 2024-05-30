using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using RealtYeahBackend.Entities;
using RealtYeahBackend.Models;
using RealtYeahBackend.Services;

namespace RealtYeahBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class OperationsController : ControllerBase
    {
        private readonly RealtyeahContext context;
        private readonly OperationService operationService;

        public OperationsController(RealtyeahContext context, OperationService operationService)
        {
            this.context = context;
            this.operationService = operationService;
        }

        // GET: api/Operations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Operation>>> GetOperations()
        {
            if (context.Operations == null)
            {
                return NotFound();
            }
            return await context.Operations.ToListAsync();
        }

        // GET: api/Operations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Operation>> GetOperation(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }
            var operation = await context.Operations.FindAsync(id);

            if (operation == null)
            {
                return NotFound();
            }

            return operation;
        }

        // PUT: api/Operations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOperation(int id, Operation operation)
        {
            if (id != operation.OperationId)
            {
                return BadRequest();
            }

            context.Entry(operation).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperationExists(id))
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

        // POST: api/Operations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Operation>> PostOperation(Operation operation)
        {
            if (context.Operations == null)
            {
                return Problem("Entity set 'RealtyeahContext.Operations'  is null.");
            }

            operation.Status = "Очікується";
            context.Operations.Add(operation);
            await context.SaveChangesAsync();

            if (operation.CounteragentSecondary == null)
            {
                context.ClientsStatusesAssignments.Add(new ClientsStatusesAssignment
                {
                    ClientId = operation.CounteragentLead,
                    Status = "Внесений у базу",
                    OperationId = operation.OperationId
                });
            }
           
            return CreatedAtAction("GetOperation", new { id = operation.OperationId }, operation);
        }

        // DELETE: api/Operations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOperation(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }
            var operation = await context.Operations.FindAsync(id);
            if (operation == null)
            {
                return NotFound();
            } 
            else if (!string.IsNullOrWhiteSpace(operation.ActType) && !operation.Status.Equals("Неуспішно"))
            {
                return BadRequest("Can't delete a successful operation with already assigned act");
            }

            context.Operations.Remove(operation);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/assign/{actType}")]
        public IActionResult AssignActToOperation(int id, string actType)
        {
            try
            {
                Operation operationWithAct = operationService.AddAct(id, actType);
                return Ok(operationWithAct);
            } 
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }           
        }

        [HttpPut("{id}/cancel")]
        public IActionResult CancelOperation(int id)
        {
            try
            {
                Operation cancelledOperation = operationService.CancelOperation(id);
                return Ok(cancelledOperation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NullReferenceException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("bd/{start}/to/{end}")]
        public async Task<ActionResult<IList<Operation>>> GetOperationsByDate(string start, string end)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            IList<Operation> operations = await context.Operations.Where(operation => operation.Date.Date >= DateTime.ParseExact(start, "ddMMyyyy", CultureInfo.InvariantCulture).Date
                                                                             && operation.Date.Date <= DateTime.ParseExact(end, "ddMMyyyy", CultureInfo.InvariantCulture).Date).ToListAsync();

            return Ok(operations);
        }

        [AllowAnonymous]
        [HttpGet("bd/{start}/to/{end}/host/{id}")]
        public async Task<ActionResult<IList<Operation>>> GetOperationsByDateAndHost(string start, string end, int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            IList<Operation> operations = await context.Operations.Where(operation => operation.Date.Date >= DateTime.ParseExact(start, "ddMMyyyy", CultureInfo.InvariantCulture).Date
                                                                                   && operation.Date.Date <= DateTime.ParseExact(end, "ddMMyyyy", CultureInfo.InvariantCulture).Date
                                                                                   && operation.HostId == id).ToListAsync();

            return Ok(operations);
        }

        [HttpGet("host/{id}")]
        public async Task<ActionResult<IList<Operation>>> GetOperationsByHost(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            IList<Operation> operations = await context.Operations.Where(operation => operation.HostId == id).ToListAsync();

            return Ok(operations);
        }

        [HttpGet("{id}/host")]
        public async Task<ActionResult<Employee>> GetHostByOperation(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Employee host = await context.Operations.Where(operation => operation.OperationId == id)
                .Select(operation => operation.Host).FirstOrDefaultAsync();

            if (host == null)
            {
                return NotFound();
            }

            return Ok(host);
        }

        [HttpGet("{id}/Object")]
        public async Task<ActionResult<EstateObject>> GetRelatedObject(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            EstateObject estateObject = await context.Operations.Where(op => op.OperationId == id)
                .Select(op => op.EstateObjectNavigation).FirstOrDefaultAsync();

            if (estateObject == null)
            {
                return NotFound();
            }

            return Ok(estateObject);
        }

        [HttpGet("{id}/ObjectAddress")]
        public async Task<ActionResult<string>> GetRelatedObjectAddress(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Operation operation = await context.Operations.Where(operation => operation.OperationId == id)
                .Include(operation => operation.EstateObjectNavigation).FirstOrDefaultAsync();

            if (operation == null)
            {
                return NotFound();
            }

            EstateObject estateObject = operation.EstateObjectNavigation;

            if (estateObject == null)
            {
                return NotFound();
            }

            return Ok("\"" + estateObject.Address + "\"");
        }
       
        [HttpGet("{id}/availableActs")]
        public ActionResult<List<bool>> GetAvailableActs(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Operation operation = context.Operations.Where(operation => operation.OperationId == id)
                .Include(operation => operation.FkOperationLeadNavigation)
                .Include(operation => operation.FkOperationSecondaryNavigation)
                .Include(operation => operation.InverseFkOperationLeadNavigation)
                .Include(operation => operation.InverseFkOperationSecondaryNavigation)
                .FirstOrDefault();

            if (operation == null)
            {
                return NotFound();
            }

            List<bool> availableActs = operationService.GetAvailableActs(operation);

            return Ok(availableActs);
        }

        [HttpGet("{id}/availableForNext")]
        public ActionResult<bool> GetNextAvailability(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            Operation operation = context.Operations.Find(id);

            if (operation == null)
            {
                return NotFound();
            }

            return Ok(operationService.GetNextAvailability(operation));
        }

        [HttpGet("actTypeActive/{actType}")]
        public async Task<ActionResult<IList<Operation>>> GetOperationsByActiveAct(string actType)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            List<Operation> operations = await context.Operations.Where(operation => actType.Equals(operation.ActType)
                                                                                   && operation.Status.Equals("Успішно")).ToListAsync();
            List<Operation> availableOperations = new List<Operation>();
            
            foreach (Operation operation in operations)
            {
                if (operationService.GetNextAvailability(operation))
                {
                    availableOperations.Add(operation);
                }
            }

            return Ok(availableOperations);
        }

        private bool OperationExists(int id)
        {
            return (context.Operations?.Any(e => e.OperationId == id)).GetValueOrDefault();
        }
    }
}
