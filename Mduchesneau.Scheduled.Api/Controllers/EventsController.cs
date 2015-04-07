﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Mduchesneau.Scheduled.Api.Helpers;
using Mduchesneau.Scheduled.Api.Models;
using Mduchesneau.Scheduled.DataModel;
//using System.Web.Http.HttpPost = HttpPost;

namespace Mduchesneau.Scheduled.Api.Controllers
{
    /// <summary>Retrieve and manage schedule events.</summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EventsController : ApiController
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the event for the specified id.</summary>
        /// <param name="id">The id of the event.</param>
        [HttpGet, Route("events/{id}")]
        public ScheduleEvent GetEvent(int id)
        {
            using (Database database = new Database())
            {
                return database.ScheduleEvents.First(p => p.ID == id);
            }
        }

        /// <summary>Get all events within bounds for the calendar specified.</summary>
        /// <param name="calendarId">Id of the calendar.</param>
        /// <param name="start">The earliest date boundary to query events for.</param>
        /// <param name="end">The latest date boundary to query events for.</param>
        [HttpGet, Route("events/calendar/{calendarId}")]
        public List<ScheduleEventWrapper> GetEventsForCalendar(int calendarId, DateTime? start = null, DateTime? end = null)
        {
            using (Database database = new Database())
            {
                var events = from scheduleEvent in database.ScheduleEvents
                             where scheduleEvent.CalendarID == calendarId
                                && (start == null || scheduleEvent.Start >= start)
                                && (end == null || scheduleEvent.End <= end)
                             
                             select new ScheduleEventWrapper() { 
                                 Id = scheduleEvent.ID, 
                                 CalendarId = scheduleEvent.CalendarID,
                                 Title = scheduleEvent.Title,
                                 Start = scheduleEvent.Start,
                                 End = scheduleEvent.End
                             };
                return events.ToList();
            }
        }

        /// <summary>Get all existing calendars.</summary>
        [HttpGet, Route("events/calendars")]
        public List<CalendarWrapper> GetCalendars()
        {
            using (Database database = new Database())
            {
                var calendars = from calendar in database.Calendars
                                select new CalendarWrapper() { Id = calendar.ID, Name = calendar.Name };
                return calendars.ToList();
            }
        }

        /// <summary>Import schedule events from the contents of the CSV file specified.</summary>
        /// <returns>If successful, the number of successfully imported schedule events.</returns>
        [HttpPost, Route("events/import")]
        public object ImportFromCsvContent()
        {
            using (Database database = new Database())
            {
                var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (file == null)
                    throw new InvalidOperationException("Import file data is null!");

                // Import events
                IEnumerable<ScheduleEvent> importedEvents = ImportHelper.ImportScheduleEvents(database, ImportHelper.ParseEventsFromCsv(file.InputStream));

                database.SaveChanges();
                return new { Message = String.Format("{0} events imported.", importedEvents.Count()) };
            }
        }
    }
}
