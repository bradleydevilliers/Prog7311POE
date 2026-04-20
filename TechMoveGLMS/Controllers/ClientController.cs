using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models.Entities;

namespace TechMoveGLMS.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET: Client
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.ToListAsync();
            return View(clients);
        }
        
        //  Client/Create
        public IActionResult Create()
        {
            return View();
        }
        
        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }
        
        // Client/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            
            return View(client);
        }
        
        // POST: Client/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }
        
        // GET: Client/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            
            return View(client);
        }
        
        // POST: Client/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}