using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRAuthenticationSample.Data
{
    public class ChatDbContext : IdentityDbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public DbSet<ChatUser> ChatUser { get; set; }
    }

    public class ChatUser
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Emial { get; set; }
        public string PassWord { get; set; }
    }
}
