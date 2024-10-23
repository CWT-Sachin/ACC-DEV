using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.ModelsOperation;

[Table("Txn_BLStatus")]
public partial class TxnBLStatus

{
    [Key]
    [Required]
    [StringLength(50)]
    public string BLno { get; set; } = null!;

    [StringLength(20)]
    public string? ShipmentType { get; set; }

    public bool? IsTS { get; set; }

    [StringLength(50)]
    public string? TSBLNo { get; set; }

    [StringLength(20)]
    public string? Leg2Vessel { get; set; }

    [StringLength(20)]
    public string? Leg2Voyage { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg2ETD { get; set; }

    [StringLength(20)]
    public string? Leg2PortofDischarge { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg2ETA_POD { get; set; }

    [StringLength(20)]
    public string? Leg2ActDepartureStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg2ActATD { get; set; }

    [StringLength(50)]
    public string? Leg2ContainerNo { get; set; }

    [StringLength(50)]
    public string? Leg2ContainerSealNo { get; set; }

    [StringLength(20)]
    public string? Leg2ContainerSize { get; set; }

    public bool? Leg3ISLegYes { get; set; }

    [StringLength(20)]
    public string? Leg3Vessel { get; set; }

    [StringLength(20)]
    public string? Leg3Voyage { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg3ETD { get; set; }

    [StringLength(20)]
    public string? Leg3PortofDischarge { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg3ETA_POD { get; set; }

    [StringLength(20)]
    public string? Leg3ActDepartureStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg3ActATD { get; set; }

    [StringLength(50)]
    public string? Leg3ContainerNo { get; set; }

    [StringLength(50)]
    public string? Leg3ContainerSealNo { get; set; }

    [StringLength(20)]
    public string? Leg3ContainerSize { get; set; }

    public bool? Leg4ISLegYes { get; set; }

    [StringLength(20)]
    public string? Leg4Vessel { get; set; }

    [StringLength(20)]
    public string? Leg4Voyage { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg4ETD { get; set; }

    [StringLength(20)]
    public string? Leg4PortofDischarge { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg4ETA_POD { get; set; }

    [StringLength(20)]
    public string? Leg4ActDepartureStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Leg4ActATD { get; set; }

    [StringLength(50)]
    public string? Leg4ContainerNo { get; set; }

    [StringLength(50)]
    public string? Leg4ContainerSealNo { get; set; }

    [StringLength(20)]
    public string? Leg4ContainerSize { get; set; }

    [StringLength(20)]
    public string? FinalFDN { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? FinalETA { get; set; }

    [StringLength(20)]
    public string? FinalActualArrivalStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? FinalActualATA { get; set; }

    [StringLength(20)]
    public string? DOReleasedStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? DOReleasedDate { get; set; }

    public bool IsDoorDelivery { get; set; } = false;

    [StringLength(20)]
    public string? DoorDeliveryStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? DoorDeliveryCompletedDate { get; set; }

    [StringLength(20)]
    public string? CargoReleasedStatus { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? CargoReleasedDate { get; set; }

    [StringLength(350)]
    public string? Remarks { get; set; }

    [StringLength(20)]
    public string? ImportVessel { get; set; }

    [StringLength(50)]
    public string? ImportVoyage { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? ImportETAColombo { get; set; }

    [StringLength(50)]
    public string? ImportContainerNo { get; set; }

    [StringLength(50)]
    public string? ImportContainerSealNo { get; set; }

    [StringLength(20)]
    public string? ImportContainerSize { get; set; }

    [StringLength(20)]
    public string? BookingReceived { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? BookingReceivedDate { get; set; }

    [StringLength(20)]
    public string? WarehouseReceived { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? WarehouseReceivedDate { get; set; }

    [StringLength(20)]
    public string? StuffingDone { get; set; }
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? StuffingDate { get; set; }


    [ForeignKey("FinalFDN")]
    [InverseProperty("TxnBLStatuses")]
    public virtual RefPort? FinalFDNNavigation { get; set; }

    [ForeignKey("Leg2PortofDischarge")]
    [InverseProperty("Leg2BLStatuses")]
    public virtual RefPort? Leg2PortofDischargeNavigation { get; set; }

    [ForeignKey("Leg3PortofDischarge")]
    [InverseProperty("Leg3BLStatuses")]
    public virtual RefPort? Leg3PortofDischargeNavigation { get; set; }

    [ForeignKey("Leg4PortofDischarge")]
    [InverseProperty("Leg4BLStatuses")]
    public virtual RefPort? Leg4PortofDischargeNavigation { get; set; }



    [ForeignKey("Leg2Vessel")]
    [InverseProperty("Leg2VBLStatuses")]
    public virtual RefVessel? Leg2VesselNavigation { get; set; }

    [ForeignKey("Leg3Vessel")]
    [InverseProperty("Leg3VBLStatuses")]
    public virtual RefVessel? Leg3VesselNavigation { get; set; }

    [ForeignKey("Leg4Vessel")]
    [InverseProperty("Leg4VBLStatuses")]
    public virtual RefVessel? Leg4VesselNavigation { get; set; }

}






