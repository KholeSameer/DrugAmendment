﻿using DrugAmmendment.Models;
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
        public ActionResult Dashboard()
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
            string criteriaFromUser = form["criteria"].Trim();
            string delivery = form["client"];
            string criteriaType = form["criteriaType"];

            if (criteriaType == "BrandName")
            {
                bool isAvailableNonActive = CheckIsAvailableNonActive(delivery,criteriaType, criteriaFromUser);
                if (isAvailableNonActive)
                {
                    UpdateToActive(delivery, criteriaType, criteriaFromUser);
                }
                else
                {
                    CheckIsAvailableActive(delivery, criteriaType, criteriaFromUser, null);
                }

            }
            else
            {
                bool isValidLeadTerm = ValidateLeadTerm(criteriaFromUser);
                if (isValidLeadTerm)
                {
                    ArrayList ThesData = GetDataFromThesTerm(criteriaFromUser);
                    string criteria = ThesData[0].ToString();
                    int? termID = Convert.ToInt32(ThesData[1]);

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
        }

        private void CheckIsAvailableActive(string delivery, string criteriaType, string criteria, int? termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            int rowsAffected = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand($"select Criteria FROM [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{delivery}' and CriteriaType = '{criteriaType}' and Criteria = '{criteria}' and IsActive = 1 ", conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rowsAffected = 1;
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

            if (rowsAffected > 0)
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
            int rowsAffected = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            
            SqlCommand cmd = new SqlCommand($"update [dbo].[ADFeedSelectionCriteriaLookup] set IsActive = 1, ModificationDate = GETDATE() where Delivery = '{delivery}' and CriteriaType = '{criteriaType}' and Criteria = '{criteria}'", conn);
            try
            {
                conn.Open();
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Response.Write("<script>window.alert(\'You are not allowed to do changes in Database.\');window.location='AddDrugView';</script>");
            }
            catch (Exception e)
            {
                Response.Write("<script>window.alert(\'Other Exception Occurred.\');window.location='AddDrugView';</script>");
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0 )
            {
                AuditLogger(delivery, criteriaType, criteria, "Active");
                Response.Write("<script>window.alert(\'Drug Updated to Active Successfully\');window.location='AddDrugView';</script>");
            }
            else
            {
                Response.Write("<script>window.alert(\'Drug Not Updated...! May be this drug is not present in DB...!\');window.location='AddDrugView';</script>");
            }
            
        }

        private bool CheckIsAvailableNonActive(string delivery, string criteriaType, string criteria)
        {
            bool flag = false;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            
            SqlCommand cmd = new SqlCommand($"select Criteria FROM [dbo].[ADFeedSelectionCriteriaLookup] where Criteria  = '{criteria}' and CriteriaType = '{criteriaType}' and Delivery = '{delivery}' and IsActive = 0", conn);
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
            SqlCommand cmd = new SqlCommand($"select LeadTerm FROM [dbo].[THSTerm] where LeadTerm  like '{criteria}'", conn);
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
            SqlCommand cmd = new SqlCommand($"select LeadTerm, TermID from [dbo].[THSTerm] where LeadTerm = '{criteria}'", conn);
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

        private void AddDrugToDB(string delivery, string criteriaType, string criteria, int? termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;
            SqlCommand cmd = new SqlCommand();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            if (termID == null)
            {
                cmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaLookup] values ('{delivery}','{criteriaType}','{criteria}',null,{1},GETDATE(),GETDATE())", conn);
            }
            else { 
                cmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaLookup] values ('{delivery}','{criteriaType}','{criteria}',{termID},{1},GETDATE(),GETDATE())", conn);
            }
            int rowsAffected = 0;
            try
            {
                conn.Open();
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Response.Write("<script>window.alert(\'You are not allowed to do changes in Database.\');window.location='AddDrugView';</script>");
            }
            catch (Exception e)
            {
                Response.Write("<script>window.alert(\'Drug is not added. Something went wrong. Exception Occurred.\');window.location='Dashboard';</script>");
            }
            finally {
                conn.Close();
            }
                         
            if (rowsAffected > 0)
            {
                AuditLogger(delivery, criteriaType, criteria, "Add");
                Response.Write("<script>window.alert(\'Drug Added Successfully\');window.location='AddDrugView';</script>");
            }
            else
            {
                Response.Write("<script>window.alert(\'Drug is not added.Something went wrong.\');window.location='AddDrugView';</script>");
            }
        }


        private void AuditLogger(string Delivery, string CriteriaType, string Criteria, string ActionType)
        {
            int? TermID = 0;
            TermID = GetTermID(Delivery,CriteriaType,Criteria);
            int rowsAffected = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand logCmd = null;
            if (TermID == 0)
            {
                 logCmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaHistory] values('{Delivery}','{CriteriaType}','{Criteria}',null,'{ActionType}',GETDATE(),'{Environment.UserName}')", conn);
            }
            else
            {
                logCmd = new SqlCommand($"insert into [dbo].[ADFeedSelectionCriteriaHistory] values('{Delivery}','{CriteriaType}','{Criteria}',{TermID},'{ActionType}',GETDATE(),'{Environment.UserName}')", conn);
            }
            
            try
            {
                conn.Open();
                rowsAffected = logCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Response.Write("<script>window.alert(\'Something went wrong while logging the events.\');window.location='AddDrugView';</script>");
            }
            finally
            {
                conn.Close();
            }
        }

        private int? GetTermID(string delivery, string criteriaType, string criteria)
        {
            int? TermID = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand($"select TermID from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{delivery}' and CriteriaType = '{criteriaType}' and Criteria = '{criteria}'", conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader[0].Equals(System.DBNull.Value))
                    {
                        TermID = 0;
                    }
                    else
                    {
                        TermID = Convert.ToInt32(reader[0]);
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
            return TermID;
        }

        [HttpGet]
        public JsonResult PopulateClients()
        {
            List<SelectListItem> clients = new List<SelectListItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("select Distinct Delivery from [dbo].[ADFeedSelectionCriteriaLookup]", conn);
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
                Response.Write("<script>window.alert(\'Something went wrong while fetching the Customer List\');window.location='DeleteDrugView';</script>");
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
            SqlCommand cmd = new SqlCommand($"select Distinct CriteriaType from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{ClientName}'",conn);
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
                Response.Write("<script>window.alert(\'Something went wrong while fetching the Criteria Type List\');window.location='DeleteDrugView';</script>");
            }
            finally
            {
                conn.Close();
            }
            return Json(criteriaType, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveDrugList(string ClientName, string CriteriaType)
        {
            List<ExportToExcel> ddList = new List<ExportToExcel>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand($"select * from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{ClientName}' and CriteriaType = '{CriteriaType}' and IsActive = 1", conn);     //To have active list change IsActive = 1;
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ExportToExcel dd = new ExportToExcel();
                    dd.Delivery = reader[0].ToString();
                    dd.CriteriaType = reader[1].ToString();
                    dd.Criteria = reader[2].ToString();
                    if (reader[3].Equals(System.DBNull.Value))
                    {
                        dd.TermID = null;
                    }
                    else
                    {
                        dd.TermID = Convert.ToInt32(reader[3]);
                    }
                    if (reader[5].Equals(System.DBNull.Value))
                    {
                        dd.ModificationDate = "Null";
                    }
                    else
                    {
                        dd.ModificationDate = reader[5].ToString();
                    }
                    if (reader[6].Equals(System.DBNull.Value))
                    {
                        dd.CreationDate = "Null";
                    }
                    else
                    {
                        dd.CreationDate = reader[6].ToString();
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

        public JsonResult GetDrugList(string ClientName, string CriteriaType)
        {
            List<DrugDetails> ddList = new List<DrugDetails>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand($"select * from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{ClientName}' and CriteriaType = '{CriteriaType}'", conn);     //To have active list change IsActive = 1;
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DrugDetails dd = new DrugDetails();
                    dd.Delivery = reader[0].ToString();
                    dd.CriteriaType = reader[1].ToString();
                    dd.Criteria = reader[2].ToString();
                    dd.IsActive = Convert.ToInt32(reader[4]);
                    if (reader[3].Equals(System.DBNull.Value))
                    {
                        dd.TermID = null;
                    }
                    else
                    {
                        dd.TermID = Convert.ToInt32(reader[3]);
                    }
                    if (reader[5].Equals(System.DBNull.Value))
                    {
                        dd.ModificationDate = null;
                    }
                    else
                    {
                        dd.ModificationDate = Convert.ToDateTime(reader[5]);
                    }
                    if (reader[6].Equals(System.DBNull.Value))
                    {
                        dd.CreationDate = null;
                    }
                    else
                    {
                        dd.CreationDate = Convert.ToDateTime(reader[6]);
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
            string Criteria = form["criteria"].Trim();
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
                int rowsAffected = 0;
                string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand($"update [dbo].[ADFeedSelectionCriteriaLookup] set IsActive = 0 , ModificationDate = GETDATE() where Delivery = '{Delivery}' and CriteriaType = '{CriteriaType}' and Criteria = '{Criteria}'", conn);
                try
                {
                    conn.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    Response.Write("<script>window.alert(\'You are not allowed to make changes in Database.\');window.location='DeleteDrugView';</script>");
                }
                catch (Exception e)
                {
                    Response.Write(e.Message);
                }
                finally
                {
                    conn.Close();
                }

                if (rowsAffected > 0)
                {
                    AuditLogger(Delivery, CriteriaType, Criteria, "NonActive");
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
            int rowsAffected = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand($"select Criteria from [dbo].[ADFeedSelectionCriteriaLookup] where Delivery = '{delivery}' and CriteriaType = '{criteriaType}' and Criteria = '{criteria}' and IsActive = 0", conn);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rowsAffected = 1;
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
            if (rowsAffected > 0)
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
            SqlCommand cmd = new SqlCommand($"SELECT distinct Criteria FROM [dbo].[ADFeedSelectionCriteriaLookup] WHERE Delivery = '{delivery}' and  CriteriaType = '{criteriaType}' and Criteria LIKE '%{criteria}%' and IsActive = 1", conn);
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
            criteria = criteria.TrimEnd();
            List<string> LeadTermList = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand($"select leadterm from [dbo].[THSTerm] where LeadTerm like '%{criteria}%' and IsApproved = 'Y'", conn);
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