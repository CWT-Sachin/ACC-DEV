using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACC_DEV.ModelsOperation;
using ACC_DEV.Models;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[Table("Ref_BankAcc")]
public partial class Ref_BankAcc
{
    [Key]
    [StringLength(20)]
    public string ID { get; set; } = null!;

    [StringLength(20)]
    public string? BankCode { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(20)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }

    [InverseProperty("RefBankNavigation")]
    public virtual ICollection<TxnReceiptHD> TxnReceiptHDs { get; } = new List<TxnReceiptHD>();

    [InverseProperty("RefBankPayNavigation")]
    public virtual ICollection<TxnPaymentHDs> TxnPaymentHDs { get; } = new List<TxnPaymentHDs>();

}
