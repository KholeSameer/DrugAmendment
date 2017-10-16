using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DrugAmmendment.Models
{
    public class DropDownClass
    {
        public List<SelectListItem> ClientName { get; set; }
        public List<SelectListItem> CriteriaType { get; set; }
    }

}