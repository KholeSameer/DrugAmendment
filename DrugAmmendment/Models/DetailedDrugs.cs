using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugAmmendment.Models
{
    public class DrugDetails
    {
        public string Delivery { get; set; }
        public string CriteriaType { get; set; }
        public string Criteria { get; set; }
        public int? TermID { get; set; }
        public int IsActive { get; set; }
        public DateTime? ModificationDate { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}