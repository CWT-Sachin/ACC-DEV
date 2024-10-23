using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACC_DEV.Models
{
    [Table("Txn_CreditSalesDtl")]
    public partial class TxnCreditSalesDtl
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string PaymentNo { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string SerialNo { get; set; }

        [StringLength(50)]
        public string LineAccNo { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "numeric(18, 2)")]
        public decimal? Amount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDateTime { get; set; }
    }
}
