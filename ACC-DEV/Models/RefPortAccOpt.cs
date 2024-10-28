using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[Table("Ref_PortsAccOpt")]
public partial class RefPortAccOpt
{
    [Key]
    [StringLength(20)]
    public string PortCode { get; set; } = null!;

    [StringLength(300)]
    public string PortName { get; set; } = null!;

    [StringLength(250)]
    public string Country { get; set; } = null!;

    [StringLength(100)]
    public string? Custom { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(1)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }

    [InverseProperty("Port")]
    public virtual ICollection<RefAgentAccOpt> RefAgents { get; } = new List<RefAgentAccOpt>();

}
