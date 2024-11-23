using CozyPlaceSG.Models;
using Goh_Ye_Xiang_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyPlaceSG.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }



        // [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        // GET: api/Bookings
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Bookings);
        }


        // [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        // GET: api/Bookings/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int? id)
        {
            if (id == null)
            {
                return BadRequest("Booking ID cannot be null.");
            }

            var booking = _context.Bookings.FirstOrDefault(e => e.BookingId == id);
            if (booking == null)
            {
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);
            }

            return Ok(booking);
        }



        // [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        // GET: api/Bookings/status/{status}
        [HttpGet("status/{status}")]
        public IActionResult GetByStatus(string status)
        {
            // Enum parsing to ensure status matches the BookingStatus enum values
            if (!Enum.TryParse(status, true, out BookingStatus parsedStatus))
            {
                return BadRequest("Invalid status value. Use 'Confirmed', 'Pending', or 'Cancelled'.");
            }

            var bookings = _context.Bookings.Where(e => e.BookingStatus == parsedStatus);

            if (!bookings.Any())
            {
                return NotFound($"No bookings found with status '{status}'.");
            }

            return Ok(bookings);
        }



        // [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpPost]
        public IActionResult Post(Booking booking)
        {

            _context.Bookings.Add(booking);
            _context.SaveChanges();


            return CreatedAtAction("GetAll", new { id = booking.BookingId }, booking);
        }



        // [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpPut]
        public IActionResult Put(int? BookingId, Booking booking)
        {
            var entity = _context.Bookings.FirstOrDefault(e => e.BookingId == BookingId);
            if (entity == null)
                return Problem(detail: "Booking with id " + BookingId + " is not found.", statusCode: 404);

            entity.FacilityName = booking.FacilityName;
            entity.FacilityDescription = booking.FacilityDescription;
            entity.BookingDateFrom = booking.BookingDateFrom;
            entity.BookingDateTo = booking.BookingDateTo;
            entity.BookingTimeFrom = booking.BookingTimeFrom;
            entity.BookingTimeTo = booking.BookingTimeTo;
            entity.BookedBy = booking.BookedBy;
            entity.BookingStatus = booking.BookingStatus;

            _context.SaveChanges();

            return Ok(entity);
        }



        // [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpDelete]
        public IActionResult Delete(int? BookingId)
        {
            var entity = _context.Bookings.FirstOrDefault(e => e.BookingId == BookingId);
            if (entity == null)
                return Problem(detail: "Booking with id " + BookingId + " is not found.", statusCode: 404);

            _context.Bookings.Remove(entity);
            _context.SaveChanges();

            return Ok(entity);
        }




    } // This is end of the class REMEMBER!!!
}