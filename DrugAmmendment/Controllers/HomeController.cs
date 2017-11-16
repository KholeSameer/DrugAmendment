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
            if (form.Count == 0 && TempData["Client"] == null && TempData["CriteriaType"] == null)
                return Redirect("Dashboard");

            string client = form["Client"];
            string criteriaType = form["criteriaType"];

            if (TempData["Client"] == null)
            {
                TempData["Client"] = client;
            }
            if (TempData["CriteriaType"] == null)
            {
                TempData["CriteriaType"] = criteriaType;
            }
            return View();
        }

        [HttpPost]
        public void AddDrug(FormCollection form)
        {
            string criteriaFromUser = form["criteria"];
            string delivery = form["client"];
            string criteriaType = form["criteriaType"];

            bool isValidLeadTerm = ValidateLeadTerm(criteriaFromUser);
            if (isValidLeadTerm)
            {
                ArrayList ThesData  = GetDataFromThesTerm(criteriaFromUser);
                string criteria = ThesData[0].ToString();
                int termID = Convert.ToInt32(ThesData[1]);

                bool isAvailableNonActive = CheckIsAvailableNonActive(delivery, criteriaType, criteria);
                if (isAvailableNonActive)
                {
                    UpdateToActive(delivery, criteriaType, criteria);
                }
                else
                {
                    CheckIsAvailableActive(delivery, criteriaType, criteria, termID);
                }
            }
            else
            {
                TempData["Client"] = delivery;
                TempData["CriteriaType"] = criteriaType;
                Response.Write("<script>window.alert(\'This drug is not a valid drug...! Please insert a valid one...!\');window.location='AddDrugView'</script>");
            }
        }

        private void CheckIsAvailableActive(string delivery, string criteriaType, string criteria, int termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            int RA = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("select Criteria FROM ADFeedSelectionCriteriaLookup where Delivery = '{0}' and CriteriaType = '{1}' and Criteria = '{2}' and IsActive = 1 ", delivery, criteriaType, criteria), conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    RA = 1;
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'Something Went Wrong...! Please Try Later...! \');window.location='Dashboard'</script>");
            }
            finally
            {
                conn.Close();
            }

            if (RA > 0)
            {
                Response.Write("<script>window.alert(\'This drug is already Present & Active in the DB...!');window.location='AddDrugView'</script>");
            }
            else
            {
                AddDrugToDB(delivery, criteriaType, criteria, termID);
            }
            
        }

        private void UpdateToActive(string delivery, string criteriaType, string criteria)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;
            int RA = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            
            SqlCommand cmd = new SqlCommand(string.Format("update dbo.ADFeedSelectionCriteriaLookup set IsActive = 1, ModificationDate = GETDATE() where Delivery = '{0}' and CriteriaType = '{1}' and Criteria = '{2}'", delivery, criteriaType, criteria), conn);
            try
            {
                conn.Open();
                RA = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'Exception Occurred...! Please try again...!\');window.location='AddDrugView';</script>");
            }
            finally
            {
                conn.Close();
            }

            if (RA > 0 )
            {
                Response.Write("<script>window.alert(\'Drug Updated to Active Successfully\');window.location='AddDrugView';</script>");
            }
            else
            {
                Response.Write("<script>window.alert(\'Drug Not Updated\');window.location='Dashboard';</script>");
            }
            
        }

        private bool CheckIsAvailableNonActive(string delivery, string criteriaType, string criteria)
        {
            bool flag = false;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            
            SqlCommand cmd = new SqlCommand(string.Format("select Criteria FROM dbo.ADFeedSelectionCriteriaLookup where Criteria  = '{0}' and Delivery = '{1}' and IsActive = 0", criteria, delivery), conn);
            try
            {
                conn.Open();
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
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return flag;
        }

        private bool ValidateLeadTerm(string criteria)
        {
            bool flag = false ;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("select LeadTerm FROM dbo.THSTerm where LeadTerm  like '{0}'", criteria), conn);
            try
            {
                conn.Open();
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

            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return flag;
        }

        private ArrayList GetDataFromThesTerm(string criteria)
        {
            ArrayList data = new ArrayList();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("select LeadTerm, TermID from dbo.THSTerm where LeadTerm = '{0}'", criteria), conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(reader[0].ToString());
                    data.Add(reader[1]);
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return data;
        }

        private void AddDrugToDB(string delivery, string criteriaType, string criteria, int termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("insert into dbo.ADFeedSelectionCriteriaLookup values ('{0}','{1}','{2}',{3},{4},GETDATE(),GETDATE())", delivery, criteriaType, criteria, termID, 1), conn);
            int RA = 0;
            try
            {
                conn.Open();
                RA = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'Drug " + criteria + " is already present in " + delivery + " Table.Errrrrrrrrrrrrrr\');window.location='Dashboard';</script>");
            }
            finally {
                conn.Close();
            }
                         
            if (RA > 0)
            {
                Response.Write("<script>window.alert(\'Drug Added Successfully\');window.location='AddDrugView';</script>");
            }
            else
            {
                Response.Write("<script>window.alert(\'Drug Not Added in DB\');window.location='Dashboard';</script>");
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
            return View();
        }

        [HttpGet]
        public JsonResult PopulateClients()
        {
            List<SelectListItem> clients = new List<SelectListItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("select Distinct Delivery from dbo.ADFeedSelectionCriteriaLookup", conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new SelectListItem { Text = reader[0].ToString() });
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return Json(clients, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult PopulateCriteriaType(string ClientName)
        {
            List<SelectListItem> criteriaType = new List<SelectListItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("select Distinct CriteriaType from dbo.ADFeedSelectionCriteriaLookup where Delivery = '{0}'", ClientName),conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    criteriaType.Add(new SelectListItem { Text = reader[0].ToString() });
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return Json(criteriaType, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDrugList(string ClientName, string CriteriaType)
        {
            List<DrugDetails> ddList = new List<DrugDetails>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("select Criteria,TermID,ModificationDate,CreationDate from dbo.ADFeedSelectionCriteriaLookup where Delivery = '{0}' and CriteriaType = '{1}' and IsActive = 1", ClientName, CriteriaType), conn);     //To have active list change IsActive = 1;
            try
            {
                conn.Open();
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
                    ddList.Add(dd);
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return Json(ddList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDrugView()
        {
            if (Request.QueryString.Count == 0 && TempData["Client"] == null && TempData["CriteriaType"] == null)
                return Redirect("Dashboard");

            if (TempData["Client"] == null)
            {
                TempData["Client"] = Request.QueryString["clientName"];
            }
            if (TempData["CriteriaType"] == null)
            {
                TempData["CriteriaType"] = Request.QueryString["criteriaType"];
            }
            return View();
        }

        public void DeleteDrug(FormCollection form)
        {
            string Criteria = form["criteria"];
            string CriteriaType = form["criteriaType"];
            string Delivery = form["client"];

            TempData["Client"] = Delivery;
            TempData["CriteriaType"] = CriteriaType;

            bool isNonActiveToDelete = CheckIsAvailableAndDeleted(Delivery, Criteria, CriteriaType);
            if (isNonActiveToDelete)
            {
                Response.Write("<script>window.alert(\'Drug is Present but it is Non-Active\');window.location='DeleteDrugView';</script>");
            }
            else
            {
                int RA = 0;
                string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(string.Format("update [dbo].[ADFeedSelectionCriteriaLookup] set IsActive = 0 , ModificationDate = GETDATE() where Delivery = '{0}' and CriteriaType = '{1}' and Criteria = '{2}'", Delivery, CriteriaType, Criteria), conn);
                try
                {
                    conn.Open();
                    RA = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Response.Write(e.Message);
                }
                finally
                {
                    conn.Close();
                }

                if (RA > 0)
                {
                    Response.Write("<script>window.alert(\'Drug Deleted Successfully...! Set IsActive to Zero\');window.location='DeleteDrugView';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'Drug is Not Available in Database to Delete...!\');window.location='DeleteDrugView';</script>");
                }
            }
        }

        private bool CheckIsAvailableAndDeleted(string delivery, string criteria, string criteriaType)
        {
            int RA = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("select Criteria from ADFeedSelectionCriteriaLookup where Delivery = '{0}' and CriteriaType = '{1}' and Criteria = '{2}' and IsActive = 0", delivery, criteriaType, criteria), conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    RA = 1;
                }

            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
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

        public JsonResult GetAutoCriteria(string criteria, string delivery, string criteriaType)
        {
            List<string> CriteriaList = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("SELECT distinct Criteria FROM dbo.ADFeedSelectionCriteriaLookup WHERE Delivery = '{0}' and  CriteriaType = '{1}' and Criteria LIKE '%{2}%' and IsActive = 1", delivery, criteriaType, criteria), conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CriteriaList.Add(reader[0].ToString());
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return Json(CriteriaList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAutoTHSTerm(string criteria, string delivery, string criteriaType)
        {
            List<string> LeadTermList = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(string.Format("select leadterm from dbo.THSTerm where LeadTerm like '%{0}%' and IsApproved = 'Y'", criteria), conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    LeadTermList.Add(reader[0].ToString());
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
            }
            finally
            {
                conn.Close();
            }
            return Json(LeadTermList, JsonRequestBehavior.AllowGet);
        }
    }
}