namespace Db.EventLog.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PcLogic")]
    public partial class PcLogic
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PcLogic()
        {
            EvOfInts = new HashSet<EvOfInt>();
        }

        [Key] [StringLength(50)] public string MachineName { get; set; }

        [Required] [StringLength(10)] public string ColorRGB { get; set; }

        public double DailyMaxHr { get; set; }

        [Column(TypeName = "text")] public string Note { get; set; }
        [Column(TypeName = "text")] public string Info { get; set; } = null;

        public DateTime? CreatedAt { get; set; }
        public DateTime? LogReadAt { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EvOfInt> EvOfInts { get; set; }
    }
}
