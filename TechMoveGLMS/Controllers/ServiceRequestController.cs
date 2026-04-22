using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models.Entities;
using TechMoveGLMS.Models.ViewModels;
using TechMoveGLMS.Services;
using TechMoveGLMS.Services.Notifications;
using TechMoveGLMS.Services.Pricing;
//(Microsoft,2026)
namespace TechMoveGLMS.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrencyService _currencyService;
        private readonly PricingContext _pricingContext;
        private readonly NotificationService _notificationService;
        
        public ServiceRequestController(
            ApplicationDbContext context,
            CurrencyService currencyService,
            PricingContext pricingContext,
            NotificationService notificationService)
        {
            _context = context;
            _currencyService = currencyService;
            _pricingContext = pricingContext;
            _notificationService = notificationService;
        }
        
        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c.Client)
                .ToListAsync();
            return View(requests);
        }
        
        // GET: ServiceRequest/Create
        public IActionResult Create()
        {
            var viewModel = new ServiceRequestViewModel
            {
                Contracts = _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.Status == "Active" && c.EndDate >= DateTime.Now)
                    .Select(c => new SelectListItem 
                    { 
                        Value = c.Id.ToString(), 
                        Text = $"{c.Client.Name} - {c.ServiceLevel} (Expires: {c.EndDate:dd/MM/yyyy})"
                    })
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
        
        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel viewModel)
        {
            // Validate that the selected contract is active and not expired
            var contract = await _context.Contracts.FindAsync(viewModel.ContractId);
            
            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Selected contract does not exist.");
            }
            else if (contract.Status != "Active")
            {
                ModelState.AddModelError("ContractId", "Cannot create service request for inactive contract.");
            }
            else if (contract.EndDate < DateTime.Now)
            {
                ModelState.AddModelError("ContractId", "Cannot create service request for expired contract.");
            }
            
            if (ModelState.IsValid && contract != null)
            {
                // Calculate cost in USD (base cost)
                decimal baseCostUsd = viewModel.ServiceLevel.ToLower() switch
                {
                    "premium" => 250.00m,
                    "enterprise" => 500.00m,
                    _ => 100.00m // Basic
                };
                
                // Apply pricing strategy
                _pricingContext.SetStrategyByServiceLevel(viewModel.ServiceLevel);
                var finalCostUsd = _pricingContext.ExecuteStrategy(
                    baseCostUsd, 
                    viewModel.ServiceLevel,
                    viewModel.Distance,
                    viewModel.IsPriority);
                
                // Convert USD to ZAR using external API
                var localCostZar = await _currencyService.ConvertUsdToZarAsync(finalCostUsd);
                
                var serviceRequest = new ServiceRequest
                {
                    ContractId = viewModel.ContractId,
                    Description = viewModel.Description,
                    ServiceLevel = viewModel.ServiceLevel,
                    Status = "Draft",
                    LocalCost = localCostZar,
                    CreatedAt = DateTime.Now
                };
                
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                
                // Notify observers about new service request
                await _notificationService.NotifyNewServiceRequestAsync(
                    $"Request #{serviceRequest.Id} - {viewModel.ServiceLevel} - Cost: R{localCostZar:F2}");
                
                TempData["Success"] = $"Service request created. Cost: R{localCostZar:F2} (Converted from ${finalCostUsd:F2})";
                return RedirectToAction(nameof(Index));
            }
            
            // Repopulate dropdowns if validation fails
            viewModel.Contracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == "Active" && c.EndDate >= DateTime.Now)
                .Select(c => new SelectListItem 
                { 
                    Value = c.Id.ToString(), 
                    Text = $"{c.Client.Name} - {c.ServiceLevel} (Expires: {c.EndDate:dd/MM/yyyy})"
                })
                .ToListAsync();
            viewModel.ServiceLevels = new List<SelectListItem>
            {
                new() { Value = "Basic", Text = "Basic" },
                new() { Value = "Premium", Text = "Premium" },
                new() { Value = "Enterprise", Text = "Enterprise" }
            };
            
            return View(viewModel);
        }
    }
}
//Microsoft, 2026. ASP.NET Core MVC Overview.[Online] Available at:
//https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-10.0