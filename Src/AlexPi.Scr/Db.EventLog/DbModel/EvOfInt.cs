namespace Db.EventLog.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("EvOfInt")]
    public partial class EvOfInt
    {
        [Key]
        [Column(Order = 0)]
        public DateTime TimeID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string MachineName { get; set; }

        public int EvOfIntFlag { get; set; }

        public virtual PcLogic PcLogic { get; set; }
    }
}
