namespace Db.EventLog.DbModel
{
    using System.Data.Entity;

    public partial class A0DbModel : DbContext
    {
        //public A0DbModel()
        //    : base("name=A0DbModel")
        //{
        //}

        public virtual DbSet<EvOfInt> EvOfInts { get; set; }
        public virtual DbSet<PcLogic> PcLogics { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PcLogic>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<PcLogic>()
                .HasMany(e => e.EvOfInts)
                .WithRequired(e => e.PcLogic)
                .WillCascadeOnDelete(false);
        }
    }
}
