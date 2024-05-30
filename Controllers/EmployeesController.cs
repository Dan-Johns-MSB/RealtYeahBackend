using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class EmployeesController : ControllerBase
    {
        private readonly RealtyeahContext context;

        public EmployeesController(RealtyeahContext context)
        {
            this.context = context;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            if (context.Employees == null)
            {
                return NotFound();
            }
            return await context.Employees.ToListAsync();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            if (context.Employees == null)
            {
                return NotFound();
            }
            var employee = await context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754      
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return BadRequest();
            }

            Debug.WriteLine(employee);

            context.Entry(employee).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            if (context.Employees == null)
            {
                return Problem("Entity set 'RealtyeahContext.Employees'  is null.");
            }
            employee.Status = "Активний";
            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetEmployee", new { id = employee.EmployeeId }, employee);
        }

        // DELETE: api/Employees/5
        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> FireEmployee(int id)
        {
            if (context.Employees == null)
            {
                return NotFound();
            }
            var employee = await context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = Role.Admin + "," + Role.MainAdmin)]
        [HttpPut("{id}/fireSaved")]
        public async Task<ActionResult<Employee>> FireEmployeeSaved(int id)
        {
            if (context.Employees == null)
            {
                return NotFound();
            }

            Employee employee = await context.Employees.FindAsync(id);
            
            if (employee == null)
            {
                return NotFound();
            }

            employee.Status = "Звільнений";
            employee.Firedate = DateTime.Now;

            User account = await context.Users.Where(user => user.EmployeeId == employee.EmployeeId).FirstOrDefaultAsync();
            context.Users.Remove(account);

            await context.SaveChangesAsync();

            return Ok(employee);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IList<Employee>>> GetEmployeesByStatus(string status)
        {
            if (context.Employees == null)
            {
                return NotFound();
            }

            IList<Employee> employees = await context.Employees.Where(employee => employee.Status.Equals(status)).ToListAsync();

            return Ok(employees);
        }

        [HttpGet("{id}/email")]
        public async Task<ActionResult<string>> GetEmployeeEmail(int id)
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

            return Ok("\"" + user.Login + "\"");
        }

        [HttpGet("{id}/relatedOperations")]
        public async Task<ActionResult<IList<Operation>>> GetEmployeeRelatedOperations(int id)
        {
            if (context.Operations == null)
            {
                return NotFound();
            }

            IList<Operation> operations = await context.Operations.Where(operation => operation.HostId == id)
                .OrderByDescending(operation => operation.Date).ToListAsync();

            return Ok(operations);
        }

        [HttpGet("{id}/relatedClients")]
        public async Task<ActionResult<IList<Client>>> GetEmployeeRelatedClients(int id)
        {
            if (context.Clients == null)
            {
                return NotFound();
            }

            IList<Client> clients = await context.Clients.Where(client => client.OperationCounteragentLeadNavigations.Any(operation => operation.HostId == id) 
                                                                       || client.OperationCounteragentSecondaryNavigations.Any(operation => operation.HostId == id)).ToListAsync();

            return Ok(clients);
        }
        
        [HttpGet("{id}/relatedObjects")]
        public async Task<ActionResult<IList<EstateObject>>> GetEmployeeRelatedObjects(int id)
        {
            if (context.EstateObjects == null)
            {
                return NotFound();
            }

            IList<EstateObject> estateObjects = await context.EstateObjects.Where(estateObject => estateObject.Operations.Any(operation => operation.HostId == id)).ToListAsync();

            return Ok(estateObjects);
        }

        private bool EmployeeExists(int id)
        {
            return (context.Employees?.Any(e => e.EmployeeId == id)).GetValueOrDefault();
        }
    }
}
