using System;
using CodecControl.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodecControl.Data.Database
{
    public class CcmDbContext : DbContext
    {
        public CcmDbContext(DbContextOptions<CcmDbContext> options) : base(options)
        {
        }


        public DbSet<RegisteredSipEntity> RegisteredSips { get; set; }
    }
}
