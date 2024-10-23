using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[PrimaryKey("PurchasVoucherNo", "SerialNo")]
[Table("Txn_PurchasVoucherDtl")]
public partial class TxnPurchasVoucherDtl
{
    [Key]
    [StringLength(50)]
    public string PurchasVoucherNo { get; set; } = null!;

    //[Key]
    //[StringLength(50)]
    //public string SerialNo { get; set; }

    [Key]
    [Column(TypeName = "numeric(6, 0)")]
    public decimal SerialNo { get; set; }

    [StringLength(50)]
    public string? LineAccNo { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }


    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Amount { get; set; }

    [Column(TypeName = "CreatedDate")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? CreatedDateTime { get; set; }



}
