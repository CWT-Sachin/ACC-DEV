using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[Table("Txn_DaysandRange")]
public class TxnDaysandRange
{
    [Key]
    [StringLength(50)]
    public string HouseBLNo { get; set; }

    [Column(TypeName = "date")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? BLDate { get; set; }

    public decimal? CBM { get; set; }
}

