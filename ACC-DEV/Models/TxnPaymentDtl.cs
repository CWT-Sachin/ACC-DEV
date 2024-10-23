using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACC_DEV.Models;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models
{
    [PrimaryKey("PaymentNo", "RefNo")]
    [Table("Txn_PaymentDtl")]
    public partial class TxnPaymentDtl
    {
        [Key]
        [StringLength(50)]
        public string? PaymentNo { get; set; }

        [Key]
        [StringLength(50)]
        public string? RefNo { get; set; }

        [StringLength(50)]
        public string? RefType { get; set; }

        [Column(TypeName = "numeric(18, 2)")]
        public decimal? Amount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDateTime { get; set; }
    }
}
