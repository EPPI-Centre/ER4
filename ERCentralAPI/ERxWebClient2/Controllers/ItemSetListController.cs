using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ItemSetListController : CSLAController
    {

        private readonly ILogger _logger;

        public ItemSetListController(ILogger<ItemSetListController> logger)
        {
            _logger = logger;
        }

       

        [HttpPost("[action]")]
        public IActionResult Fetch([FromBody] SingleInt64Criteria ItemIDCrit)
        {
            try
            {

                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
                SingleCriteria<ItemSetList, Int64> criteria = new SingleCriteria<ItemSetList, Int64>(ItemIDCrit.Value);
                ItemSetList result = dp.Fetch(criteria);
                foreach (ItemSet iSet in result)
                {
                    foreach(ReadOnlyItemAttribute roia in iSet.ItemAttributesList)
                    {
                        roia.ItemAttributeFullTextDetails.Sort();
                    }
                }
                //ItemSetList result = dp.Fetch(ItemIDCrit.Value);
                return Ok(result);

            }
            catch(Exception e)
            {               
                _logger.LogError(e, "Error when fetching a set list: {0}", ItemIDCrit.Value);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("[action]")]
        public IActionResult ExcecuteItemAttributeSaveCommand([FromBody] MVCItemAttributeSaveCommand MVCcmd)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ItemAttributeSaveCommand cmd = new ItemAttributeSaveCommand(
                        MVCcmd.saveType
                        , MVCcmd.itemAttributeId
                        , MVCcmd.itemSetId
                        , MVCcmd.additionalText
                        , MVCcmd.attributeId
                        , MVCcmd.setId
                        , MVCcmd.itemId
                        , MVCcmd.itemArmId
                        , MVCcmd.revInfo.ToCSLAReviewInfo()
                        //,rinf
                        );
                    DataPortal<ItemAttributeSaveCommand> dp = new DataPortal<ItemAttributeSaveCommand>();
                    cmd = dp.Execute(cmd);
                    MVCcmd.additionalText = cmd.AdditionalText;
                    MVCcmd.attributeId = cmd.AttributeId;
                    MVCcmd.itemArmId = cmd.ItemArmId;
                    MVCcmd.itemAttributeId = cmd.ItemAttributeId;
                    MVCcmd.itemId = cmd.ItemId;
                    MVCcmd.itemSetId = cmd.ItemSetId;
                    MVCcmd.setId = cmd.SetId;
                    return Ok(MVCcmd);
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcmd);
                _logger.LogError(e, "Dataportal Error with Item Attributes: {0}", json);
                throw;
            }
        }
        [HttpPost("[action]")]
        public IActionResult ExecuteItemAttributeBulkInsertCommand([FromBody] MVCItemAttributeBulkSaveCommand MVCcmd)
        {//method is "..BulkInsert.." rather than "BulkSave" 'cause we NEVER use the CSLA object (ItemAttributeBulkSaveCommand) to delete (code in there wouldn't work!).
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ItemAttributeBulkSaveCommand cmd = new ItemAttributeBulkSaveCommand(
                        "Insert"
                        , MVCcmd.attributeId
                        , MVCcmd.setId
                        , MVCcmd.itemIds.Trim(',')
                        , MVCcmd.searchIds.Trim(',')
                        );
                    DataPortal<ItemAttributeBulkSaveCommand> dp = new DataPortal<ItemAttributeBulkSaveCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(MVCcmd);//command is mute, doesn't tell us anything
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcmd);
                _logger.LogError(e, "Dataportal Error with Item Attributes: {0}", json);
                throw;
            }
        }
        [HttpPost("[action]")]
        public IActionResult ExecuteItemAttributeBulkDeleteCommand([FromBody] MVCItemAttributeBulkSaveCommand MVCcmd)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ItemAttributeBulkDeleteCommand cmd = new ItemAttributeBulkDeleteCommand(
                        MVCcmd.attributeId
                        , MVCcmd.itemIds.Trim(',')
                        , MVCcmd.setId
                        , MVCcmd.searchIds.Trim(',')
                        );
                    DataPortal<ItemAttributeBulkDeleteCommand> dp = new DataPortal<ItemAttributeBulkDeleteCommand>();
                    cmd = dp.Execute(cmd);
                    return Ok(MVCcmd);//command is mute, doesn't tell us anything
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcmd);
                _logger.LogError(e, "Dataportal Error with Item Attributes: {0}", json);
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult FetchPDFCoding([FromBody] MVCiaPDFListSelCrit MVCsel)
        {
            try
            {
                SetCSLAUser();
                DataPortal<ItemAttributePDFList> dp = new DataPortal<ItemAttributePDFList>();
                ItemAttributePDFList result = dp.Fetch(new iaPDFListSelCrit(MVCsel.itemDocumentId, MVCsel.itemAttributeId));
                return Ok(result);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when fetching PDFCoding: docID ={0} iaID ={1]", MVCsel.itemDocumentId, MVCsel.itemAttributeId);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public IActionResult CreatePDFCodingPage([FromBody] MVCiaPDFCodingPage MVCcodingInPage)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ItemAttributePDF ToSave = ItemAttributePDF.GetNewItemAttributePDF(MVCcodingInPage.Page);
                    ToSave.ItemAttributeId = MVCcodingInPage.ItemAttributeId;
                    ToSave.ItemDocumentId = MVCcodingInPage.ItemDocumentId;
                    ToSave.ShapeTxt = MVCcodingInPage.ShapeTxt;
                    ToSave.PdfTronXml = MVCcodingInPage.PdfTronXml;
                    ToSave.inPageSelections = new Csla.Core.MobileList<InPageSelections>();
                    foreach (InPageSelections inpsel in MVCcodingInPage.InPageSelections)
                    {
                        ToSave.inPageSelections.Add(inpsel);
                    }
                    DataPortal<ItemAttributePDF> dp2 = new DataPortal<ItemAttributePDF>();
                    ToSave = dp2.Update(ToSave);
                    return Ok(ToSave);
                    
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcodingInPage);
                _logger.LogError(e, "Dataportal Error with CreatePDFCodingPage: {0}", json);
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult UpdatePDFCodingPage([FromBody] MVCiaPDFCodingPage MVCcodingInPage)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<ItemAttributePDF> dp = new DataPortal<ItemAttributePDF>();
                    ItemAttributePDF ToSave = dp.Fetch(new ItemAttributePDFSingleCriteria(MVCcodingInPage.ItemAttributePDFId));
                    if (ToSave != null)
                    {//all is well: we can change what we need.
                        ToSave.ShapeTxt = MVCcodingInPage.ShapeTxt;
                        ToSave.PdfTronXml = MVCcodingInPage.PdfTronXml;
                        //CslaInpsels is used to rebuild the "inpage selections" with some care...
                        Csla.Core.MobileList<InPageSelections> CslaInpsels = new Csla.Core.MobileList<InPageSelections>();
                        foreach (InPageSelections inpsel in MVCcodingInPage.InPageSelections)
                        {
                            CslaInpsels.Add(inpsel);
                        }
                        //previous attemp: do reconstuctions on server side...
                        //ToSave.inPageSelections.Clear();
                        //List<InPageSelections> ToKeepAside = new List<InPageSelections>();
                        //foreach (InPageSelections inpsel in MVCcodingInPage.InPageSelections)
                        //{//we'll try to match selections, and keep the indexes in place for those that weren't touched...
                        //    List<InPageSelections> ToKeep = ToSave.inPageSelections.FindAll(found => found.SelTxt == inpsel);
                        //    if (ToKeep.Count == 1)
                        //    {//keep unchanged.
                        //        CslaInpsels.Add(ToKeep[0]);
                        //    }
                        //    else if (ToKeep.Count == 0 && inpsel.Length > 15)//safety first: we try to re-identify the selection, but only if it's reasonably long
                        //    {
                        //        //try again, stripping spaces
                        //        ToKeep = ToSave.inPageSelections.FindAll(found => found.SelTxt.Replace(" ", "") == inpsel.Replace(" ", ""));
                        //        if (ToKeep.Count == 1)
                        //        {//keep unchanged.
                        //            CslaInpsels.Add(ToKeep[0]);
                        //        }
                        //        else if (ToKeep.Count == 0)
                        //        {//this is new, we can't find it...
                        //            CslaInpsels.Add(new InPageSelections(0, 0, inpsel));
                        //        }
                        //        else if (ToKeep.Count > 1)
                        //        {//remove one of the matches (should not match again!).
                        //            CslaInpsels.Add(ToKeep[0]);
                        //            ToSave.inPageSelections.Remove(ToKeep[0]);
                        //        }
                        //    }
                        //    else if (ToKeep.Count == 0)
                        //    {//this is new, we can't find it...
                        //        CslaInpsels.Add(new InPageSelections(0, 0, inpsel));
                        //    }
                        //    else if (ToKeep.Count > 1)
                        //    {//remove one of the matches (should not match again!).
                        //        CslaInpsels.Add(ToKeep[0]);
                        //        ToSave.inPageSelections.Remove(ToKeep[0]);
                        //    }
                        //}
                        ToSave.inPageSelections = CslaInpsels;
                        DataPortal<ItemAttributePDF> dp2 = new DataPortal<ItemAttributePDF>();
                        ToSave = dp2.Update(ToSave);
                        return Ok(ToSave);
                    }
                    else return NotFound();
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(MVCcodingInPage);
                _logger.LogError(e, "Dataportal Error with UpdatePDFCodingPage: {0}", json);
                throw;
            }
        }

        [HttpPost("[action]")]
        public IActionResult DeletePDFCodingPage([FromBody] SingleInt64Criteria ItemAttributePDFId)
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    DataPortal<ItemAttributePDF> dp = new DataPortal<ItemAttributePDF>();
                    ItemAttributePDF ToDelete = dp.Fetch(new ItemAttributePDFSingleCriteria(ItemAttributePDFId.Value));
                    
                    if (ToDelete != null)
                    {//all is well: we can change what we need.
                        ToDelete.Delete();
                        ToDelete = ToDelete.Save();
                        return Ok(ItemAttributePDFId.Value);//to retrun the ID so that it can be removed from client.
                    }
                    else return NotFound();
                }
                else return Forbid();

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(ItemAttributePDFId.Value);
                _logger.LogError(e, "Dataportal Error with DeletePDFCodingPage: {0}", ItemAttributePDFId.Value);
                throw;
            }
        }
    }
    

    public class MVCItemAttributeSaveCommand
    {
        public string saveType { get; set; }
        public Int64 itemAttributeId { get; set; }
        public Int64 itemSetId { get; set; }
        public string additionalText { get; set; }
        public Int64 attributeId { get; set; }
        public int setId { get; set; }
        public Int64 itemId { get; set; }
        public Int64 itemArmId { get; set; }
        public MVCReviewInfo revInfo { get; set; }
    }
    public class MVCReviewInfo
    {
        public int reviewId { get; set; }
        public string reviewName { get; set; }
        public bool showScreening { get; set; }
        public int screeningCodeSetId { get; set; }
        public string screeningMode { get; set; }
        public string screeningReconcilliation { get; set; }
        public Int64 screeningWhatAttributeId { get; set; }
        public int screeningNPeople { get; set; }
        public bool screeningAutoExclude { get; set; }
        public bool screeningModelRunning { get; set; }
        public bool screeningIndexed { get; set; }
        public bool screeningListIsGood { get; set; }
        public string bL_ACCOUNT_CODE { get; set; }
        public string bL_AUTH_CODE { get; set; }
        public string bL_TX { get; set; }
        public string bL_CC_ACCOUNT_CODE { get; set; }
        public string bL_CC_AUTH_CODE { get; set; }
        public string bL_CC_TX { get; set; }
        public ReviewInfo ToCSLAReviewInfo()
        {
            ReviewInfo result = new ReviewInfo();
            result.BL_ACCOUNT_CODE = this.bL_ACCOUNT_CODE;
            result.BL_AUTH_CODE = this.bL_AUTH_CODE;
            result.BL_CC_ACCOUNT_CODE = this.bL_CC_ACCOUNT_CODE;
            result.BL_CC_AUTH_CODE = this.bL_CC_AUTH_CODE;
            result.BL_CC_TX = this.bL_CC_TX;
            result.BL_TX = this.bL_TX;
            result.ReviewId = this.reviewId;
            result.ReviewName = this.reviewName;
            result.ScreeningAutoExclude = this.screeningAutoExclude;
            result.ScreeningCodeSetId = this.screeningCodeSetId;
            result.ScreeningIndexed = this.screeningIndexed;
            result.ScreeningListIsGood = this.screeningListIsGood;
            result.ScreeningMode = this.screeningMode;
            result.ScreeningModelRunning = this.screeningModelRunning;
            result.ScreeningNPeople = this.screeningNPeople;
            result.ScreeningReconcilliation = this.screeningReconcilliation;
            result.ScreeningWhatAttributeId = this.screeningWhatAttributeId;
            result.ShowScreening = this.showScreening;
            return result;
        }
    }
    public class MVCItemAttributeBulkSaveCommand
    {
        public long attributeId;
        public int setId;
        public string itemIds;
        public string searchIds;
    }
    public class MVCiaPDFListSelCrit
    {
        public Int64 itemDocumentId;
        public Int64 itemAttributeId;
    }
    public class MVCiaPDFCodingPage
    {
        public Int64 ItemAttributePDFId;
        public Int64 ItemDocumentId;
        public Int64 ItemAttributeId;
        public string ShapeTxt;
        public InPageSelections[] InPageSelections;
        public int Page;
        public string PdfTronXml;
    }
}
