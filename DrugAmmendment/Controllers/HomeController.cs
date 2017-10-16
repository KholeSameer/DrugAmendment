using DrugAmmendment.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DrugAmmendment.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddDrugView()
        {
            return View();
        }
        
        public ActionResult RemoveDrugView()
        {
            return View();
        }
        public void RemoveDrug()
        {
            string drugName = Request["drugName"].ToString();
            Response.Write(drugName);
            Response.Write("Done");
        }

        public ActionResult UpdateDrugView()
        {
            return View();
        }
        public void UpdateDrug()
        {
            string drugName = Request["drugName"].ToString();
            Response.Write(drugName);
            Response.Write("Done");
        }

        public ActionResult Dashboard()
        {
            DropDownClass dd = new DropDownClass();
            dd.ClientName = PopulateClients();
            //dd.CriteriaType = null;
            //dd.CriteriaType = PopulateCriteriaType("nl.ranbaxy");
            return View(dd);
        }

        private static List<SelectListItem> PopulateClients()
        {
            List<SelectListItem> clients = new List<SelectListItem>();
            //SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\Drug_Ammendment\DrugAmmendment\App_Data\DB.mdf;Integrated Security=True");
            SqlConnection conn = new SqlConnection(@"Data Source=LAINPUMA0601\ADISBE;Initial Catalog=adisdb-local;Persist Security Info=True;User ID=usr_datacleaning;Password=usr_datacleaning");
            conn.Open();
            SqlCommand cmd = new SqlCommand("select Distinct Delivery from dbo.ADFeedSelectionCriteriaLookup", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                clients.Add(new SelectListItem { Text = reader[0].ToString() });
            }
            conn.Close();

            return clients;
        }

        [HttpGet]
        public JsonResult PopulateCriteriaType(string ClientName)
        {
            List<SelectListItem> criteriaType = new List<SelectListItem>();
            SqlConnection conn = new SqlConnection(@"Data Source=LAINPUMA0601\ADISBE;Initial Catalog=adisdb-local;Persist Security Info=True;User ID=usr_datacleaning;Password=usr_datacleaning");
            conn.Open();
            SqlCommand cmd = new SqlCommand(string.Format("select Distinct CriteriaType from dbo.ADFeedSelectionCriteriaLookup where Delivery = '{0}'", ClientName),conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                criteriaType.Add(new SelectListItem { Text = reader[0].ToString() });
            }
            conn.Close();
            return Json(criteriaType, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDrugList(string ClientName, string CriteriaType)
        {
            List<DrugDetails> ddList = new List<DrugDetails>();

            SqlConnection conn = new SqlConnection(@"Data Source=LAINPUMA0601\ADISBE;Initial Catalog=adisdb-local;Persist Security Info=True;User ID=usr_datacleaning;Password=usr_datacleaning");
            conn.Open();

            //SqlCommand cmd = new SqlCommand(string.Format("select Criteria,TermID,ModificationDate,CreationDate from dbo.ADFeedSelectionCriteriaLookup where Delivery = '{0}' and CriteriaType = '{1}'", ClientName, CriteriaType), conn);
            SqlCommand cmd = new SqlCommand(string.Format("select Criteria,TermID,ModificationDate,CreationDate from dbo.ADFeedSelectionCriteriaLookup where Delivery = '{0}' and CriteriaType = '{1}' and IsActive = 0", ClientName, CriteriaType), conn);     //To have active list change IsActive = 1;

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DrugDetails dd = new DrugDetails();
                dd.Criteria = reader[0].ToString();
                if (reader[1].Equals(System.DBNull.Value))
                {
                    dd.TermID = null;
                }
                else
                {
                    dd.TermID = Convert.ToInt32(reader[1]);
                }
                if (reader[2].Equals(System.DBNull.Value))
                {
                    dd.ModificationDate = null;
                }
                else
                {
                    dd.ModificationDate = Convert.ToDateTime(reader[2]);
                }
                if (reader[3].Equals(System.DBNull.Value))
                {
                    dd.CreationDate = null;
                }
                else
                {   
                    dd.CreationDate = Convert.ToDateTime(reader[3]);
                }
                //DrugDetails dd = new DrugDetails { Criteria = reader[0].ToString(), TermID = Convert.ToInt32(reader[1]), ModificationDate = Convert.ToDateTime(reader[2]), CreationDate = Convert.ToDateTime(reader[3]) };
                ddList.Add(dd);
            }
            conn.Close();
            //Response.Write("<h1>" + ClientName + "</h1><h1>" + CriteriaType + "</h1>");
            var data = Json(ddList, JsonRequestBehavior.AllowGet);
            return data;
            
        }

    }
}