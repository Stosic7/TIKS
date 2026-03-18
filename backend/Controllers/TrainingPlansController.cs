using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingPlansController : ControllerBase
    {
        private readonly GymContext _context;

        public TrainingPlansController(GymContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<TrainingPlan>>> GetAll()
        {
            var plans = await _context.TrainingPlans
                .Include(tp => tp.Member)
                .Include(tp => tp.Training)
                .ToListAsync();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingPlan>> GetById(int id)
        {
            var plan = await _context.TrainingPlans
                .Include(tp => tp.Member)
                .Include(tp => tp.Training)
                .FirstOrDefaultAsync(tp => tp.Id == id);

            if (plan == null)
                return NotFound();

            return Ok(plan);
        }

        [HttpPost]
        public async Task<ActionResult<TrainingPlan>> Create([FromBody] TrainingPlan plan)
        {
            plan.Member = null!;
            plan.Training = null!;
            _context.TrainingPlans.Add(plan);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TrainingPlan plan)
        {
            if (id != plan.Id)
                return BadRequest();

            plan.Member = null!;
            plan.Training = null!;
            _context.Entry(plan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TrainingPlans.AnyAsync(tp => tp.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.TrainingPlans.FindAsync(id);
            if (plan == null)
                return NotFound();

            _context.TrainingPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
