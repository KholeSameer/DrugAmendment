using DrugAmmendment.Models;
using DrugAmmendment.Services;
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
        private IDrugAmendmentConnectionService _drugAmendmentConnectionService = null;

        public HomeController()
        {
            _drugAmendmentConnectionService = new DrugAmendmentConnectionService();
        }
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
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;
            try
            {
                if (criteriaType == "BrandName")
                {
                    bool isAvailableNonActive = false;
                    try
                    {
                        isAvailableNonActive = CheckIsAvailableNonActive(delivery, criteriaType, criteriaFromUser);
                    }
                    catch (Exception)
                    {
                        Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
                    }

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
                    bool isValidLeadTerm = false;
                    
                    try
                    {
                        isValidLeadTerm = ValidateLeadTerm(criteriaFromUser);
                    }
                    catch (Exception)
                    {
                        Response.Write("<script>window.alert(\'"+UserFriendlyMessage.getMessage()+"\');window.location='AddDrugView'</script>");
                    }
                    
                    if (isValidLeadTerm)
                    {
                        ArrayList ThesData = new ArrayList();
                        try
                        {
                            ThesData = GetDataFromThesTerm(criteriaFromUser);
                        }
                        catch (Exception)
                        {
                            Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
                        }
                        
                        string criteria = ThesData[0].ToString();
                        int? termID = Convert.ToInt32(ThesData[1]);
                        bool isAvailableNonActive = false;
                        try
                        {
                            isAvailableNonActive = CheckIsAvailableNonActive(delivery, criteriaType, criteria);
                        }
                        catch (Exception)
                        {
                            Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
                        }
                        
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
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'"+ UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView'</script>");
            }
        }
        private void CheckIsAvailableActive(string delivery, string criteriaType, string criteria, int? termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            try
            {
                if (_drugAmendmentConnectionService.CheckIsAvailableActive(delivery, criteriaType, criteria, termID) > 0)
                {
                    Response.Write("<script>window.alert(\'This drug is already Present & Active in the DB...!');window.location='AddDrugView'</script>");
                }
                else
                {
                    AddDrugToDB(delivery, criteriaType, criteria, termID);
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'"+UserFriendlyMessage.getMessage()+"\');window.location='AddDrugView'</script>");
            }
            
            
        }
        private void UpdateToActive(string delivery, string criteriaType, string criteria)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            try
            {
                if (_drugAmendmentConnectionService.UpdateToActive(delivery, criteriaType, criteria) > 0)
                {
                    AuditLogger(delivery, criteriaType, criteria, "Active");
                    Response.Write("<script>window.alert(\'Drug Updated to Active Successfully\');window.location='AddDrugView';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'Drug Not Updated...! May be this drug is not present in DB...!\');window.location='AddDrugView';</script>");
                }
            }
            catch(Exception)
            {
                Response.Write("<script>window.alert(\'"+UserFriendlyMessage.getMessage()+"\');window.location='AddDrugView';</script>");
            }
        }
        private bool CheckIsAvailableNonActive(string delivery, string criteriaType, string criteria)
        {
            bool _flag = false;
            try
            {
                _flag = _drugAmendmentConnectionService.CheckIsAvailableNonActive(delivery,criteriaType,criteria);
            }
            catch (Exception)
            {
                throw;
            }
            return _flag;
        }
        private bool ValidateLeadTerm(string criteria)
        {
            bool _flag = false ;
            try
            {
                _flag = _drugAmendmentConnectionService.ValidateLeadTerm(criteria);

            }
            catch (Exception)
            {
                throw;
            }
            return _flag;
        }
        private ArrayList GetDataFromThesTerm(string criteria)
        {
            ArrayList data = new ArrayList();
            try
            {
                data  = _drugAmendmentConnectionService.GetDataFromThesTerm(criteria);
            }
            catch (Exception)
            {
                throw;
            }
            return data;
        }
        private void AddDrugToDB(string delivery, string criteriaType, string criteria, int? termID)
        {
            TempData["Client"] = delivery;
            TempData["CriteriaType"] = criteriaType;

            try
            {
                if (_drugAmendmentConnectionService.AddDrugToDB(delivery, criteriaType, criteria, termID) > 0)
                {
                    AuditLogger(delivery, criteriaType, criteria, "Add");
                    Response.Write("<script>window.alert(\'Drug Added Successfully\');window.location='AddDrugView';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'Drug is not added.Something went wrong.\');window.location='AddDrugView';</script>");
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView';</script>");
            }           
            
        }
        private void AuditLogger(string Delivery, string CriteriaType, string Criteria, string ActionType)
        {
            TempData["Client"] = Delivery;
            TempData["CriteriaType"] = CriteriaType;

            try
            {
                _drugAmendmentConnectionService.AuditLogger(Delivery,CriteriaType,Criteria,ActionType);

            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='AddDrugView';</script>");
            }
        }

        [HttpGet]
        public JsonResult PopulateClients()
        {
            List<SelectListItem> _clients = new List<SelectListItem>();
            try
            {
                _clients = _drugAmendmentConnectionService.PopulateClients();
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_clients, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult PopulateCriteriaType(string ClientName)
        {
            List<SelectListItem> _criteriaType = new List<SelectListItem>();
            try
            {
                _criteriaType = _drugAmendmentConnectionService.PopulateCriteriaType(ClientName);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_criteriaType, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveDrugList(string ClientName, string CriteriaType)
        {
            List<ExportToExcel> _ddList = new List<ExportToExcel>();
            try
            {
                _ddList = _drugAmendmentConnectionService.GetActiveDrugList(ClientName,CriteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_ddList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDrugList(string ClientName, string CriteriaType)
        {
            List<DrugDetails> _ddList = new List<DrugDetails>();
            try
            {
                _ddList = _drugAmendmentConnectionService.GetDrugList(ClientName,CriteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='Dashboard';</script>");
            }
            return Json(_ddList, JsonRequestBehavior.AllowGet);
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
            bool isAvailableNonActive = false;

            try
            {
                isAvailableNonActive = CheckIsAvailableNonActive(Delivery, CriteriaType, Criteria);
                if (isAvailableNonActive)
                {
                    Response.Write("<script>window.alert(\'Drug is Present but it is Non-Active\');window.location='DeleteDrugView';</script>");
                }
                else
                {
                    DeleteDrugFromDB(Delivery, CriteriaType, Criteria);
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }
            
        }

        private void DeleteDrugFromDB(string Delivery, string CriteriaType, string Criteria)
        {
            try
            {
                if (_drugAmendmentConnectionService.DeleteDrugFromDB(Delivery,CriteriaType,Criteria) > 0)
                {
                    AuditLogger(Delivery, CriteriaType, Criteria, "NonActive");
                    Response.Write("<script>window.alert(\'Drug Deleted Successfully...! Set IsActive to Zero\');window.location='DeleteDrugView';</script>");
                }
                else
                {
                    Response.Write("<script>window.alert(\'Drug is Not Available in Database to Delete...!\');window.location='DeleteDrugView';</script>");
                }
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }
            
        }


        public JsonResult GetAutoCriteria(string criteria, string delivery, string criteriaType)
        {
            List<string> _criteriaList = new List<string>();
            try
            {
                _criteriaList = _drugAmendmentConnectionService.GetAutoCriteria(criteria,delivery,criteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }
            return Json(_criteriaList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAutoTHSTerm(string criteria, string delivery, string criteriaType)
        {
            criteria = criteria.TrimEnd();
            List<string> _leadTermList = new List<string>();
            try
            {
                _leadTermList = _drugAmendmentConnectionService.GetAutoTHSTerm(criteria,delivery,criteriaType);
            }
            catch (Exception)
            {
                Response.Write("<script>window.alert(\'" + UserFriendlyMessage.getMessage() + "\');window.location='DeleteDrugView'</script>");
            }
            return Json(_leadTermList, JsonRequestBehavior.AllowGet);
        }
    }
}