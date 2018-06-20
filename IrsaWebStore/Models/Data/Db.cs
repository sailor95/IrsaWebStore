using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace IrsaWebStore.Models.Data
{
    public class Db : DbContext
    {
        public DbSet<PageDTO> Pages { get; set; }
        public DbSet<SidebarDTO> Sidebar { get; set; }

        public System.Data.Entity.DbSet<IrsaWebStore.Models.ViewModel.Pages.PageVM> PageVMs { get; set; }

        public System.Data.Entity.DbSet<IrsaWebStore.Models.ViewModel.Pages.SidebarVM> SidebarVMs { get; set; }
    }
}