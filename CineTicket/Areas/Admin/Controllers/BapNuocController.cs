using CineTicket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class SnacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SnacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Snacks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Snacks.ToListAsync());
        }

        // GET: Admin/Snacks/Add
        [HttpGet]
        public IActionResult Add() => View();

        // POST: Admin/Snacks/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Snack snack)
        {
            if (ModelState.IsValid)
            {
                _context.Snacks.Add(snack);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(snack);
        }

        // GET: Admin/Snacks/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var snack = await _context.Snacks.FindAsync(id);
            if (snack == null)
                return NotFound();

            return View(snack);
        }

        // POST: Admin/Snacks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Snack snack)
        {
            if (id != snack.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Snacks.Update(snack);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(snack);
        }

        // GET: Admin/Snacks/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var snack = await _context.Snacks.FindAsync(id);
            if (snack == null)
                return NotFound();

            return View(snack);
        }

        // POST: Admin/Snacks/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var snack = await _context.Snacks.FindAsync(id);
            if (snack != null)
            {
                _context.Snacks.Remove(snack);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
