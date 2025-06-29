using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ItineraryController : ControllerBase
    {
        private readonly DataContext _context;

        public ItineraryController(DataContext context)
        {
            _context = context;
        }

        // GET: api/itinerary
        [HttpGet]
        public ActionResult<IEnumerable<Itinerary>> GetItineraries()
        {
            var itineraries = _context.Itineraries.ToList();
            return Ok(itineraries);
        }

        // GET: api/itinerary/{id}
        [HttpGet("{id}")]
        public ActionResult<Itinerary> GetItinerary(int id)
        {
            var itinerary = _context.Itineraries.Find(id);
            if (itinerary == null)
                return NotFound();

            return Ok(itinerary);
        }

        // GET: api/itinerary?tourId={tourId}
        [HttpGet("byTourId")]
        public ActionResult<IEnumerable<Itinerary>> GetItinerariesByTourId(string tourId)
        {
            var itineraries = _context.Itineraries.Where(i => i.TourId == tourId).ToList();

            if (itineraries.Count == 0)
            {
                return NotFound(new { message = $"Không có lịch trình nào cho Tour ID: {tourId}." });
            }

            return Ok(itineraries);
        }

        // POST: api/itinerary
        [HttpPost]
        public ActionResult<Itinerary> CreateItinerary(Itinerary itinerary)
        {
            _context.Itineraries.Add(itinerary);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetItinerary), new { id = itinerary.Id }, itinerary);
        }

        // PUT: api/itinerary/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateItinerary(int id, Itinerary itinerary)
        {
            var existingItinerary = _context.Itineraries.Find(id);
            if (existingItinerary == null)
                return NotFound();

            // Cập nhật thông tin lịch trình
            existingItinerary.DayNumber = itinerary.DayNumber;
            existingItinerary.Description = itinerary.Description;
            existingItinerary.TourId = itinerary.TourId;

            _context.Itineraries.Update(existingItinerary);
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/itinerary/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteItinerary(int id)
        {
            var itinerary = _context.Itineraries.Find(id);
            if (itinerary == null)
                return NotFound();

            _context.Itineraries.Remove(itinerary);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
