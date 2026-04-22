using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Services;
using TechMoveGLMS.Models.ViewModels;
using TechMoveGLMS.Services.Contracts;
using TechMoveGLMS.Services.Notifications;

namespace TechMoveGLMS.Controllers
{
    //(Microsoft,2026)
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractFactory _contractFactory;
        private readonly NotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;
        private readonly AuthService _authService;
        
        public ContractController(
            ApplicationDbContext context,
            IContractFactory contractFactory,
            NotificationService notificationService,
            IWebHostEnvironment environment,
            AuthService authService)
        {
            _context = context;
            _contractFactory = contractFactory;
            _notificationService = notificationService;
            _environment = environment;
             _authService = authService;
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
                // GET: Contract/Edit/5 - Admin Only
        public async Task<IActionResult> Edit(int? id)
        {
         // Only Admin can edit contracts
             if (!_authService.IsAdmin())
            {
               return RedirectToAction("AccessDenied", "Account");
            }
    
             if (id == null) return NotFound();
    
             var contract = await _context.Contracts
             .Include(c => c.Client)
             .FirstOrDefaultAsync(c => c.Id == id);
        
            if (contract == null) return NotFound();
    
             var viewModel = new ContractViewModel
            {
                 Id = contract.Id,
                 ClientId = contract.ClientId,
                 StartDate = contract.StartDate,
                 EndDate = contract.EndDate,
                 ServiceLevel = contract.ServiceLevel,
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
    
            // Add Status to ViewBag for the dropdown
         ViewBag.StatusList = new List<SelectListItem>
         {
             new() { Value = "Draft", Text = "Draft" },
             new() { Value = "Active", Text = "Active" },
             new() { Value = "Expired", Text = "Expired" }
         };
            ViewBag.CurrentStatus = contract.Status;
    
            return View(viewModel);
         }
          // POST: Contract/Edit/5 - Admin Only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractViewModel viewModel, string status)
            {
             // Only Admin can edit contracts
            if (!_authService.IsAdmin())
            {
             return RedirectToAction("AccessDenied", "Account");

            }
    
              if (id != viewModel.Id) return NotFound();
    
                 // File validation - only PDF allowed
              if (viewModel.SignedAgreement != null)
               {
                  var extension = Path.GetExtension(viewModel.SignedAgreement.FileName).ToLower();
                 if (extension != ".pdf")
                 {
                     ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
                 }
                }
    
                  if (ModelState.IsValid)
                 {
                    var contract = await _context.Contracts.FindAsync(id);
                  if (contract == null) return NotFound();
        
                     contract.ClientId = viewModel.ClientId;
                     contract.StartDate = viewModel.StartDate;
                     contract.EndDate = viewModel.EndDate;
                     contract.ServiceLevel = viewModel.ServiceLevel;
                    contract.Status = status; // ← Update status from dropdown
        
                  // Handle file upload if new file provided
                  if (viewModel.SignedAgreement != null && viewModel.SignedAgreement.Length > 0)
                 {
                     // Delete old file if exists
                   if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                 {
                      var oldFilePath = Path.Combine(_environment.WebRootPath, 
                       contract.SignedAgreementPath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
             }
             contract.SignedAgreementPath = await SaveUploadedFile(viewModel.SignedAgreement);
         }
        
                  _context.Update(contract);
                 await _context.SaveChangesAsync();
        
                 await _notificationService.NotifyAllAsync(
                 "Contract Updated",
                 $"Contract #{contract.Id} status changed to {contract.Status}");
        
                  TempData["Success"] = $"Contract #{contract.Id} updated successfully!";
                 return RedirectToAction(nameof(Index));
    }
    
             viewModel.Clients = await _context.Clients
             .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
            viewModel.ServiceLevels = new List<SelectListItem>
    {
             new() { Value = "Basic", Text = "Basic" },
             new() { Value = "Premium", Text = "Premium" },
             new() { Value = "Enterprise", Text = "Enterprise" }
    };
    
         ViewBag.StatusList = new List<SelectListItem>
        {
            new() { Value = "Draft", Text = "Draft" },
            new() { Value = "Active", Text = "Active" },
            new() { Value = "Expired", Text = "Expired" }
        };
            ViewBag.CurrentStatus = status;
    
            return View(viewModel);

        }

    }
 
}       
//Microsoft, 2026. ASP.NET Core MVC Overview.[Online] Available at:
//https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-10.0