using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public enum ServiceRequestStatus { Pending, InProgress, Completed, Cancelled }

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Cost in USD as entered by user.</summary>
    [Range(0, double.MaxValue)]
    public decimal CostUsd { get; set; }

    /// <summary>Cost converted to ZAR at time of creation.</summary>
    public decimal CostZar { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
