using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ServiceRequestController : Controller
{
    private readonly GlmsDbContext _db;
    private readonly ICurrencyService _currency;

    public ServiceRequestController(GlmsDbContext db, ICurrencyService currency)
    {
        _db = db; _currency = currency;
    }

    public async Task<IActionResult> Index() =>
        View(await _db.ServiceRequests.Include(sr => sr.Contract).ToListAsync());

    public async Task<IActionResult> Create()
    {
        // Only allow active contracts
        var activeContracts = await _db.Contracts
            .Where(c => c.Status == ContractStatus.Active)
            .Include(c => c.Client)
            .ToListAsync();

        ViewBag.Contracts = new SelectList(activeContracts, "Id", "ServiceLevel");
        ViewBag.UsdToZarRate = await _currency.GetUsdToZarRateAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest request)
    {
        // Workflow guard: re-validate contract status server-side
        var contract = await _db.Contracts.FindAsync(request.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError("", "Contract not found.");
        }
        else if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
        {
            ModelState.AddModelError("", $"Cannot create a service request for a contract that is {contract.Status}.");
        }

        if (!ModelState.IsValid)
        {
            var activeContracts = await _db.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .Include(c => c.Client).ToListAsync();
            ViewBag.Contracts = new SelectList(activeContracts, "Id", "ServiceLevel");
            ViewBag.UsdToZarRate = await _currency.GetUsdToZarRateAsync();
            return View(request);
        }

        var rate = await _currency.GetUsdToZarRateAsync();
        request.CostZar = _currency.Convert(request.CostUsd, rate);
        request.CreatedAt = DateTime.UtcNow;

        _db.ServiceRequests.Add(request);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var sr = await _db.ServiceRequests.Include(s => s.Contract).FirstOrDefaultAsync(s => s.Id == id);
        return sr == null ? NotFound() : View(sr);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var sr = await _db.ServiceRequests.Include(s => s.Contract).FirstOrDefaultAsync(s => s.Id == id);
        return sr == null ? NotFound() : View(sr);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sr = await _db.ServiceRequests.FindAsync(id);
        if (sr != null) { _db.ServiceRequests.Remove(sr); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    // AJAX endpoint: returns ZAR equivalent for a given USD amount
    [HttpGet]
    public async Task<IActionResult> ConvertRate(decimal usd)
    {
        var rate = await _currency.GetUsdToZarRateAsync();
        return Json(new { zar = _currency.Convert(usd, rate), rate });
    }
}
