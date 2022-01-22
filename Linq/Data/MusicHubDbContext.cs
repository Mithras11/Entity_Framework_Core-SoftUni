namespace MusicHub.Data
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Models.Enums;

    public class MusicHubDbContext : DbContext
    {
        public MusicHubDbContext()
        {
        }

        public MusicHubDbContext(DbContextOptions options)
            : base(options)
        {
        }


        public DbSet<Album> Albums { get; set; }
        public DbSet<Performer> Performers { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<SongPerformer> SongsPerformers { get; set; }
        public DbSet<Writer> Writers { get; set; }




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer(Configuration.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<SongPerformer>()                     also works?!
            //.HasKey(x => new { x.SongId, x.PerformerId });

            builder.Entity<SongPerformer>(sp =>
            {
                sp.HasKey(x => new { x.SongId, x.PerformerId });
            });



            builder
                .Entity<Song>()
                .Property(e => e.Genre)
                .HasConversion(
                    v => v.ToString(),
                    v => (Genre)Enum.Parse(typeof(Genre), v));
        }
    }
}
