using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrugAmmendment.Models
{
    public class DrugDetails
    {
        public string Criteria { get; set; }
        public int? TermID { get; set; }
        public DateTime? ModificationDate { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}