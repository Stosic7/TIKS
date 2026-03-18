using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingsController : ControllerBase
    {
        private readonly GymContext _context;

        public TrainingsController(GymContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Training>>> GetAll()
        {
            var trainings = await _context.Trainings
                .Include(t => t.Trainer)
                .ToListAsync();
            return Ok(trainings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Training>> GetById(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.Trainer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
                return NotFound();

            return Ok(training);
        }

        [HttpPost]
        public async Task<ActionResult<Training>> Create([FromBody] Training training)
        {
            training.Trainer = null!;
            training.TrainingPlans = new List<TrainingPlan>();
            _context.Trainings.Add(training);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = training.Id }, training);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Training training)
        {
            if (id != training.Id)
                return BadRequest();

            training.Trainer = null!;
            training.TrainingPlans = new List<TrainingPlan>();
            _context.Entry(training).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Trainings.AnyAsync(t => t.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training == null)
                return NotFound();

            _context.Trainings.Remove(training);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
