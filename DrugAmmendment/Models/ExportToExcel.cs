using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugAmmendment.Models
{
    public class ExportToExcel
    {
        public string Delivery { get; set; }
        public string CriteriaType { get; set; }
        public string Criteria { get; set; }
        public int? TermID { get; set; }
        public string ModificationDate { get; set; }
        public string CreationDate { get; set; }
    }
}