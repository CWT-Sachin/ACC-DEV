using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.Models;


namespace ACC_DEV.Models;

[Table("Ref_CustomerAccOpt")]
public partial class RefCustomerAccOpt
{
    [Key]
    [StringLength(20)]
    public string CustomerId { get; set; } = null!;

    [StringLength(250)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    public string CustomerType { get; set; } = null!;

    [Column("CustomerClubID")]
    [StringLength(20)]
    public string? CustomerClubId { get; set; }

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

    [StringLength(15)]
    public string? TelNo1 { get; set; }

    [StringLength(15)]
    public string? TelNo2 { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? ContactPersonName { get; set; }

    [StringLength(250)]
    public string? Designation { get; set; }

    [StringLength(15)]
    public string? TelNo { get; set; }

    [StringLength(15)]
    public string? MobileNo { get; set; }

    [StringLength(15)]
    public string? ResidenceTel { get; set; }

    [StringLength(200)]
    public string? ContactPersonEmail { get; set; }

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



    [InverseProperty("InvoiceHdAcc")] // from Accounts 
    public virtual ICollection<TxnInvoiceHd> TxnInvoiceHds { get; } = new List<TxnInvoiceHd>();

    [InverseProperty("CustomerNavigation")] // from Accounts 
    public virtual ICollection<TxnCreditSalesHD> TxnCreditSalesHDs { get; } = new List<TxnCreditSalesHD>();

    [InverseProperty("RefCustomerNavigation")] // from Accounts 
    public virtual ICollection<TxnReceiptHD> TxnReceiptHDs { get; } = new List<TxnReceiptHD>();





}
