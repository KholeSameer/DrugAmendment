using DrugAmmendment.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.UI;

namespace DrugAmmendment.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddDrugView(FormCollection form)
        {
            string client = form["SelectClient"];
            string criteriaType = form["criteriaType"];
            //Response.Write("Client : "+ client + "  Criteria Type : " + criteriaType);
            ViewBag.client = client;
            ViewBag.criteriaType = criteriaType;
            return View();
        }

        [HttpPost]
        public void AddDrug(FormCollection form)
        {
            //string criteriaFromUser = Criteria;
            //string delivery = Delivery;
            //string criteriaType = CriteriaType;

            string criteriaFromUser = form["criteria"];
            string delivery = form["client"];
            string criteriaType = form["criteriaType"];

            bool isValidLeadTerm = ValidateLeadTerm(criteriaFromUser);
            if (isValidLeadTerm)
            {
                ArrayList ThesData  = GetDataFromThesTerm(criteriaFromUser);
                string criteria = ThesData[0].ToString();
                int termID = Convert.ToInt32(ThesData[1]);
                DateTime now = DateTime.Now;
                bool isAddedToDB = AddDrugToDB(delivery, criteriaType, criteria, termID);
                if (isAddedToDB)
                {
                    Response.Write("<script>window.alert(\'Drug Added Successfully\');window.location='Dashboard';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'Drug Not Added in DB\');window.location='Dashboard';</script>");
                }
            }
            else
            {
                ViewBag.client = delivery;
                ViewBag.criteriaType = criteriaType;
                Response.Write("<script>window.alert(\'This drug is not a valid drug...! Please insert a valid one...!\');window.location='AddDrugView'</script>");
            }
        }

        public bool ValidateLeadTerm(string criteria)
        {
            bool flag = false ;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(string.Format("select LeadTerm FROM [adisdb-local].[dbo].[THSTerm] where LeadTerm  like '{0}'", criteria), conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader[0].ToString().Equals(criteria, StringComparison.InvariantCultureIgnoreCase))
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                }
            }
            conn.Close();
            return flag;
        }

        private ArrayList GetDataFromThesTerm(string criteria)
        {
            ArrayList data = new ArrayList();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(string.Format("select LeadTerm, TermID from [adisdb-local].[dbo].[THSTerm] where LeadTerm = '{0}'", criteria), conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                data.Add(reader[0].ToString());
                data.Add(reader[1]);
            }
            conn.Close();
            return data;
        }

        private bool AddDrugToDB(string delivery, string criteriaType, string criteria, int termID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            
            SqlCommand cmd = new SqlCommand(string.Format("insert into dbo.ADFeedSelectionCriteriaLookup values ('{0}','{1}','{2}',{3},{4},'{5}','{6}')", delivery, criteriaType, criteria, termID, 1, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") ), conn);
            int RA = 0;
            try
            {
                conn.Open();
                RA = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Response.Write("<script>window.alert(\'Drug " + criteria + " is already present in " + delivery + " Table.\');window.location='Dashboard';</script>");
            }
            finally {
                conn.Close();
            }
                         
            if (RA > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            return View(dd);
        }

        private static List<SelectListItem> PopulateClients()
        {
            List<SelectListItem> clients = new List<SelectListItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
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
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
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
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            //SqlCommand cmd = new SqlCommand(string.Format("select Criteria,TermID,ModificationDate,CreationDate from dbo.ADFeedSelectionCriteriaLookup where Delivery = '{0}' and CriteriaType = '{1}'", ClientName, CriteriaType), conn);
            SqlCommand cmd = new SqlCommand(string.Format("select Criteria,TermID,ModificationDate,CreationDate from dbo.ADFeedSelectionCriteriaLookup where Delivery = '{0}' and CriteriaType = '{1}' and IsActive = 1", ClientName, CriteriaType), conn);     //To have active list change IsActive = 1;

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

        public ActionResult DeleteDrugView()
        {
            ViewBag.CriteriaType = Request.QueryString["criteriaType"];
            ViewBag.Delivery = Request.QueryString["clientName"];
            return View();
        }

        public void DeleteDrug(FormCollection form)
        {
            

            Response.Write("<h1>" + form["criteria"] + " <br/>  " + form["client"] + " <br/> " + form["criteriaType"] + "</h1>" );
        }

        public JsonResult GetAutoCriteria(string criteria, string delivery, string criteriaType)
        {
            List<string> CriteriaList = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(string.Format("SELECT distinct Criteria FROM dbo.ADFeedSelectionCriteriaLookup WHERE Delivery = '{0}' and  CriteriaType = '{1}' and Criteria LIKE '%{2}%'", delivery, criteriaType, criteria), conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CriteriaList.Add(reader[0].ToString());
            }
            conn.Close();
            return Json(CriteriaList, JsonRequestBehavior.AllowGet);
        }
    }
}