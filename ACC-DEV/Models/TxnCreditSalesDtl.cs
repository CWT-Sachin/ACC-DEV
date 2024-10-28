using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models
{
    [PrimaryKey("CreditSalesNo", "SerialNo")]
    [Table("Txn_CreditSalesDtl")]
    public partial class TxnCreditSalesDtl
    {
        [Key]
        [StringLength(50)]
        public string CreditSalesNo { get; set; }

        [Key]
        [StringLength(50)]
        public string SerialNo { get; set; }

        [StringLength(50)]
        public string LineAccNo { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "numeric(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? Amount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDateTime { get; set; }

    }
}
