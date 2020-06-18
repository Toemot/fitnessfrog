using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Treehouse.FitnessFrog.Data;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Controllers
{
    public class EntriesController : Controller
    {
        private EntriesRepository _entriesRepository = null;

        public EntriesController()
        {
            _entriesRepository = new EntriesRepository();
        }

        public ActionResult Index()
        {
            List<Entry> entries = _entriesRepository.GetEntries();

            // Calculate the total activity.
            double totalActivity = entries
                .Where(e => e.Exclude == false)
                .Sum(e => e.Duration);

            // Determine the number of days that have entries.
            int numberOfActiveDays = entries
                .Select(e => e.Date)
                .Distinct()
                .Count();

            ViewBag.TotalActivity = totalActivity;
            ViewBag.AverageDailyActivity = (totalActivity / (double)numberOfActiveDays);

            return View(entries);
        }

        public ActionResult Add()
        {
            var entry = new Entry()
            {
                Date = DateTime.Today,
            };

            ViewBag.ActivitiesSelectListItems = new SelectList(Data.Data.Activities, "Id", "Name");
            return View(entry);
        }

        [HttpPost]
        public ActionResult Add(Entry entry)
        {
            ValidateEntry(entry);
            if (ModelState.IsValid)
            {
                _entriesRepository.AddEntry(entry);

                return RedirectToAction("Index");
            }
            SetupActivitiesSelectListItems();
            return View(entry);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            //Get request from the entries repository
            Entry entry = _entriesRepository.GetEntry(id.Value);

            //Return a status of Not Found is the entry isn't found.
            if (entry == null)
            {
                return HttpNotFound();
            }

            // Pass entry into the view method
            SetupActivitiesSelectListItems();

            return View(entry);
        }

        [HttpPost]
        public ActionResult Edit(Entry entry) 
        {
            //Validate the entry
            ValidateEntry(entry);

            //If Entry is valid, 1) Use the repository to update the entry. 2) Redirect the user to the "Entries" list page
            if (ModelState.IsValid)
            {
                _entriesRepository.UpdateEntry(entry);
                return RedirectToAction("Index");
            }

            //Populate the activities select list items ViewBag property.
            SetupActivitiesSelectListItems();

            return View(entry);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Retrieve entry for the provided parameter value.
            Entry entry = _entriesRepository.GetEntry(id.Value);

            //Return not found if entry was not found
            if (entry == null)
            {
                return HttpNotFound();
            }

            //Pass entry to the view

            return View(entry);
        }

        [HttpPost]
        public ActionResult Delete(int id) 
        {
            //Delete the entry
            _entriesRepository.DeleteEntry(id);

            //Redirect entry to the "Entries" list page
            return RedirectToAction("Index");
        }

        private void ValidateEntry(Entry entry)
        {
            //If there aren't any Duration field validation errors, then make sure that the duration < 0.
            if (ModelState.IsValidField("Duration") && entry.Duration <= 0)
            {
                ModelState.AddModelError("Duration", "The Duration field value must be greater than '0'");
            }
        }

        private void SetupActivitiesSelectListItems()
        {
            ViewBag.ActivitiesSelectListItems = new SelectList(Data.Data.Activities, "Id", "Name");
        }
    }
}