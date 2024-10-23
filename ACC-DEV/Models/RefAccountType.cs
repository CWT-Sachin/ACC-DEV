﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[Table("Ref_AccountTypes")]
public partial class RefAccountType
{
    [Key]
    [Column("AccTypeID")]
    [StringLength(20)]
    public string? AccTypeID { get; set; }
    [StringLength(300)]
    public string? AccTypeName { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(20)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }


    [InverseProperty("RefAccountType")]
    public virtual ICollection<RefChartOfAcc> RefChartOfAcc { get; set; } = new List<RefChartOfAcc>();



}
