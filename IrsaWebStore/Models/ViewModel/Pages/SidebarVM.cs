using IrsaWebStore.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IrsaWebStore.Models.ViewModel.Pages
{
    public class SidebarVM
    {
        public SidebarVM()
        {

        }
        public SidebarVM(SidebarDTO row)
        {
            Id = row.Id;
            Body = row.Body;
        }

        public int Id { get; set; }
        public string Body { get; set; }
    }
}