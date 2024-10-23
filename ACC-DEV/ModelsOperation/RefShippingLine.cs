using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.ModelsOperation;

[Table("Ref_ShippingLine")]
public partial class RefShippingLine
{
    [Key]
    [Column("ShippingLineID")]
    [StringLength(20)]
    public string ShippingLineId { get; set; } = null!;

    [StringLength(300)]
    public string Name { get; set; } = null!;

    [StringLength(300)]
    public string? AgentName { get; set; }

    [StringLength(250)]
    public string? Address1 { get; set; }

    [StringLength(250)]
    public string? Address2 { get; set; }

    [StringLength(250)]
    public string? Address3 { get; set; }

    [StringLength(250)]
    public string? City { get; set; }

    [StringLength(200)]
    public string? Country { get; set; }

    [StringLength(15)]
    public string? TelNo { get; set; }

    [StringLength(15)]
    public string? FaxNo { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Website { get; set; }

    [StringLength(100)]
    public string? CustomCode { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(1)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }


    //[InverseProperty("ShippingLineNavigation")]
    //public virtual ICollection<TxnImportJobHd> TxnImportJobHds { get; } = new List<TxnImportJobHd>();

    //[InverseProperty("ShippingLineExportNavigation")]
    //public virtual ICollection<TxnExportJobHD> TxnExportJobHds { get; } = new List<TxnExportJobHD>();

    //[InverseProperty("ShippingLine")]
    //public virtual ICollection<TxnStuffingPlanHd> TxnStuffingPlanHds { get; } = new List<TxnStuffingPlanHd>();

    //[InverseProperty("ColoaderNavigation")]
    //public virtual ICollection<TxnStuffingPlanHd> TxnStuffingPlanHdsCoLoad { get; } = new List<TxnStuffingPlanHd>();

    [InverseProperty("ShippingLineNavigation")]
    public virtual ICollection<TxnImportJobHd> TxnImportJobHds { get; } = new List<TxnImportJobHd>();

    [InverseProperty("ShippingLineExportNavigation")]
    public virtual ICollection<TxnExportJobHD> TxnExportJobHds { get; } = new List<TxnExportJobHD>();

    [InverseProperty("ColoaderExportNavigation")]
    public virtual ICollection<TxnExportJobHD> TxnExportJobHdsColoader { get; } = new List<TxnExportJobHD>();

    [InverseProperty("ShippingLine")]
    public virtual ICollection<TxnStuffingPlanHd> TxnStuffingPlanHds { get; } = new List<TxnStuffingPlanHd>();

    [InverseProperty("ColoaderNavigation")]
    public virtual ICollection<TxnStuffingPlanHd> TxnStuffingPlanHdsCoLoad { get; } = new List<TxnStuffingPlanHd>();


}
