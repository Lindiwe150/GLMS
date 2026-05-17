using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ContractController : Controller
{
    private readonly GlmsDbContext _db;
    private readonly IFileService _fileService;
    private readonly IWebHostEnvironment _env;

    public ContractController(GlmsDbContext db, IFileService fileService, IWebHostEnvironment env)
    {
        _db = db; _fileService = fileService; _env = env;
    }

    // GET: /Contract?startDate=&endDate=&status=
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? status)
    {
        var query = _db.Contracts.Include(c => c.Client).AsQueryable();

        if (startDate.HasValue) query = query.Where(c => c.StartDate >= startDate.Value);
        if (endDate.HasValue)   query = query.Where(c => c.EndDate <= endDate.Value);
        if (status.HasValue)    query = query.Where(c => c.Status == status.Value);

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate   = endDate?.ToString("yyyy-MM-dd");
        ViewBag.Status    = status;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = new SelectList(await _db.Clients.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract, IFormFile? signedAgreement)
    {
        if (signedAgreement != null)
        {
            if (!_fileService.IsPdf(signedAgreement))
            {
                ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
            }
            else
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                contract.SignedAgreementPath = await _fileService.SavePdfAsync(signedAgreement, uploadsFolder);
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = new SelectList(await _db.Clients.ToListAsync(), "Id", "Name");
            return View(contract);
        }

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _db.Contracts.FindAsync(id);
        if (contract == null) return NotFound();
        ViewBag.Clients = new SelectList(await _db.Clients.ToListAsync(), "Id", "Name", contract.ClientId);
        return View(contract);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? signedAgreement)
    {
        if (id != contract.Id) return BadRequest();

        if (signedAgreement != null)
        {
            if (!_fileService.IsPdf(signedAgreement))
            {
                ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
            }
            else
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                contract.SignedAgreementPath = await _fileService.SavePdfAsync(signedAgreement, uploadsFolder);
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = new SelectList(await _db.Clients.ToListAsync(), "Id", "Name", contract.ClientId);
            return View(contract);
        }

        _db.Contracts.Update(contract);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _db.Contracts.Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);
        return contract == null ? NotFound() : View(contract);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var contract = await _db.Contracts.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == id);
        return contract == null ? NotFound() : View(contract);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _db.Contracts.FindAsync(id);
        if (contract != null) { _db.Contracts.Remove(contract); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    // Download signed agreement
    public IActionResult Download(int id)
    {
        var contract = _db.Contracts.Find(id);
        if (contract?.SignedAgreementPath == null) return NotFound();
        var fullPath = Path.Combine(_env.WebRootPath, contract.SignedAgreementPath);
        if (!System.IO.File.Exists(fullPath)) return NotFound();
        return PhysicalFile(fullPath, "application/pdf", Path.GetFileName(fullPath));
    }
}
