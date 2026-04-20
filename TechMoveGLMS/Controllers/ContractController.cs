using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models.ViewModels;
using TechMoveGLMS.Services.Contracts;
using TechMoveGLMS.Services.Notifications;

namespace TechMoveGLMS.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractFactory _contractFactory;
        private readonly NotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;
        
        public ContractController(
            ApplicationDbContext context,
            IContractFactory contractFactory,
            NotificationService notificationService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _contractFactory = contractFactory;
            _notificationService = notificationService;
            _environment = environment;
        }
        
        // GET: Contract
        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .ToListAsync();
            return View(contracts);
        }
        
        // GET: Contract/Create
        public IActionResult Create()
        {
            var viewModel = new ContractViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddYears(1),
                Clients = _context.Clients
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList(),
                ServiceLevels = new List<SelectListItem>
                {
                    new() { Value = "Basic", Text = "Basic" },
                    new() { Value = "Premium", Text = "Premium" },
                    new() { Value = "Enterprise", Text = "Enterprise" }
                }
            };
            return View(viewModel);
        }
        
        // POST: Contract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractViewModel viewModel)
        {
            // File validation - only PDF allowed
            if (viewModel.SignedAgreement != null)
            {
                var extension = Path.GetExtension(viewModel.SignedAgreement.FileName).ToLower();
                if (extension != ".pdf")
                {
                    ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed for signed agreements.");
                }
            }
            
            if (ModelState.IsValid)
            {
                // Use Factory Method pattern to create contract
                var contract = _contractFactory.CreateContract(
                    viewModel.ServiceLevel,
                    viewModel.ClientId,
                    viewModel.StartDate,
                    viewModel.EndDate);
                
                // Handle file upload
                if (viewModel.SignedAgreement != null && viewModel.SignedAgreement.Length > 0)
                {
                    contract.SignedAgreementPath = await SaveUploadedFile(viewModel.SignedAgreement);
                }
                
                _context.Add(contract);
                await _context.SaveChangesAsync();
                
                // Notify observers about new contract
                await _notificationService.NotifyAllAsync(
                    "New Contract Created",
                    $"Contract #{contract.Id} has been created for client with service level {contract.ServiceLevel}");
                
                return RedirectToAction(nameof(Index));
            }
            
            // Repopulate dropdowns if validation fails
            viewModel.Clients = await _context.Clients
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            viewModel.ServiceLevels = new List<SelectListItem>
            {
                new() { Value = "Basic", Text = "Basic" },
                new() { Value = "Premium", Text = "Premium" },
                new() { Value = "Enterprise", Text = "Enterprise" }
            };
            
            return View(viewModel);
        }
        
        // Helper method for file upload - UUID naming to prevent overwrites
        private async Task<string> SaveUploadedFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "agreements");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            
            // Generate UUID filename to prevent overwrites
            var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return "/uploads/agreements/" + fileName;
        }
        
        // GET: Contract/DownloadAgreement
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                return NotFound();
            }
            
            var filePath = Path.Combine(_environment.WebRootPath, 
                contract.SignedAgreementPath.TrimStart('/'));
                
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            
            var fileName = Path.GetFileName(filePath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}