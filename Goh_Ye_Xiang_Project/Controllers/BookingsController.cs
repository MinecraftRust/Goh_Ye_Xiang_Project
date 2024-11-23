using CozyPlaceSG.Data;
using CozyPlaceSG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        private async Task<T> ParseRequestBody<T>() where T : class, new()
        {
            string requestBody = string.Empty;
            using (var reader = new StreamReader(Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (Request.ContentType == "application/json")
            {
                return JsonConvert.DeserializeObject<T>(requestBody);
            }
            else if (Request.ContentType == "application/x-www-form-urlencoded")
            {
                var formData = await Request.ReadFormAsync();
                var model = new T();
                foreach (var property in typeof(T).GetProperties())
                {
                    if (formData.ContainsKey(property.Name))
                    {
                        property.SetValue(model, Convert.ChangeType(formData[property.Name], property.PropertyType));
                    }
                }
                return model;
            }

            return null;
        }

        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Bookings);
        }

        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
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

        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpGet("status/{status}")]
        public IActionResult GetByStatus(string status)
        {
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

        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var booking = await ParseRequestBody<Booking>();
            if (booking == null || string.IsNullOrEmpty(booking.FacilityName) || string.IsNullOrEmpty(booking.BookedBy))
            {
                return BadRequest("FacilityName and BookedBy are required.");
            }

            booking.BookingStatus = Enum.TryParse(booking.BookingStatus.ToString(), true, out BookingStatus parsedStatus)
                ? parsedStatus
                : BookingStatus.Pending;

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return CreatedAtAction("GetAll", new { id = booking.BookingId }, booking);
        }

        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpPut]
        public async Task<IActionResult> Put()
        {
            var booking = await ParseRequestBody<Booking>();
            if (booking == null || booking.BookingId <= 0)
            {
                return BadRequest("Invalid booking data.");
            }

            var entity = _context.Bookings.FirstOrDefault(e => e.BookingId == booking.BookingId);
            if (entity == null)
                return Problem(detail: "Booking with id " + booking.BookingId + " is not found.", statusCode: 404);

            entity.FacilityName = booking.FacilityName;
            entity.FacilityDescription = booking.FacilityDescription;
            entity.BookingDateFrom = booking.BookingDateFrom;
            entity.BookingDateTo = booking.BookingDateTo;
            entity.BookingTimeFrom = booking.BookingTimeFrom;
            entity.BookingTimeTo = booking.BookingTimeTo;
            entity.BookedBy = booking.BookedBy;
            entity.BookingStatus = Enum.TryParse(booking.BookingStatus.ToString(), true, out BookingStatus parsedStatus) ? parsedStatus : BookingStatus.Pending;

            _context.SaveChanges();

            return Ok(entity);
        }

        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.member)]
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var deleteRequest = await ParseRequestBody<Booking>();
            if (deleteRequest == null || deleteRequest.BookingId <= 0)
            {
                return BadRequest("Invalid booking ID.");
            }

            var entity = _context.Bookings.FirstOrDefault(e => e.BookingId == deleteRequest.BookingId);
            if (entity == null)
                return Problem(detail: "Booking with id " + deleteRequest.BookingId + " is not found.", statusCode: 404);

            _context.Bookings.Remove(entity);
            _context.SaveChanges();

            return Ok(entity);
        }
    }
}
