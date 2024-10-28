using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.ModelsOperation;
using ACC_DEV.Models;

namespace ACC_DEV.Models;

[Table("Ref_Supplier")]
public partial class RefSupplier
{
    [Key]
    [StringLength(20)]
    public string SupplierId { get; set; } = null!;

    [StringLength(250)]
    public string Name { get; set; } = null!;


    [StringLength(250)]
    public string? Address1 { get; set; }

    [StringLength(250)]
    public string? Address2 { get; set; }

    [StringLength(250)]
    public string? Address3 { get; set; }

    [StringLength(250)]
    public string? City { get; set; }

    [StringLength(200)]
    public string? County { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

     [StringLength(15)]
    public string? TelNo { get; set; }

    [StringLength(15)]
    public string? MobileNo { get; set; }


    [StringLength(100)]
    public string? VatReg { get; set; }

    [StringLength(100)]
    public string? SvatReg { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(20)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }


    [InverseProperty("SupplierNavigation")] // from Accounts 
    public virtual ICollection<TxnPurchasVoucherHD> TxnPurchasVoucherHD { get; } = new List<TxnPurchasVoucherHD>();

    [InverseProperty("SupplierPettyNavigation")] // from Accounts 
    public virtual ICollection<TxnPettyCashHD> TxnPettyCashHD { get; } = new List<TxnPettyCashHD>();

    [InverseProperty("RefSupplierNavigation")] // from Accounts 
    public virtual ICollection<TxnPaymentHDs> TxnPaymentHDs { get; } = new List<TxnPaymentHDs>();
}
