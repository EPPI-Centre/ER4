import { Inject, Injectable, Output, EventEmitter, NgZone, Attribute, OnDestroy } from '@angular/core';
import { lastValueFrom, Observable, of, Subscription } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item, ItemListService, Criteria, ItemList, StringKeyValue } from './ItemList.service';
import { ReviewSet, SetAttribute, ReviewSetsService, singleNode, ItemAttributeSaveCommand, iSetType } from './ReviewSets.service';
import { ArmTimepointLinkListService } from './ArmTimepointLinkList.service';
import { ItemDocsService } from './itemdocs.service';
import { Outcome, OutcomeItemList, OutcomeItemAttributesList, OutcomeItemAttribute } from './outcomes.service';
import { EventEmitterService } from './EventEmitter.service';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root',
})

export class ItemCodingService extends BusyAwareService implements OnDestroy {
  constructor(
    private _httpC: HttpClient,
    configService: ConfigService,
    private modalService: ModalService,
    private ArmsService: ArmTimepointLinkListService,
    private ReviewSetsService: ReviewSetsService,
    private ReviewerIdentityService: ReviewerIdentityService,
    private ngZone: NgZone,
    private ItemDocsService: ItemDocsService,
    private EventEmitterService: EventEmitterService,
    private ItemListService: ItemListService
  ) {
    super(configService);
    //console.log("On create DuplicatesService");
    this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
  }

  ngOnDestroy() {
    console.log("Destroy DuplicatesService");
    if (this.clearSub != null) this.clearSub.unsubscribe();
  }
  private clearSub: Subscription | null = null;

  @Output() DataChanged = new EventEmitter();
  @Output() ItemAttPDFCodingChanged = new EventEmitter();//used to build the PDFtron annotations on the fly
  @Output() ToggleLiveComparison = new EventEmitter();
  private _ItemCodingList: ItemSet[] = [];
  //public itemID = new Subject<number>();
  private _CurrentItemAttPDFCoding: ItemAttPDFCoding = new ItemAttPDFCoding();
  private _PerItemReport: boolean = true;
  //if PerItemReport, _stopQuickReport gets flipped from outside (set method below) as "Cancel",
  //otherise it's the variable that control execution of per-page reports and acts as "cancel" from the outside.
  //thus, it needs to be true whenever a report is actually NOT running...
  private _stopQuickReport: boolean = true;
  private _CodingReport: string = "";
  public get CodingReport(): string {
    return this._CodingReport;
  }
  public jsonReport: JsonReport = new JsonReport();
  public get stopQuickReport(): boolean {
    return this._stopQuickReport;
  }
  public set stopQuickReport(val: boolean) {
    this._stopQuickReport = val;
    if (this._stopQuickReport == true) {
      //this request is coming from the outside (or a failure), so we'll clear the reports contents to avoid presenting incomplete data
      console.log("cancelling report...");
      this.jsonReport = new JsonReport();
      this._CodingReport = "";
    }
  }
  public get ItemCodingList(): ItemSet[] {
    //if (this._ItemCodingList.length == 0) {
    //    const ItemSetsJson = localStorage.getItem('ItemCodingList');
    //    let ReadOnlyReviews: ItemSet[] = ItemSetsJson !== null ? JSON.parse(ItemSetsJson) : [];
    //    if (ReadOnlyReviews == undefined || ReadOnlyReviews == null || ReadOnlyReviews.length == 0) {
    //        return this._ItemCodingList;
    //    }
    //    else {
    //        //not sure we should do anything here
    //    }
    //}
    return this._ItemCodingList;
  }

  public set ItemCodingList(icl: ItemSet[]) {
    this._ItemCodingList = icl;
    this.Save();
  }
  public get CurrentItemAttPDFCoding(): ItemAttPDFCoding {
    return this._CurrentItemAttPDFCoding;
  }
  public SelectedSetAttribute: SetAttribute | null = null;//used to fetch PDF coding...

  public Fetch(ItemId: number) {
    this._BusyMethods.push("Fetch");
    //this.itemID.next(ItemId); 
    //console.log('FetchCoding');
    let body = JSON.stringify({ Value: ItemId });
    this._httpC.post<iItemSet[]>(this._baseUrl + 'api/ItemSetList/Fetch',
      body).subscribe(result => {
        this.ItemCodingList = [];
        for (let iSet of result) {
          let NewRealItemSet: ItemSet = new ItemSet(iSet);
          this.ItemCodingList.push(NewRealItemSet);
        }
        this.RemoveBusy("Fetch");
        //console.log("emitting!");
        this.DataChanged.emit();
        //this.ReviewSetsService.AddItemData(result);
        //this.Save();
      }, error => {
        this.RemoveBusy("Fetch");
        if (this.SelfSubscription4QuickCodingReport) {
          this.SelfSubscription4QuickCodingReport.unsubscribe();
          this.SelfSubscription4QuickCodingReport = null;
        }
        this.modalService.SendBackHomeWithError(error);
      }
      );
  }

  public FetchItemAttPDFCoding(criteria: ItemAttPDFCodingCrit) {
    this._BusyMethods.push("FetchItemAttPDFCoding");
    //this.itemID.next(ItemId); 
    //console.log('FetchCoding');
    this._CurrentItemAttPDFCoding = new ItemAttPDFCoding();
    this._httpC.post<ItemAttributePDF[]>(this._baseUrl + 'api/ItemSetList/FetchPDFCoding',
      criteria).subscribe(result => {
        //console.log("FetchItemAttPDFCoding", result);
        this._CurrentItemAttPDFCoding.Criteria = criteria;
        this._CurrentItemAttPDFCoding.ItemAttPDFCoding = result;
        this.ItemAttPDFCodingChanged.emit();
      }, error => {
        this.RemoveBusy("FetchItemAttPDFCoding");
        this.modalService.SendBackHomeWithError(error);
        this.ngZone.run(() => this.IsBusy);
      }
        , () => {
          this.RemoveBusy("FetchItemAttPDFCoding");
          this.ngZone.run(() => this.IsBusy);
        }
      );
  }

  public async StandaloneFetchItemAttPDFCoding(criteria: ItemAttPDFCodingCrit): Promise<ItemAttributePDF[] | boolean> {
    this._BusyMethods.push("StandaloneFetchItemAttPDFCoding");
    this._CurrentItemAttPDFCoding = new ItemAttPDFCoding();
    return lastValueFrom(this._httpC.post<ItemAttributePDF[]>(this._baseUrl + 'api/ItemSetList/FetchPDFCoding',
      criteria)).then(result => {
        //console.log("FetchItemAttPDFCoding", result);
        //this._CurrentItemAttPDFCoding.Criteria = criteria;
        //this._CurrentItemAttPDFCoding.ItemAttPDFCoding = result;
        this.RemoveBusy("StandaloneFetchItemAttPDFCoding");
        this.ngZone.run(() => this.IsBusy);
        return result;
        //this.ItemAttPDFCodingChanged.emit();
      }, error => {
        this.RemoveBusy("StandaloneFetchItemAttPDFCoding");
        this.modalService.GenericError(error);
        this.ngZone.run(() => this.IsBusy);
        return false;
      }).catch(caught => {
        this.RemoveBusy("StandaloneFetchItemAttPDFCoding");
        this.modalService.GenericError(caught);
        this.ngZone.run(() => this.IsBusy);
        return false;
      });
  }

  public SaveItemAttPDFCoding(perPageXML: string, itemAttributeId: number, cmd: ItemAttributeSaveCommand | null = null) {
    this._BusyMethods.push("SaveItemAttPDFCoding");
    //this is big an complex because we use the XML representation of highlights to re-match with exsiting InPageSelections so to keep the ER4 side happy, when possible...

    this.ngZone.run(() => this.IsBusy);//make sure that Angular visual thread notices (necessary because this )
    let parser = new DOMParser();
    let xmlDoc = parser.parseFromString(perPageXML, "text/xml");
    let parsererror = xmlDoc.getElementsByTagName("parsererror")
    if (parsererror && parsererror.length > 0) {
      //ugh, some bad chars in here...
      //perPageXML = perPageXML.replace(/<contents>/g, '<contents><![CDATA[').replace(/<\/contents>/g, ']]><\/contents>').replace('encoding="UTF-8"', 'encoding="UTF-16"')
      //console.log("sanitised: ", perPageXML);
      let done = false;
      let contIndex = 0;
      while (!done) {
        contIndex = perPageXML.indexOf("<contents>", contIndex);
        if (contIndex == -1) {
          done = true;
        }
        else {
          let endIndex = perPageXML.indexOf("</contents>", contIndex);
          let toclean = perPageXML.substr(contIndex + 10, endIndex - contIndex - 10);
          //console.log("toclean: ", toclean, contIndex, endIndex);
          let cleaned = encodeURI(toclean);//replaces chars that are not UTF8 and would prevent parser.parseFromString(perPageXML, "text/xml");
          perPageXML = perPageXML.substr(0, contIndex + 10) + cleaned + perPageXML.substr(endIndex);
          contIndex = endIndex;
        }
      }
      //console.log("sanitised: ", perPageXML);
      xmlDoc = parser.parseFromString(perPageXML, "text/xml");

    }
    let xAnnots = xmlDoc.getElementsByTagName("highlight");
    let defmtx = xmlDoc.getElementsByTagName("defmtx");
    if (!xAnnots || xAnnots.length == 0 || !defmtx || defmtx.length == 0) {
      //console.log("Can't do, missing data:", xAnnots, defmtx, xmlDoc);
      this.RemoveBusy("SaveItemAttPDFCoding");
      this.ngZone.run(() => this.IsBusy);
      return;
    }
    let matrix = defmtx[0].getAttribute("matrix");
    let pageSize = 810;//we need to invert coordinates we're getting. PDFTron is giving us the wrong kind...
    //810 is a random value I've seen on an A4 PDF...
    if (matrix) {
      let mmm = matrix.split(",");
      if (mmm.length > 0) {
        pageSize = (+mmm[mmm.length - 1]) / 0.75;
      }
    }
    let iaPDF: MVCiaPDFCodingPage = new MVCiaPDFCodingPage();
    let pageNst = xAnnots[0].getAttribute("page");
    let pageN = -1;
    if (pageNst) pageN = +pageNst;
    if (pageN !== null && pageN > -1) {
      pageN++;
    }
    else pageN = -1;
    iaPDF.Page = pageN;
    let existing: ItemAttributePDF | undefined = undefined;
    iaPDF.PdfTronXml = perPageXML;
    iaPDF.ItemDocumentId = this.CurrentItemAttPDFCoding.Criteria.itemDocumentId;
    if (this.CurrentItemAttPDFCoding.Criteria.itemAttributeId == itemAttributeId && this.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
      //good, we have this and should check if we need to change the object for a given page...
      existing = this.CurrentItemAttPDFCoding.ItemAttPDFCoding.find(found => found.page == pageN);
      //if (exsting) {
      //    //iaPDF.itemAttributeId = exsting.itemAttributeId;
      //    iaPDF.ItemAttributePDFId = exsting.itemAttributePDFId;
      //    iaPDF.ItemDocumentId = exsting.itemDocumentId;
      //    //iaPDF.Page = exsting.page;
      //    //rootdone = true;
      //}
    }
    iaPDF.ItemAttributeId = itemAttributeId;
    iaPDF.ItemDocumentId = this.CurrentItemAttPDFCoding.Criteria.itemDocumentId;
    iaPDF.ShapeTxt = "F1";
    let WorkingInPageSel: InPageSelection[] = [];//we use this to handle exsisting inpage sels, without touching the original array.
    if (existing) {
      WorkingInPageSel = existing.inPageSelections.slice(0);
      iaPDF.ItemAttributePDFId = existing.itemAttributePDFId;
    }
    let color: string | null = "";
    if (xAnnots.length > 0) {
      color = xAnnots[0].getAttribute("color");
    }
    //console.log("color, existing: ", color, existing);
    if (color == "#FF7C06") {
      //this is the special case where we can't match the shapes with the text, we'll add the XML for speed, but leave the rest untouched.
      if (existing !== undefined) {
        //we have the data to save...
        for (let inPSel of existing.inPageSelections) {
          let newinPSel = {
            Start: inPSel.start,
            End: inPSel.end,
            SelTxt: inPSel.selTxt
          }
          iaPDF.InPageSelections.push(newinPSel);
        }
        //console.log("special case add XML only: ", iaPDF);
        iaPDF.ShapeTxt = existing.shapeTxt;
      }
    }
    else {
      for (let i = 0; i < xAnnots.length; i++) {
        //build shapes and inpageselections, trying to keep those we can retain (to keep char offsets as in ER4)
        let xAnn = xAnnots[i];
        let tronShapeSt = xAnn.getAttribute("coords");
        if (!tronShapeSt) tronShapeSt = "";
        let coords = tronShapeSt.split(',');


        while (coords.length != 0) {
          let removed = coords.splice(0, 8);
          //order of these is all mixed up because the XML uses the "wrong" kind of coordinates
          iaPDF.ShapeTxt += "M" + (+removed[0] / 0.75).toFixed(3) + ",";
          iaPDF.ShapeTxt += (pageSize - (+removed[5] / 0.75)).toFixed(3) + "L";
          iaPDF.ShapeTxt += (+removed[2] / 0.75).toFixed(3) + ",";
          iaPDF.ShapeTxt += (pageSize - (+removed[7] / 0.75)).toFixed(3) + "L";
          iaPDF.ShapeTxt += (+removed[6] / 0.75).toFixed(3) + ",";
          iaPDF.ShapeTxt += (pageSize - (+removed[1] / 0.75)).toFixed(3) + "L";
          iaPDF.ShapeTxt += (+removed[4] / 0.75).toFixed(3) + ",";
          iaPDF.ShapeTxt += (pageSize - (+removed[3] / 0.75)).toFixed(3) + "z";
        }

        let content = xAnn.getElementsByTagName("contents");
        if (content && content.length > 0
          && content[0].childNodes
          && content[0].childNodes.length > 0
          && content[0].childNodes[0].nodeValue
        ) {
          let selt = content[0].childNodes[0].nodeValue;
          if (existing !== undefined && selt) {
            //we'll try to find selections, keep them if they exist.
            let exsInPageSels = WorkingInPageSel.filter(found => found.selTxt == selt);
            if (exsInPageSels.length == 0 && WorkingInPageSel.length > 0 && selt.length > 15) {
              //we'll try again, removing spaces
              exsInPageSels = existing.inPageSelections.filter(found => selt && found.selTxt.replace(/\s/g, '') == selt.replace(/\s/g, ''));
            }
            if (exsInPageSels.length >= 1) {
              //we have 1 or more selections with the same text (go figure), we need to remove one from WorkingInPageSel so not to match it again
              let newSel: CSLAInPageSelection = { End: exsInPageSels[0].end, Start: exsInPageSels[0].start, SelTxt: exsInPageSels[0].selTxt };
              iaPDF.InPageSelections.push(newSel);
              WorkingInPageSel.splice(WorkingInPageSel.indexOf(exsInPageSels[0]), 1);
            }
            else { //exsInPageSels.length == 0
              let newSel: CSLAInPageSelection = { End: 0, Start: 0, SelTxt: selt };
              iaPDF.InPageSelections.push(newSel);
            }
          }
          else if (selt) {
            //this is fresh coding for current page, we can't try to recycle anything.
            let newSel: CSLAInPageSelection = { End: 0, Start: 0, SelTxt: selt };
            iaPDF.InPageSelections.push(newSel);
          }
          else {//uh? can't do much...
            console.log("One annotation didn't contain any text!", content[0].childNodes[0]);
          }

        }
      }
    }
    if (cmd && cmd.attributeId != 0 && cmd.saveType == "Insert") {
      iaPDF.CreateInfo = cmd;
      iaPDF.ItemDocumentId = this.ItemDocsService.CurrentDocId;
    }
    let endpoint = iaPDF.ItemAttributePDFId === 0 ? "api/ItemSetList/CreatePDFCodingPage" : "api/ItemSetList/UpdatePDFCodingPage";
    this._httpC.post<iCreatePDFCodingPageResult>(this._baseUrl + endpoint,
      iaPDF).subscribe(result => {
        //console.log("SaveItemAttPDFCoding // " + endpoint, result);
        if (this._CurrentItemAttPDFCoding.ItemAttPDFCoding == null) {
          this._CurrentItemAttPDFCoding.ItemAttPDFCoding = [];
        }
        let indexOfRes = this._CurrentItemAttPDFCoding.ItemAttPDFCoding.findIndex((found: ItemAttributePDF) => result.iaPDFpage.itemAttributePDFId == found.itemAttributePDFId)
        if (indexOfRes == -1) this._CurrentItemAttPDFCoding.ItemAttPDFCoding.push(result.iaPDFpage);//add new page
        else this._CurrentItemAttPDFCoding.ItemAttPDFCoding.splice(indexOfRes, 1, result.iaPDFpage);//replace existing - maybe we don't need to...
        if (iaPDF.CreateInfo && result.createInfo) {
          //console.log("Create new ItemAtt, maybe Item set:", result.createInfo);
          //we also needed to create ItemAttribute and maybe ItemSet... Lots to do...
          let ItemSet = this.FindItemSetBySetId(result.createInfo.setId);
          this.ApplyInsertOrUpdateItemAttribute(result.createInfo, ItemSet);//most happens here.
          //see: https://github.com/angular/angular/issues/25837#issuecomment-434049467
          //the ngZone thing makes sure Angular updates the visual structure.
          this.ngZone.run(
            () => {
              const check = this.IsBusy;
              this.ReviewSetsService.AddItemData(this.ItemCodingList, this.ArmsService.SelectedArm == null ? 0 : this.ArmsService.SelectedArm.itemArmId);
            }
          );


          this.FetchItemAttPDFCoding(new ItemAttPDFCodingCrit(iaPDF.ItemDocumentId, result.createInfo.itemAttributeId));
        }
      }, error => {
        this.RemoveBusy("SaveItemAttPDFCoding");

        this.modalService.SendBackHomeWithError(error);
      }
        , () => {
          this.RemoveBusy("SaveItemAttPDFCoding");
          this.ngZone.run(() => this.IsBusy);
        }
      );
  }
  //part of a small "normalise code" (avoid replication) quick win: called by coding page, coding full and PDFtroncontainer.
  public ApplyInsertOrUpdateItemAttribute(cmdResult: ItemAttributeSaveCommand, itemSet: ItemSet | null = null) {
    //console.log("ApplyInsertOrUpdateItemAttribute CmdResult", cmdResult);

    //console.log("itemSet", itemSet);
    let newItemA: ReadOnlyItemAttribute = new ReadOnlyItemAttribute();
    if (itemSet) {
      const index = (itemSet.itemAttributesList.findIndex(found => found.itemAttributeId == cmdResult.itemAttributeId))
      if (index != -1) {
        newItemA = itemSet.itemAttributesList.splice(index, 1)[0];
      }
    }
    newItemA.additionalText = cmdResult.additionalText;
    newItemA.armId = cmdResult.itemArmId;
    newItemA.armTitle = "";
    newItemA.attributeId = cmdResult.attributeId;
    newItemA.itemAttributeId = cmdResult.itemAttributeId;
    if (itemSet) {
      itemSet.itemAttributesList.push(newItemA);
    }
    else {//didn't have the itemSet, so need to create it...
      let newItemSet: ItemSet = new ItemSet();
      newItemSet.contactId = this.ReviewerIdentityService.reviewerIdentity.userId;
      newItemSet.contactName = this.ReviewerIdentityService.reviewerIdentity.name;
      let setDest = this.ReviewSetsService.FindSetById(cmdResult.setId);
      if (setDest) {
        newItemSet.isCompleted = setDest.codingIsFinal;
        newItemSet.setName = setDest.set_name;
      }
      newItemSet.isLocked = false;
      newItemSet.itemId = cmdResult.itemId;
      newItemSet.itemSetId = cmdResult.itemSetId;
      newItemSet.setId = cmdResult.setId;
      newItemSet.itemAttributesList.push(newItemA);
      this.ItemCodingList.push(newItemSet);
    }
    this.SetNewEmptyItemAttPDFCoding(cmdResult.itemAttributeId);
  }
  //part of a small "normalise code" (avoid replication) quick win: called by coding page, coding full and PDFtroncontainer.
  public ApplyDeleteItemAttribute(itemSet: ItemSet | null, itemAtt: ReadOnlyItemAttribute | null) {
    //console.log("ApplyDeleteItemAttribute", itemSet, itemAtt);
    if (itemSet && itemAtt) {
      //remove the itemAttribute from itemSet
      //console.log("Before filter", itemSet.itemAttributesList.length);
      itemSet.itemAttributesList = itemSet.itemAttributesList.filter(obj => obj.itemAttributeId !== itemAtt.itemAttributeId);
      //console.log("After filter", itemSet.itemAttributesList.length);
      if (itemSet.itemAttributesList.length == 0) {
        //if itemset does not have item attributes, remove the itemset
        this.ItemCodingList = this.ItemCodingList.filter(obj => itemSet && obj.itemSetId !== itemSet.itemSetId);
      }
      //if (itemSet) console.log(itemSet.itemAttributesList.length);
      this.ClearItemAttPDFCoding();
    }
  }
  public DeleteItemAttPDFCodingPage(page: number, itemAttributeId: number) {
    this._BusyMethods.push("DeleteItemAttPDFCodingPage");
    this.ngZone.run(() => this.IsBusy);
    //console.log("DeleteItemAttPDFCodingPage", page, itemAttributeId, this._BusyMethods);
    let existing: ItemAttributePDF | undefined = undefined;

    if (this.CurrentItemAttPDFCoding.Criteria.itemAttributeId == itemAttributeId && this.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
      //good, we have this and should check if we need to change the object for a given page...
      existing = this.CurrentItemAttPDFCoding.ItemAttPDFCoding.find(found => found.page == page);
    }
    if (existing == undefined) {
      //not good. We don't know what to delete...
      this.RemoveBusy("DeleteItemAttPDFCodingPage");
      this.ngZone.run(() => this.IsBusy);
      this.modalService.GenericErrorMessage("Sorry, we can't find the PDF-coding to delete. \nNo Data was changed!\nIf the problem persists, please contact EPPISupport.")
      return;
    }
    let body = JSON.stringify({ Value: existing.itemAttributePDFId });
    this._httpC.post<number>(this._baseUrl + "api/ItemSetList/DeletePDFCodingPage",
      body).subscribe(result => {
        //console.log("DeleteItemAttPDFCodingPage", result, this._BusyMethods);
        if (this._CurrentItemAttPDFCoding.ItemAttPDFCoding == null) {
          this._CurrentItemAttPDFCoding.ItemAttPDFCoding = [];
        }
        let indexOfRes = this._CurrentItemAttPDFCoding.ItemAttPDFCoding.findIndex((found: ItemAttributePDF) => result == found.itemAttributePDFId)
        //console.log("ItemAttPDFCoding before:", this._CurrentItemAttPDFCoding.ItemAttPDFCoding.length, this._CurrentItemAttPDFCoding.ItemAttPDFCoding);
        if (indexOfRes >= -1) this._CurrentItemAttPDFCoding.ItemAttPDFCoding.splice(indexOfRes, 1);
        //console.log("ItemAttPDFCoding after:",
        //    this._CurrentItemAttPDFCoding.ItemAttPDFCoding.length, this._CurrentItemAttPDFCoding.ItemAttPDFCoding
        //    , this._BusyMethods
        //);
        //if (indexOfRes == -1) this._CurrentItemAttPDFCoding.ItemAttPDFCoding.push(result);//add new page
        //else this._CurrentItemAttPDFCoding.ItemAttPDFCoding.splice(indexOfRes, 1, result);//replace existing - maybe we don't need to...
        //this._CurrentItemAttPDFCoding.Criteria = criteria;
        //this._CurrentItemAttPDFCoding.ItemAttPDFCoding = result;
        //this.ItemAttPDFCodingChanged.emit();
      }, error => {
        this.RemoveBusy("DeleteItemAttPDFCodingPage");
        this.modalService.SendBackHomeWithError(error);
      }
        , () => {
          this.RemoveBusy("DeleteItemAttPDFCodingPage");
          this.ngZone.run(() => this.IsBusy);
        }
      );

  }
  public ClearItemAttPDFCoding() {
    //console.log("ClearItemAttPDFCoding");

    this._CurrentItemAttPDFCoding = new ItemAttPDFCoding();
    this.ItemAttPDFCodingChanged.emit();
  }
  public SetNewEmptyItemAttPDFCoding(ItemAttId: number) {
    if (ItemAttId == 0 || this.ItemDocsService.CurrentDocId == 0) this.ClearItemAttPDFCoding();
    else {
      this._CurrentItemAttPDFCoding = new ItemAttPDFCoding();
      this._CurrentItemAttPDFCoding.Criteria.itemAttributeId = ItemAttId;
      this._CurrentItemAttPDFCoding.Criteria.itemDocumentId = this.ItemDocsService.CurrentDocId;
      this.ItemAttPDFCodingChanged.emit();
    }
  }
  private SelfSubscription4QuickCodingReport: Subscription | null = null;

  public Clear() {
    this._ItemsToReport = [];
    this._CodingReport = "";
    this._stopQuickReport = true;//gets set to false when we start fetching a report...
    this._PerItemReport = true;
    this.jsonReport = new JsonReport();
    this._CurrentItemIndex4QuickCodingReport = 0;
    if (this.SelfSubscription4QuickCodingReport) {
      this.SelfSubscription4QuickCodingReport.unsubscribe();
      this.SelfSubscription4QuickCodingReport = null;
    }
    this._BusyMethods = [];
    this.SelectedSetAttribute = null;
    this._ItemCodingList = [];
    this.ClearItemAttPDFCoding();
  }
  private _ItemsToReport: Item[] = [];
  private _ReviewSetsToReportOn: ReviewSet[] = [];
  private _CurrentItemIndex4QuickCodingReport: number = 0;
  public get QuickCodingReportIsRunning(): boolean {
    if (this._PerItemReport) return this._ItemsToReport.length > this._CurrentItemIndex4QuickCodingReport;
    else return !this.stopQuickReport;
  }
  public get ProgressOfQuickCodingReport(): string {
    if (this._PerItemReport) return "Retreiving Item " + (this._CurrentItemIndex4QuickCodingReport + 1).toString() + " of " + this._ItemsToReport.length;
    else return "Fetching 'Paged' Report (page: " + (this._CurrentItemIndex4QuickCodingReport + 1) + ")...";
  }
  public FetchCodingReport(Items: Item[], ReviewSetsToReportOn: ReviewSet[], isJson: boolean) {
    this._ItemsToReport = [];
    this._ReviewSetsToReportOn = [];
    this._PerItemReport = true;
    if (this.SelfSubscription4QuickCodingReport) {
      this.SelfSubscription4QuickCodingReport.unsubscribe();
      this.SelfSubscription4QuickCodingReport = null;
    }
    this._CurrentItemIndex4QuickCodingReport = 0;
    this._CodingReport = "";
    this.jsonReport = new JsonReport();
    this._stopQuickReport = false;
    if (!Items || Items.length < 1) {
      return;
    }
    this._BusyMethods.push("FetchCodingReport");
    this._ItemsToReport = Items;
    this._ReviewSetsToReportOn = ReviewSetsToReportOn;
    if (isJson && this._ReviewSetsToReportOn) {
      let cSets: ReviewSet4ER4Json[] = [];
      for (let rs of this._ReviewSetsToReportOn) {
        let setForReport = new ReviewSet4ER4Json(rs);
        //console.log("adding this set to report", setForReport);
        cSets.push(setForReport);
      }
      this.jsonReport.CodeSets = cSets;
    }
    this.InterimGetItemCodingForReport(isJson);
    this.RemoveBusy("FetchCodingReport");
  }
  public FetchSingleCodingReport(itemSet: ItemSet, item: Item): string {
    // not setting this._stopQuickReport as this is coming from ItemDetails/Coding record - might need to change if we'll call this method from the quickcodingreport component.
    let result: string = "";
    let reviewSet = this.ReviewSetsService.FindSetById(itemSet.setId);
    if (!reviewSet) return result;
    result += "<h4>" + item.shortTitle + " [ID: " + item.itemId + "]</h4>";
    result += "<br /><h6>Reviewer: " + itemSet.contactName + "</h6>";
    result += "<p><h4>" + reviewSet.set_name + (itemSet.isCompleted ? " (completed)" : " (incomplete)") + "</h4></p><p><ul>";
    for (let attributeSet of reviewSet.attributes) {
      //console.log("about to go into writeCodingReportAttributesWithArms", itemSet, attributeSet);
      result += this.writeCodingReportAttributesWithArms(itemSet, attributeSet);
    }
    result += "</ul></p>";
    //console.log("about to go into OutcomesTable", itemSet.outcomeItemList.outcomesList);
    result += "<p>" + this.OutcomesTable(itemSet.OutcomeList) + "</p>";
    return result;
  }

  private InterimGetItemCodingForReport(isJson: boolean) {
    let GotCancelled: boolean = false;
    if (this.stopQuickReport == true) {
      //makes the index jump forward so that below this.QuickCodingReportIsRunning will return "false" triggering the gracious end of recursion.
      this._CurrentItemIndex4QuickCodingReport = this._ItemsToReport.length + 1;
      this._stopQuickReport = false;
      GotCancelled = true;
    }
    if (!this.SelfSubscription4QuickCodingReport) {
      //initiate recursion, ugh!
      if (isJson) {
        this.SelfSubscription4QuickCodingReport = this.DataChanged.subscribe(
          () => {
            this.AddToJSONQuickCodingReport();
            this._CurrentItemIndex4QuickCodingReport++;
            this.InterimGetItemCodingForReport(isJson);
          }//no error handling: any error in this.Fetch(...) sends back home!!
        );
      } else {
        this.SelfSubscription4QuickCodingReport = this.DataChanged.subscribe(
          () => {
            this.AddToQuickCodingReport();
            this._CurrentItemIndex4QuickCodingReport++;
            this.InterimGetItemCodingForReport(isJson);
          }//no error handling: any error in this.Fetch(...) sends back home!!
        );
      }
    }
    if (!this.QuickCodingReportIsRunning) {
      if (this.SelfSubscription4QuickCodingReport) {
        this.SelfSubscription4QuickCodingReport.unsubscribe();
        this.SelfSubscription4QuickCodingReport = null;
      }
      if (GotCancelled) {//we don't want to show partial results!
        this.jsonReport = new JsonReport();
        this._CodingReport = "";
      }
      return;
    }
    //passing negative item IDs make the ItemList object grab the full text as well as "normal coding"
    else this.Fetch(-this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport].itemId);
  }
  private AddToQuickCodingReport() {
    if (this._CodingReport == "") {
      // this._CodingReport = "Start of report<br />";
    }
    const currentItem = this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport];
    //console.log("AddToQuickCodingReport", currentItem);
    if (!currentItem || currentItem.itemId == 0) return;
    //this._CodingReport += "Item: "
    //    + this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport].itemId.toString()
    //    + " contains " + this.ItemCodingList.length + " item sets. <br />";
    this._CodingReport += "<h4>ID " + currentItem.itemId.toString() + ": " + currentItem.shortTitle.replace(/</g, "&lt;") + "</h4><br />";
      ItemListService.GetCitation(currentItem) + "<br />";
    if (currentItem.oldItemId != "") {
      this._CodingReport += "<b>Your ID:</b> " + currentItem.oldItemId + "<br />";
    }
    if (currentItem.abstract != "") {
      this._CodingReport += '<div class="small mt-1"><b>Abstract:</b> ' + currentItem.abstract.replace(/</g, "&lt;") + "</div>";
    }
    this.AddCodingToReport();
    this._CodingReport += "<hr />";
  }
  private AddCodingToReport() {
    //console.log("AddCodingToReport", this._ReviewSetsToReportOn.length);
    for (let i = 0; i < this._ReviewSetsToReportOn.length; i++) {
      let reviewSet: ReviewSet = this._ReviewSetsToReportOn[i];
      for (let itemSet of this._ItemCodingList) {
        if (itemSet.setId == this._ReviewSetsToReportOn[i].set_id && itemSet.isCompleted == true) {
          this._CodingReport += "<br /><h6>Reviewer: " + itemSet.contactName + "</h6>";

          if (reviewSet != null) {
            this._CodingReport += "<p><h4>" + reviewSet.set_name + "</h4></p><p><ul>";
            for (let attributeSet of reviewSet.attributes) {
              //console.log("about to go into writeCodingReportAttributesWithArms", itemSet, attributeSet);
              this._CodingReport += this.writeCodingReportAttributesWithArms(itemSet, attributeSet);
            }
            this._CodingReport += "</ul></p>";
            //console.log("about to go into OutcomesTable", itemSet.outcomeItemList.outcomesList);
            this._CodingReport += "<p>" + this.OutcomesTable(itemSet.OutcomeList) + "</p>";
          }

        }
      }
    }
  }
  private AddToJSONQuickCodingReport() {
    const currentItem = this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport];
    let jItem: Item4ER4Json = new Item4ER4Json(currentItem);
    for (let i = 0; i < this._ReviewSetsToReportOn.length; i++) {
      let reviewSet: ReviewSet = this._ReviewSetsToReportOn[i];
      for (let itemSet of this._ItemCodingList) {
        if (itemSet.setId == this._ReviewSetsToReportOn[i].set_id && itemSet.isCompleted == true) {
          for (let ia of itemSet.itemAttributesList) {
            jItem.Codes.push(new Attribute4ER4Json(ia));
            //console.log(ia, new Attribute4ER4Json(ia));
          }
          for (let o of itemSet.OutcomeList) {
            jItem.Outcomes.push(new Outcome4ER4Json(o));
          }
        }
      }
    }
    //console.log("Json report add item:", jItem.ItemId, jItem.Codes.length, this.jsonReport.References.length);
    this.jsonReport.References.push(jItem);
  }
  private writeCodingReportAttributesWithArms(itemSet: ItemSet, attributeSet: SetAttribute): string {
    let report: string = "";
    let roias = itemSet.itemAttributesList.filter(found => found.attributeId == attributeSet.attribute_id);
    //console.log("roias", roias, itemSet.itemAttributesList, attributeSet.attribute_id);
    if (roias != null && roias.length > 0) {
      for (let roia of roias) {
        let AttributeName = attributeSet.attribute_name;
        if (roia.armId != 0) {
          AttributeName += " [" + roia.armTitle + "]";
        }

        report += '<li class="text-success"><span class="font-weight-bold">' + AttributeName + "</span>";
        if (roia.additionalText.length > 0) report += "<br /><i>" + roia.additionalText.replace("\n", "<br />") + "</i>";
        if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {

          report += this.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails);
        }
        report += "</li>";
      }
      if (this.CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
      {
        report += "<ul>";
        for (let child of attributeSet.attributes) {
          report += this.writeCodingReportAttributesWithArms(itemSet, child);
        }
        report += "</ul>";
      }
    }
    else {
      if (this.CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
      {
        report += '<li class="text-muted">' + attributeSet.attribute_name + "</li>";
        report += "<ul>";
        for (let child of attributeSet.attributes) {
          report += this.writeCodingReportAttributesWithArms(itemSet, child);
        }
        report += "</ul>";
      }
    }
    return report;
  }
  public addFullTextToComparisonReport(list: ItemAttributeFullTextDetails[]): string {
    //console.log("addFullTextToComparisonReport", list);
    let result: string = "";
    for (let ftd of list) {
      result += "<br style='mso-data-placement:same-cell;'  />" + ftd.docTitle + ": ";
      if (ftd.isFromPDF) {
        let rres = ftd.text.replace(/\[\u00ACs\]/g, '');//"\u00AC" is "¬", wouldn't match it otherwise
        rres = rres.replace(/\[\u00ACe\]/g, "");
        result += "<span class='small text-info'>" + rres + "</span><br style='mso-data-placement:same-cell;'  />";//.replace(/\[¬s\]/g, '').replace(/\[¬e\/]/g, "") + "</span>";
      }
      else {
        result += "<code class='small'>" + ftd.text + "(from char " + ftd.textFrom.toString() + " to char " + ftd.textTo.toString()
          + ")</code><br style='mso-data-placement:same-cell;'  />";
      }
    }
    //console.log("addFullTextToComparisonReport", list, result);
    return result;
  }

  public MatchOutcomes(Outcomes1: Outcome[], Outcomes2: Outcome[], Outcomes3: Outcome[] | null = null): any[] {
    let result: any[] = [];
    for (const o of Outcomes2) {
      o.SetCalculatedValues();
    }
    if (Outcomes3 != null && Outcomes3.length > 0) {
      for (const o of Outcomes3) {
        o.SetCalculatedValues();
      }
      this.MatchOutcomeInTriplets(Outcomes1, Outcomes2, Outcomes3, result);
    } else {
      this.MatchOutcomeInPairs(Outcomes1, Outcomes2, result);
    }
    console.log("MatchOutcomesResult:", result);
    return result;
  }

  private MatchOutcomeInPairs(Outcomes1: Outcome[], Outcomes2: Outcome[], result: any[]): any[] {
    //let usedIds1: number[] = [];
    //let usedIds2: number[] = [];
    for (const o1 of Outcomes1) {
      o1.SetCalculatedValues();
      const o2s = Outcomes2.filter(f => f.outcomeTypeId == o1.outcomeTypeId && f.es == o1.es && f.sees == o1.sees);
      if (o2s.length == 1) {
        //easy case - we have only one possible match, we'll use it...
        let resLine = [o1.outcomeId, o2s[0].outcomeId];
        //usedIds1.push(o1.outcomeId);
        //usedIds2.push(o2s[0].outcomeId);
        Outcomes2 = Outcomes2.filter(f => f.outcomeId != o2s[0].outcomeId); //remove the matched outcome
        result.push(resLine);
      } else if (o2s.length > 1) {
        //Difficult one, we'll try calculating an additional matching score, based on Intervention, Outcome, Comparison code, plus timepoint and arms.
        //Max total score is 6
        let bestMatchId = this.BestMatchOverOneToManyOutcomes(o1, o2s);
        if (bestMatchId > 0) {
          let resLine = [o1.outcomeId, bestMatchId];
          //usedIds1.push(o1.outcomeId);
          //usedIds2.push(bestMatchId);
          Outcomes2 = Outcomes2.filter(f => f.outcomeId != bestMatchId);
          result.push(resLine);
        }
      }
    }
    return result;
  }

  private MatchOutcomeInTriplets(Outcomes1: Outcome[], Outcomes2: Outcome[], Outcomes3: Outcome[], result: any[]): any[] {
    console.log('MatchOutcomeInTriplets!!!!!');
    for (const o1 of Outcomes1) {
      o1.SetCalculatedValues();
      const o2s = Outcomes2.filter(f => f.es == o1.es && f.sees == o1.sees);
      const o3s = Outcomes3.filter(f => f.es == o1.es && f.sees == o1.sees);
      //dealing with cases from the easiest to the hardest
      if (o2s.length == 1 && o3s.length == 1) {
        let resLine = [o1.outcomeId, o2s[0].outcomeId, o3s[0].outcomeId];
        //Outcomes1 = Outcomes1.filter(f => f.outcomeId != o1.outcomeId);
        Outcomes2 = Outcomes2.filter(f => f.outcomeId != o2s[0].outcomeId);
        Outcomes3 = Outcomes3.filter(f => f.outcomeId != o3s[0].outcomeId);
        result.push(resLine);
      } else if (o2s.length == 1 && o3s.length == 0) {
        let resLine = [o1.outcomeId, o2s[0].outcomeId, -1];
        Outcomes2 = Outcomes2.filter(f => f.outcomeId != o2s[0].outcomeId);
        result.push(resLine);
      } else if (o2s.length == 0 && o3s.length == 1) {
        let resLine = [o1.outcomeId, -1, o3s[0].outcomeId];
        Outcomes3 = Outcomes3.filter(f => f.outcomeId != o3s[0].outcomeId);
        result.push(resLine);
      }
      else if (o2s.length > 1 && o3s.length < 2) {
        //need to pick one from o2s, but no ambiguity re o3s
        const bestMatch = this.BestMatchOverOneToManyOutcomes(o1, o2s);
        if (bestMatch > 0) {
          if (o3s.length == 1) {
            let resLine = [o1.outcomeId, bestMatch, o3s[0].outcomeId];
            Outcomes2 = Outcomes2.filter(f => f.outcomeId != bestMatch);
            Outcomes3 = Outcomes3.filter(f => f.outcomeId != o3s[0].outcomeId);
            result.push(resLine);
          } else {
            let resLine = [o1.outcomeId, bestMatch, -1];
            Outcomes2 = Outcomes2.filter(f => f.outcomeId != bestMatch);
            result.push(resLine);
          }
        } else {
          if (o3s.length == 1) {
            let resLine = [o1.outcomeId, -1, o3s[0].outcomeId];
            Outcomes3 = Outcomes3.filter(f => f.outcomeId != o3s[0].outcomeId);
            result.push(resLine);
          }
        }
      }
      else if (o3s.length > 1 && o2s.length < 2) {
        //need to pick one from o3s, but no ambiguity re o2s
        const bestMatch = this.BestMatchOverOneToManyOutcomes(o1, o3s);
        if (bestMatch > 0) {
          if (o2s.length == 1) {
            let resLine = [o1.outcomeId, o2s[0].outcomeId, bestMatch];
            Outcomes2 = Outcomes2.filter(f => f.outcomeId != o2s[0].outcomeId);
            Outcomes3 = Outcomes3.filter(f => f.outcomeId != bestMatch);
            result.push(resLine);
          } else {
            let resLine = [o1.outcomeId, -1, bestMatch];
            Outcomes3 = Outcomes3.filter(f => f.outcomeId != bestMatch);
            result.push(resLine);
          }
        } else {
          if (o2s.length == 1) {
            let resLine = [o1.outcomeId, o2s[0].outcomeId, -1];
            Outcomes3 = Outcomes3.filter(f => f.outcomeId != o3s[0].outcomeId);
            result.push(resLine);
          }
        }
      }
      else if (o2s.length > 1 && o3s.length > 1) {
        //meh, far too many possible matches!!
        const bestMatch2s = this.BestMatchOverOneToManyOutcomes(o1, o2s);
        const bestMatch3s = this.BestMatchOverOneToManyOutcomes(o1, o3s);
        if (bestMatch2s > 0 || bestMatch3s > 0) {
          //we do have something...
          let resLine = [o1.outcomeId, bestMatch2s, bestMatch3s];
          Outcomes2 = Outcomes2.filter(f => f.outcomeId != bestMatch2s);
          Outcomes3 = Outcomes3.filter(f => f.outcomeId != bestMatch3s);
          result.push(resLine);
        }
      }
    }
    if (Outcomes2.length > 0 && Outcomes3.length > 0) {
      //we matched Outcomes1 against the other two, but we still have unmatched outcomes in the latter two, so we'll try matching them pairwise
      let PairwiseResult: any[] = [];
      this.MatchOutcomeInPairs(Outcomes2, Outcomes3, PairwiseResult);
      for (const pwline of PairwiseResult) {
        let resLine = [-1, pwline[0], pwline[1]];
        //no need to filter Outcomes2, Outcomes3 as we're done using them
        result.push(resLine);
      }
    }
    return result;
  }

  private BestMatchOverOneToManyOutcomes(o1: Outcome, o2s: Outcome[]): number {
    let scoredMatches: OutcomeSimilaritiesKeyValue[] = [];
    for (const o2 of o2s) {
      let o2Score = new OutcomeSimilaritiesKeyValue(o2, 0);
      if (o1.interventionText !== '' && o1.interventionText == o2.interventionText) o2Score.value++;
      if (o1.outcomeText !== '' && o1.outcomeText == o2.outcomeText) o2Score.value++;
      if (o1.controlText !== '' && o1.controlText == o2.controlText) o2Score.value++;
      if (o1.itemTimepointId > 0 && o1.itemTimepointId == o2.itemTimepointId) o2Score.value++;
      if (o1.itemArmIdGrp1 > 0 && o1.itemArmIdGrp1 == o2.itemArmIdGrp1) o2Score.value++;
      if (o1.itemArmIdGrp2 > 0 && o1.itemArmIdGrp2 == o2.itemArmIdGrp2) o2Score.value++;

      if (o2Score.value > 0) scoredMatches.push(o2Score);
    }
    let CurrentMaxScore: number = 6;
    while (CurrentMaxScore >= 1) {
      const matchesAtThisScore = scoredMatches.filter(f => f.value == CurrentMaxScore);
      if (matchesAtThisScore.length == 1) {
        return matchesAtThisScore[0].key.outcomeId;
      } else if (matchesAtThisScore.length > 1) {
        //too many matches, what can we do?
        if (CurrentMaxScore >= 4) {
          //outcomes are similar enough, we'll take the 1st as a possible match - better than giving up entirely?
          return matchesAtThisScore[0].key.outcomeId;
        }
        //otherwise we do give up
        return -1;
      }
      else {
        //lower the matching score and see if we find a unique match...
        CurrentMaxScore--;
      }
    }
    return -1;
  }
  public GetUnmatchedOutcomes(Outcomes: Outcome[], matchedIds: any[], position: number): Outcome[] {
    let res = Outcomes;
    for (let row of matchedIds ) {
      res = res.filter(f => f.outcomeId !== row[position]);
    }
    return res;
  }

  public OutcomesTable(Outcomes: Outcome[], addHeader: boolean = true): string {
    //this table doesn't compare outcomes, so styles are the same for all data-cells
    const TDstrings: StringKeyValue[] = [new StringKeyValue("title", "<td>")
      , new StringKeyValue("outcomeDescription", "<td>"), new StringKeyValue("timepointDisplayValue", "<td>"), new StringKeyValue("outcomeText", "<td>")
      , new StringKeyValue("interventionText", "<td>"), new StringKeyValue("controlText", "<td>"), new StringKeyValue("grp1ArmName", "<td>")
      , new StringKeyValue("grp2ArmName", "<td>"), new StringKeyValue("data1", "<td>"), new StringKeyValue("data2", "<td>")
      , new StringKeyValue("data3", "<td>"), new StringKeyValue("data4", "<td>"), new StringKeyValue("data5", "<td>")
      , new StringKeyValue("data6", "<td>"), new StringKeyValue("data7", "<td>"), new StringKeyValue("data8", "<td>")
      , new StringKeyValue("data11", "<td>"), new StringKeyValue("data12", "<td>"), new StringKeyValue("data13", "<td>")
      , new StringKeyValue("data14", "<td>"), new StringKeyValue("es", "<td>"), new StringKeyValue("sees", "<td>")
      , new StringKeyValue("OutcomeClassifications", "<td>")
    ];
    let retVal: string = "";
    let i: number = -1;
    const Start: string = addHeader ? "<p><b>Outcomes</b></p>" : "";
    let sortedOutcomes = Outcomes.sort(function (a, b) { return a.outcomeTypeId - b.outcomeTypeId });
    for (let o of sortedOutcomes) {
      if (i != o.outcomeTypeId) {
        if (retVal == "") {
          retVal = Start + "<table class='m-1' border='1'>";
        }
        else {
          retVal += "</table><table class='m-1' border='1'>";
        }
        i = o.outcomeTypeId;
        retVal += this.GetOutcomeHeaders(o);
      }
      retVal += this.GetOutcomeInnerTable(o, TDstrings);
    }
    return retVal + "</table>";
  }
  
  private OutcomesFieldAreEqual(outcomes: Outcome[], fieldName: string): boolean {
    const key = fieldName as keyof Outcome;
    if (outcomes.length < 2) return false;//safety!
    if (outcomes[0][key] == outcomes[1][key]) {
      if (outcomes[2] == undefined || outcomes[2][key] == outcomes[0][key]) return true;
    }
    return false;
  }

  private OutcomesClassifictaionsAreEqual(outcomes: Outcome[]): boolean {
    if (outcomes.length < 2) return false;//safety!
    let list1 = outcomes[0].outcomeCodes.outcomeItemAttributesList.sort(function (a, b) { return a.attributeId - b.attributeId });
    let l1Str = "";
    for (let el of list1) l1Str += el.attributeId + ',';
    let list2 = outcomes[1].outcomeCodes.outcomeItemAttributesList.sort(function (a, b) { return a.attributeId - b.attributeId });
    let l2Str = "";
    for (let el of list2) l2Str += el.attributeId + ',';
    let list3: OutcomeItemAttribute[] = (outcomes[2] != undefined) ? outcomes[2].outcomeCodes.outcomeItemAttributesList.sort(function (a, b) { return a.attributeId - b.attributeId }) : [];
    let l3Str = "";
    if (list3.length > 0) for (let el of list3) l2Str += el.attributeId + ',';
    if (l1Str == l2Str) {
      if (l3Str == "" || l3Str == l1Str) return true;
    }
    return false;
  }

  public GetOutcomeTableForComparison(Outcomes: Outcome[], Reviewer1: string, Reviewer2: string, Reviewer3: string) {
    //used to get a table with 2 or 3 outcomes, showing differences between outcomes entered by different people
    //outcomes fed into this method are all of the same type
    let TDs: StringKeyValue[] = this.GetOutcomeComparisonStyledTDs(Outcomes);
    let retVal: string = "<table class='m-1' border='1'>";
    let i = 0;
    for (let o of Outcomes) {
      let Name = "";
      if (i == 0) {
        retVal += this.GetOutcomeHeaders(o, true);
        Name = Reviewer1;
      }
      else if (i == 1) Name = Reviewer2;
      else if (i == 2) Name = Reviewer3;
      else break;
      i++;
      retVal += this.GetOutcomeInnerTable(o, TDs, Name);
    }
    return retVal + "</table>";
  }
  private GetOutcomeComparisonStyledTDs(Outcomes: Outcome[]): StringKeyValue[] {
    let res: StringKeyValue[] = [];
    if (Outcomes.length == 0) return res;

    const Equal = "<td>";
    const Different = "<td class='light-yellow-bg'>";
    res.push(new StringKeyValue("title", this.OutcomesFieldAreEqual(Outcomes, "title") ? Equal : Different));
    res.push(new StringKeyValue("outcomeDescription", this.OutcomesFieldAreEqual(Outcomes, "outcomeDescription") ? Equal : Different));
    res.push(new StringKeyValue("timepointDisplayValue", this.OutcomesFieldAreEqual(Outcomes, "timepointDisplayValue") ? Equal : Different));
    res.push(new StringKeyValue("outcomeText", this.OutcomesFieldAreEqual(Outcomes, "outcomeText") ? Equal : Different));
    res.push(new StringKeyValue("interventionText", this.OutcomesFieldAreEqual(Outcomes, "interventionText") ? Equal : Different));
    res.push(new StringKeyValue("controlText", this.OutcomesFieldAreEqual(Outcomes, "controlText") ? Equal : Different));
    res.push(new StringKeyValue("grp1ArmName", this.OutcomesFieldAreEqual(Outcomes, "grp1ArmName") ? Equal : Different));
    res.push(new StringKeyValue("grp2ArmName", this.OutcomesFieldAreEqual(Outcomes, "grp2ArmName") ? Equal : Different));
    switch (Outcomes[0].outcomeTypeId) {
      case 0: // manual entry
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("data3", this.OutcomesFieldAreEqual(Outcomes, "data3") ? Equal : Different));
        res.push(new StringKeyValue("data4", this.OutcomesFieldAreEqual(Outcomes, "data4") ? Equal : Different));
        res.push(new StringKeyValue("data5", this.OutcomesFieldAreEqual(Outcomes, "data5") ? Equal : Different));
        res.push(new StringKeyValue("data6", this.OutcomesFieldAreEqual(Outcomes, "data6") ? Equal : Different));
        res.push(new StringKeyValue("data7", this.OutcomesFieldAreEqual(Outcomes, "data7") ? Equal : Different));
        res.push(new StringKeyValue("data8", this.OutcomesFieldAreEqual(Outcomes, "data8") ? Equal : Different));
        res.push(new StringKeyValue("data11", this.OutcomesFieldAreEqual(Outcomes, "data11") ? Equal : Different));
        res.push(new StringKeyValue("data12", this.OutcomesFieldAreEqual(Outcomes, "data12") ? Equal : Different));
        res.push(new StringKeyValue("data13", this.OutcomesFieldAreEqual(Outcomes, "data13") ? Equal : Different));
        res.push(new StringKeyValue("data14", this.OutcomesFieldAreEqual(Outcomes, "data14") ? Equal : Different));
        break;

      case 1: // n, mean, SD
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("data3", this.OutcomesFieldAreEqual(Outcomes, "data3") ? Equal : Different));
        res.push(new StringKeyValue("data4", this.OutcomesFieldAreEqual(Outcomes, "data4") ? Equal : Different));
        res.push(new StringKeyValue("data5", this.OutcomesFieldAreEqual(Outcomes, "data5") ? Equal : Different));
        res.push(new StringKeyValue("data6", this.OutcomesFieldAreEqual(Outcomes, "data6") ? Equal : Different));
        res.push(new StringKeyValue("es", this.OutcomesFieldAreEqual(Outcomes, "es") ? Equal : Different));
        res.push(new StringKeyValue("sees", this.OutcomesFieldAreEqual(Outcomes, "sees") ? Equal : Different));
        break;

      case 2: // binary 2 x 2 table
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("data3", this.OutcomesFieldAreEqual(Outcomes, "data3") ? Equal : Different));
        res.push(new StringKeyValue("data4", this.OutcomesFieldAreEqual(Outcomes, "data4") ? Equal : Different));
        res.push(new StringKeyValue("es", this.OutcomesFieldAreEqual(Outcomes, "es") ? Equal : Different));
        res.push(new StringKeyValue("sees", this.OutcomesFieldAreEqual(Outcomes, "sees") ? Equal : Different));
        break;

      case 3: //n, mean SE
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("data3", this.OutcomesFieldAreEqual(Outcomes, "data3") ? Equal : Different));
        res.push(new StringKeyValue("data4", this.OutcomesFieldAreEqual(Outcomes, "data4") ? Equal : Different));
        res.push(new StringKeyValue("data5", this.OutcomesFieldAreEqual(Outcomes, "data5") ? Equal : Different));
        res.push(new StringKeyValue("data6", this.OutcomesFieldAreEqual(Outcomes, "data6") ? Equal : Different));
        res.push(new StringKeyValue("es", this.OutcomesFieldAreEqual(Outcomes, "es") ? Equal : Different));
        res.push(new StringKeyValue("sees", this.OutcomesFieldAreEqual(Outcomes, "sees") ? Equal : Different));
        break;

      case 4: //n, mean CI
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("data3", this.OutcomesFieldAreEqual(Outcomes, "data3") ? Equal : Different));
        res.push(new StringKeyValue("data4", this.OutcomesFieldAreEqual(Outcomes, "data4") ? Equal : Different));
        res.push(new StringKeyValue("data5", this.OutcomesFieldAreEqual(Outcomes, "data5") ? Equal : Different));
        res.push(new StringKeyValue("data6", this.OutcomesFieldAreEqual(Outcomes, "data6") ? Equal : Different));
        res.push(new StringKeyValue("data7", this.OutcomesFieldAreEqual(Outcomes, "data7") ? Equal : Different));
        res.push(new StringKeyValue("data8", this.OutcomesFieldAreEqual(Outcomes, "data8") ? Equal : Different));
        res.push(new StringKeyValue("es", this.OutcomesFieldAreEqual(Outcomes, "es") ? Equal : Different));
        res.push(new StringKeyValue("sees", this.OutcomesFieldAreEqual(Outcomes, "sees") ? Equal : Different));
        break;

      case 5: //n, t or p value
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("data3", this.OutcomesFieldAreEqual(Outcomes, "data3") ? Equal : Different));
        res.push(new StringKeyValue("data4", this.OutcomesFieldAreEqual(Outcomes, "data4") ? Equal : Different));
        res.push(new StringKeyValue("es", this.OutcomesFieldAreEqual(Outcomes, "es") ? Equal : Different));
        res.push(new StringKeyValue("sees", this.OutcomesFieldAreEqual(Outcomes, "sees") ? Equal : Different));
        break;

      case 6: // binary 2 x 2 table
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("data3", this.OutcomesFieldAreEqual(Outcomes, "data3") ? Equal : Different));
        res.push(new StringKeyValue("data4", this.OutcomesFieldAreEqual(Outcomes, "data4") ? Equal : Different));
        res.push(new StringKeyValue("es", this.OutcomesFieldAreEqual(Outcomes, "es") ? Equal : Different));
        res.push(new StringKeyValue("sees", this.OutcomesFieldAreEqual(Outcomes, "sees") ? Equal : Different));
        break;

      case 7: // correlation coefficient r
        res.push(new StringKeyValue("data1", this.OutcomesFieldAreEqual(Outcomes, "data1") ? Equal : Different));
        res.push(new StringKeyValue("data2", this.OutcomesFieldAreEqual(Outcomes, "data2") ? Equal : Different));
        res.push(new StringKeyValue("sees", this.OutcomesFieldAreEqual(Outcomes, "sees") ? Equal : Different));
        break;

      default:
        break;
    }
    res.push(new StringKeyValue("OutcomeClassifications", this.OutcomesClassifictaionsAreEqual(Outcomes) ? Equal : Different));
    return res;
  }


  private GetOutcomeHeaders(o: Outcome, forComparison = false): string {
    let retVal = "<tr bgcolor='silver'>" + (forComparison ? "<th>Reviewer</th>" : "")
            + "<th>Title</th><th>Description</th><th>Timepoint</th><th>Outcome</th><th>Intervention</th><th>Control</th><th>Arms</th><th>Type</th>";
    switch (o.outcomeTypeId) {
      case 0: // manual entry
        retVal += "<th>SMD</th><th>SE</th><th>r</th><th>SE</th><th>Odds ratio</th><th>SE</th><th>Risk ratio</th><th>SE</th><th>Risk difference</th><th>SE</th><th>Mean difference</th><th>SE</th>";
        break;

      case 1: // n, mean, SD
        retVal += "<th>Group 1 N</th><th>Group 2 N</th><th>Group 1 mean</th><th>Group 2 mean</th><th>Group 1 SD</th>" +
          "<th>Group 2 SD</th><th>SMD</th><th>SE</th>";
        break;

      case 2: // binary 2 x 2 table
        retVal += "<th>Group 1 events</th><th>Group 2 events</th><th>Group 1 no events</th><th>Group 2 no events</th><th>Odds ratio</th><th>SE (log OR)</th>";
        break;

      case 3: //n, mean SE
        retVal += "<th>Group 1 N</th><th>Group 2 N</th><th>Group 1 mean</th><th>Group 2 mean</th><th>Group 1 SE</th>" +
          "<th>Group 2 SE</th><th>SMD</th><th>SE</th>";
        break;

      case 4: //n, mean CI
        retVal += "<th>Group 1 N</th><th>Group 2 N</th><th>Group 1 mean</th><th>Group 2 mean</th><th>Group 1 CI lower</th>" +
          "<th>Group 1 CI upper</th><th>Group 2 CI lower</th><th>Group 2 CI upper</th><th>SMD</th><th>SE</th>";
        break;

      case 5: //n, t or p value
        retVal += "<th>Group 1 N</th><th>Group 2 N</th><th>Group 1 mean</th><th>Group 2 mean</th><th>t-value</th>" +
          "<th>p-value</th><th>SMD</th><th>SE</th>";
        break;

      case 6: // diagnostic test 2 x 2 table
        retVal += "<th>True positive</th><th>False positive</th><th>False negative</th><th>True negative</th><th>Diagnostic odds ratio</th><th>SE (log dOR)</th>";
        break;

      case 7: // correlation coeffiecient r
        retVal += "<th>Group size</th><th>r</th><th>SE (Z transformed)</th>";
        break;

      default:
        break;
    }
    return retVal + "<th>Outcome Classifications</th></tr>";
  }

  private GetOutcomeInnerTable(o: Outcome, TDs: StringKeyValue[], ReviewerName: string = ""): string {
    let retVal = "<tr>";
    if (ReviewerName != "") {
      retVal += "<td>" + ReviewerName + "</td>";
    }
    retVal += TDs.find(f => f.key == "title")?.value + o.title
      + "</td>" + TDs.find(f => f.key == "outcomeDescription")?.value + o.outcomeDescription.replace("\r", "<br style='mso-Data-placement:same-cell;'  />")
      + "</td>" + TDs.find(f => f.key == "timepointDisplayValue")?.value + o.timepointDisplayValue
      + "</td>" + TDs.find(f => f.key == "outcomeText")?.value + o.outcomeText
      + "</td>" + TDs.find(f => f.key == "interventionText")?.value + o.interventionText
      + "</td>" + TDs.find(f => f.key == "controlText")?.value + o.controlText + "</td>";
    let armsTD1 = TDs.find(f => f.key == "grp1ArmName")?.value;
    let armsTD2 = TDs.find(f => f.key == "grp2ArmName")?.value;
    if (!armsTD1) armsTD1 = "<td>";
    if (armsTD2 && armsTD2.length > armsTD1.length) armsTD1 = armsTD2;
    retVal += armsTD1 + (o.grp1ArmName != "" ? "Arm1:&nbsp;" + o.grp1ArmName + "; " : "")
      + (o.grp2ArmName != "" ? "Arm2:&nbsp;" + o.grp2ArmName : "") + "</td>"
    switch (o.outcomeTypeId) {
      case 0: // manual entry
        retVal += "<td>Manual entry</td>" +
        TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data3")?.value + (typeof o.data3 === 'number' ? o.data3.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data4")?.value + (typeof o.data4 === 'number' ? o.data4.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data5")?.value + (typeof o.data5 === 'number' ? o.data5.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data6")?.value + (typeof o.data6 === 'number' ? o.data6.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data7")?.value + (typeof o.data7 === 'number' ? o.data7.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data8")?.value + (typeof o.data8 === 'number' ? o.data8.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data11")?.value + (typeof o.data11 === 'number' ? o.data11.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data12")?.value + (typeof o.data12 === 'number' ? o.data12.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data13")?.value + (typeof o.data13 === 'number' ? o.data13.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data14")?.value + (typeof o.data14 === 'number' ? o.data14.toFixed(3) : "NaN") + "</td>";
        break;

      case 1: // n, mean, SD
        retVal += "<td>Continuous: Ns, means and SD</td>" +
        TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data3")?.value + (typeof o.data3 === 'number' ? o.data3.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data4")?.value + (typeof o.data4 === 'number' ? o.data4.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data5")?.value + (typeof o.data5 === 'number' ? o.data5.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "data6")?.value + (typeof o.data6 === 'number' ? o.data6.toFixed(3) : "NaN") + "</td>" +
        TDs.find(f => f.key == "es")?.value + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td>" + 
        TDs.find(f => f.key == "sees")?.value + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
        break;

      case 2: // binary 2 x 2 table
        retVal += "<td>Binary: 2 x 2 table</td>" +
          TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data3")?.value + (typeof o.data3 === 'number' ? o.data3.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data4")?.value + (typeof o.data4 === 'number' ? o.data4.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "es")?.value + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "sees")?.value + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
        break;

      case 3: //n, mean SE
        retVal += "<td>Continuous: N, Mean, SE</td>" +
          TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data3")?.value + (typeof o.data3 === 'number' ? o.data3.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data4")?.value + (typeof o.data4 === 'number' ? o.data4.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data5")?.value + (typeof o.data5 === 'number' ? o.data5.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data6")?.value + (typeof o.data6 === 'number' ? o.data6.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "es")?.value + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "sees")?.value + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
        break;

      case 4: //n, mean CI
        retVal += "<td>Continuous: N, Mean, CI</td>" +
          TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data3")?.value + (typeof o.data3 === 'number' ? o.data3.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data4")?.value + (typeof o.data4 === 'number' ? o.data4.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data5")?.value + (typeof o.data5 === 'number' ? o.data5.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data6")?.value + (typeof o.data6 === 'number' ? o.data6.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data7")?.value + (typeof o.data7 === 'number' ? o.data7.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data8")?.value + (typeof o.data8 === 'number' ? o.data8.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "es")?.value + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "sees")?.value + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
        break;

      case 5: //n, t or p value
        retVal += "<td>Continuous: N, t- or p-value</td>" +
          TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data3")?.value + (typeof o.data3 === 'number' ? o.data3.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data4")?.value + (typeof o.data4 === 'number' ? o.data4.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "es")?.value + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "sees")?.value + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
        break;

      case 6: // binary 2 x 2 table
        retVal += "<td>Diagnostic test: 2 x 2 table</td>" +
          TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data3")?.value + (typeof o.data3 === 'number' ? o.data3.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data4")?.value + (typeof o.data4 === 'number' ? o.data4.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "es")?.value + (typeof o.es === 'number' ? o.es.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "sees")?.value + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
        break;

      case 7: // correlation coefficient r
        retVal += "<td>Correlation coefficient r</td>" +
          TDs.find(f => f.key == "data1")?.value + (typeof o.data1 === 'number' ? o.data1.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "data2")?.value + (typeof o.data2 === 'number' ? o.data2.toFixed(3) : "NaN") + "</td>" +
          TDs.find(f => f.key == "sees")?.value + (typeof o.sees === 'number' ? o.sees.toFixed(3) : "NaN") + "</td>";
        break;

      default:
        break;
    }
    retVal += TDs.find(f => f.key == "OutcomeClassifications")?.value;
    for (let OIA of o.outcomeCodes.outcomeItemAttributesList) {
      retVal += OIA.attributeName + "<br style='mso-data-placement:same-cell;' >";
    }
    return retVal + "</td></tr>";
  }

  public CodingReportCheckChildSelected(itemSet: ItemSet, attributeSet: SetAttribute): boolean {
    if (itemSet) {
      for (let roia of itemSet.itemAttributesList) {
        if (roia.attributeId == attributeSet.attribute_id) return true;
      }
      for (let child of attributeSet.attributes) {
        if (this.CodingReportCheckChildSelected(itemSet, child) == true) {
          return true;
        }
      }
    }
    return false;
  }

  public FetchQuickQuestionReport(Items: Item[], nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
    if (this.SelfSubscription4QuickCodingReport) {
      this.SelfSubscription4QuickCodingReport.unsubscribe();
      this.SelfSubscription4QuickCodingReport = null;
    }
    this._CurrentItemIndex4QuickCodingReport = 0;
    this._CodingReport = "";
    this.jsonReport = new JsonReport();
    this._stopQuickReport = false;
    this._PerItemReport = true;
    if (!Items || Items.length < 1) {
      return;
    }
    //this._BusyMethods.push("FetchQuickQuestionReport");
    this._ItemsToReport = Items;
    this.InterimGetItemCodingForQuestionReport(nodesToReportOn, options);
    //this.RemoveBusy("FetchQuickQuestionReport");
  }
  private InterimGetItemCodingForQuestionReport(nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
    let GotCancelled: boolean = false;
    //console.log("in InterimGetItemCodingForQuestionReport", this._CurrentItemIndex4QuickCodingReport);
    if (this._stopQuickReport == true) {
      //makes the index jump forward so that below this.QuickCodingReportIsRunning will return "false" triggering the gracious end of recursion.
      this._CurrentItemIndex4QuickCodingReport = this._ItemsToReport.length + 1;
      this._stopQuickReport = false;
      GotCancelled = true;
    }
    if (!this.SelfSubscription4QuickCodingReport) {
      //initiate recursion, ugh!
      this.SelfSubscription4QuickCodingReport = this.DataChanged.subscribe(
        () => {
          //console.log("in QQ rep subscription, this._CurrentItemIndex4QuickCodingReport:", this._CurrentItemIndex4QuickCodingReport);
          this.AddToQuickQuestionReport(nodesToReportOn, options);
          this._CurrentItemIndex4QuickCodingReport++;
          this.InterimGetItemCodingForQuestionReport(nodesToReportOn, options);
        }//no error handling: any error in this.Fetch(...) sends back home!!
      );
    }
    if (!this.QuickCodingReportIsRunning) {
      //console.log("QuickQ rep isn't running (in interim fetch)", GotCancelled);
      if (this.SelfSubscription4QuickCodingReport) {
        this.SelfSubscription4QuickCodingReport.unsubscribe();
        this.SelfSubscription4QuickCodingReport = null;
        if (this._CodingReport.length != 0) this._CodingReport += "</table>";
      }
      if (GotCancelled) {//we don't want to show partial results!
        //this.jsonReport = new JsonReport();
        this._CodingReport = "";
      }
      return;
    }
    //passing negative item IDs make the ItemList object grab the full text as well as "normal coding"
    else this.Fetch(-this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport].itemId);
  }
  private AddToQuickQuestionReport(nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
    if (this._CodingReport == "") {
      //this._CodingReport = "Quick Question Report:<br />";
      this._CodingReport += "<table class='border border-dark'><tr><th class='border border-dark'>Item</th>";
      if (options.IncludeFullTitle) {
        this._CodingReport += "<th class='border border-dark'>Title</th>";
      }
      for (let node of nodesToReportOn) {
        this._CodingReport += "<th class='border border-dark'>" + node.name;
        if (options.ShowCodeIds) {
          if (node.nodeType == "ReviewSet") this._CodingReport += " (" + node.id.substring(2) + ")";
          else if (node.nodeType == "SetAttribute") this._CodingReport += " (" + node.id.substring(1) + ")";
        }
        this._CodingReport += "</th>";
      }
      this._CodingReport += "</tr>";
    }
    const currentItem = this._ItemsToReport[this._CurrentItemIndex4QuickCodingReport];
    //console.log("AddToQuickCodingReport", currentItem);
    if (!currentItem || currentItem.itemId == 0) return;
    this._CodingReport += "<tr><td class='border border-dark'>" + currentItem.shortTitle + " (ID:" + currentItem.itemId + ")</td>";
    if (options.IncludeFullTitle) {
      this._CodingReport += "<td class='border border-dark'>" + currentItem.title + "</td>";
    }
    this.AddQuestionCodingToReport(nodesToReportOn, options);
    this._CodingReport += "</tr>";
    //console.log("end of AddToQuickQuestionReport");
  }
  AddQuestionCodingToReport(nodesToReportOn: singleNode[], options: QuickQuestionReportOptions) {
    for (let node of nodesToReportOn) {
      this._CodingReport += "<td class='border border-dark'>";
      let ChildrenIds: number[] = [];
      for (let aNode of node.attributes) {
        ChildrenIds.push((aNode as SetAttribute).attribute_id);
      }
      for (let itemSet of this._ItemCodingList.filter(found => found.isCompleted)) {
        for (let roia of itemSet.itemAttributesList) {
          if (ChildrenIds.indexOf(roia.attributeId) > -1) {
            //this itemSet contains a child of this node, report it:
            let fNode = node.attributes.find(found => found.id == "A" + roia.attributeId);
            if (fNode) {
              this._CodingReport += "-";
              if (roia.armId > 0) {
                this._CodingReport += fNode.name
                  + (options.ShowCodeIds ? "(" + roia.attributeId + ")" : "")
                  + " [<span class='alert-info small'>" + roia.armTitle + "</span>]";
              }
              else this._CodingReport += fNode.name + (options.ShowCodeIds ? "(" + roia.attributeId + ")" : "");
              if (options.ShowInfobox && roia.additionalText && roia.additionalText.length > 0)
                this._CodingReport += "<br style='mso-data-placement:same-cell;' /><i class='small'>" + roia.additionalText.replace("\n", "<br style='mso-data-placement:same-cell;' />") + "</i>";
              if (options.ShowCodedText && roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
                this._CodingReport += this.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails);
              }
              this._CodingReport += "<br style='mso-data-placement:same-cell;' />";
            }
          }
        }
      }
      if (this._CodingReport.endsWith("<br style='mso-data-placement:same-cell;' />")) this._CodingReport = this._CodingReport.substring(0, this._CodingReport.length - 44);
      this._CodingReport += "</td>";
    }
  }

  public Save() {
    //nope! We're not saving this to localstorage:
    //item coding comes and goes (as we change items), so best not to keep it, if needed we'll re-fetch it.

    //if (this._ItemCodingList.length > 0)
    //    localStorage.setItem('ItemCodingList', JSON.stringify(this._ItemCodingList));
    //else if (localStorage.getItem('ItemCodingList'))//to be confirmed!! 
    //    localStorage.removeItem('ItemCodingList');
  }
  public FindItemSetBySetId(DestSetId: number): ItemSet | null {
    //this is where somewhat complicated logic needs to happen. We need to replicate here the logic that decides if a new itemset is needed or not...
    let result: ItemSet | null = null;
    let CompletedSet = this._ItemCodingList.find(found => found.setId == DestSetId && found.isCompleted);
    if (CompletedSet != undefined) {
      //good we found a completed set, we should use it
      return CompletedSet;
    }
    let IncompleteSet = this._ItemCodingList.find(found => found.setId == DestSetId
      && !found.isCompleted
      && found.contactId == this.ReviewerIdentityService.reviewerIdentity.userId
    );
    if (IncompleteSet != undefined) {
      return IncompleteSet;
    }
    //old bugged code, could work only if the completed version was encountered first...
    //for (let itemSet of this._ItemCodingList) {
    //    if (itemSet.setId == DestSetId) {
    //        //we have an itemSet in the desired set: if complete, we'll use it. Otherwise, check that it belongs to current user.
    //        //if itemset to be used is locked, we should not even have tried, so tricky case...
    //        if (itemSet.isCompleted) {
    //            if (itemSet.isLocked) {
    //                //alert('Coding is locked! We shouldn\'t be doing this...');
    //                //throw new Error('Coding is locked! We shouldn\'t be doing this...');
    //            }
    //            result = itemSet;
    //            break;
    //        }
    //        else if (itemSet.contactId == this.ReviewerIdentityService.reviewerIdentity.userId) {
    //            if (itemSet.isLocked) {
    //                //alert('Coding is locked! We shouldn\'t be doing this...');
    //                //throw new Error('Coding is locked! We shouldn\'t be doing this...');
    //            }
    //            result = itemSet;
    //            break;
    //        }
    //    }
    //}
    return result;
  }
  public FindROItemAttributeByAttribute(Att: SetAttribute): ReadOnlyItemAttribute | null {
    let set = this.FindItemSetBySetId(Att.set_id);
    if (!set) return null;
    for (let ROatt of set.itemAttributesList) {
      if (ROatt.attributeId == Att.attribute_id && ROatt.armId == Att.armId) return ROatt;
    }
    return null;
  }
  public markBusyBuildingHighlights(): void {
    this._BusyMethods.push("BuildingHighlights");
  }
  public removeBusyBuildingHighlights(): void {
    this.RemoveBusy("BuildingHighlights");
  }
  public FindItemSetByItemSetId(ItemSetId: number): ItemSet | null {
    let result: ItemSet | null = null;
    let ind = this._ItemCodingList.findIndex(found => found.itemSetId == ItemSetId);
    if (ind != -1) return this._ItemCodingList[ind];
    else return result;
  }
  public FetchAllFullTextData(itemid: number): Promise<boolean> {
    this._BusyMethods.push("FetchAllFullTextData");
    return lastValueFrom(this._httpC.post<ItemAttributeFullTextDetails[]>(
      this._baseUrl + 'api/Comparisons/ItemAttributesFullTextData',
      itemid
    )).then(
      (res: ItemAttributeFullTextDetails[]) => {
        //let fullText: object[] = [];
        if (res != null) {
          //console.log("got ItemAttributeFullTextDetails", res);
          for (let iaFT of res) {
            let ItSet = this.ItemCodingList.find(found => found.itemSetId == iaFT.itemSetId);
            if (ItSet != undefined) {
              //console.log("got ItSet", ItSet);
              let ItmAtt = ItSet.itemAttributesList.find(found => found.itemAttributeId == iaFT.itemAttributeId);
              if (ItmAtt != undefined) {
                //console.log("got ItmAtt", ItmAtt);
                let iaFTdetails = ItmAtt.itemAttributeFullTextDetails.find(found => found.itemAttributeTextId == iaFT.itemAttributeTextId);
                if (iaFTdetails == undefined) ItmAtt.itemAttributeFullTextDetails.push(iaFT);
              }
            }
          }
        }
        this.RemoveBusy("FetchAllFullTextData");
        return true;
      }, (error) => {
        this.RemoveBusy("FetchAllFullTextData");
        console.log("Error in FetchAllFullTextData", error);
        this.modalService.GenericErrorMessage("Sorry could not fetch the full-text data for the selected coding. Report is aborting.");
        return false;
      }
    ).catch((caught) => {
      this.RemoveBusy("FetchAllFullTextData");
      this.modalService.GenericErrorMessage("Sorry could not fetch the full-text data for the selected coding. Report is aborting.");
      console.log("Catch in FetchAllFullTextData", caught);
      return false;
    });
  }

  public async FetchCurrentQuickCodingReportAllPages(crit: Criteria, ReviewSetsToReportOn: ReviewSet[], isJson: boolean) {
    let interimCrit: Criteria = crit.Clone(); //we are going to use this criteria by changing page...
    this._stopQuickReport = false;
    this._BusyMethods.push("FetchCodingReportAllPages");
    let counter: number = 0;
    while (this.stopQuickReport == false && counter < 10000 && counter < this.ItemListService.ItemList.pagecount * 2) {//10000 cycles max, but not forever!
      counter++;
      this.ngZone.run(() => this.IsBusy);
      await this.FetchCurrentQuickCodingReportPage(interimCrit, ReviewSetsToReportOn, isJson, true);
    }
    if (counter >= 10000 || counter >= this.ItemListService.ItemList.pagecount * 2) {
      //we aborted, should tell the user
      this.modalService.GenericErrorMessage("Sorry, fetching of the coding report has been interrupted as it surpassed maximum allowed number of requests for data.");
    }
    this.RemoveBusy("FetchCodingReportAllPages");
    this.ngZone.run(() => this.IsBusy);
  }

  public async FetchCurrentQuickCodingReportPage(crit: Criteria, ReviewSetsToReportOn: ReviewSet[], isJson: boolean, multiplePages: boolean = false) {
    if (multiplePages == false || crit.pageNumber == 0) {
      this._CodingReport = "";
      this.jsonReport = new JsonReport();
    }
    console.log("FetchCurrentQuickCodingReportPage: ", crit.pageNumber, crit.pageSize, ReviewSetsToReportOn.length);
    this._ItemsToReport = [];
    this._ReviewSetsToReportOn = [];
    this._PerItemReport = false;
    //if (this.SelfSubscription4QuickCodingReport) {
    //    this.SelfSubscription4QuickCodingReport.unsubscribe();
    //    this.SelfSubscription4QuickCodingReport = null;
    //}
    this._CurrentItemIndex4QuickCodingReport = crit.pageNumber;

    //this.stopQuickReport = false;
    let sets = ReviewSetsToReportOn.map(el => { return el.set_id; })
      .join(',');
    //console.log("sets:", sets);
    if (sets.length < 0) return;
    this._BusyMethods.push("FetchCodingReportPage");
    //this._ItemsToReport = Items;
    this._ReviewSetsToReportOn = ReviewSetsToReportOn;
    if (isJson && this._ReviewSetsToReportOn) {
      let cSets: ReviewSet4ER4Json[] = [];
      for (let rs of this._ReviewSetsToReportOn) {
        let setForReport = new ReviewSet4ER4Json(rs);
        //console.log("adding this set to report", setForReport);
        cSets.push(setForReport);
      }
      this.jsonReport.CodeSets = cSets;
    }
    const Data = await this.FetchCodingReportDataPage(crit, sets);
    let LastTimeRound: boolean = false;
    if (Data == null || Data.totalItemCount == -1) {
      //this was an error, didn't work, abort.
      console.log("Error in fetching data, we'll abort");
      this.stopQuickReport = true;//setting it like this wipes out the current results...
      LastTimeRound = true;
      this.RemoveBusy("FetchCodingReportPage");
      //return;
    }
    else if (multiplePages && crit.pageNumber < Data.pageCount - 1) {
      crit.pageNumber += 1;
    } else {
      //last (or only) time round
      this.RemoveBusy("FetchCodingReportPage");
      LastTimeRound = true;
    }
    //console.log("Report data:", Data, crit);

    //we put data in the report in here, user might have cancelled while we were fetching data, as it could take several seconds...
    if (this._stopQuickReport == false) {
      this._CurrentItemIndex4QuickCodingReport = 0;
      for (const itm of Data.items) {
        //console.log("Reporting on item", itm);
        this._ItemsToReport = [itm];
        this._ItemCodingList = Data.itemSets.filter(found => found.itemId == itm.itemId).map(im => { return new ItemSet(im); });

        //console.log("Reporting on item (Item id, len):", this._ItemsToReport[0].itemId, this._ItemCodingList.length);
        if (isJson) this.AddToJSONQuickCodingReport();
        else this.AddToQuickCodingReport();
      }
      this._ItemsToReport = [];
    }
    if (LastTimeRound) {
      //signal that we are stopping, without wiping what we've collected so far!
      this._stopQuickReport = true;
    }
    this.RemoveBusy("FetchCodingReportPage");
  }

  private async FetchCodingReportDataPage(crit: Criteria, sets: string): Promise<iQuickCodingReportData> {
    this._BusyMethods.push("FetchCodingReportDataPage");
    let criteria: QuickCodingReportDataSelectionCriteria = new QuickCodingReportDataSelectionCriteria(crit, sets);
    this._CurrentItemAttPDFCoding = new ItemAttPDFCoding();
    const ErrResult: iQuickCodingReportData = {
      pageSize: -1,
      pageCount: -1,
      pageIndex: -1,
      totalItemCount: -1,
      items: [],
      itemSets: []
    };
    return lastValueFrom(this._httpC.post<iQuickCodingReportData>(this._baseUrl + 'api/ItemSetList/FetchQuickCodingReportPage',
      criteria)).then(
        result => {
          this.RemoveBusy("FetchCodingReportDataPage");
          this.ngZone.run(() => this.IsBusy);
          return result;
        },
        error => {
          this.RemoveBusy("FetchCodingReportDataPage");
          this.ngZone.run(() => this.IsBusy);
          this.modalService.GenericError(error);
          return ErrResult;
        }
      ).catch(
        caught => {
          this.RemoveBusy("FetchCodingReportDataPage");
          this.ngZone.run(() => this.IsBusy);
          this.modalService.GenericError(caught);
          return ErrResult;
        });
  }
}


export interface iItemSet {
  itemSetId: number;
  setId: number;
  itemId: number;
  contactId: number;
  contactName: string;
  setName: string;
  isCompleted: boolean;
  isLocked: boolean;
  itemAttributesList: ReadOnlyItemAttribute[];
  isSelected: boolean;
  outcomeItemList: OutcomeItemList;//this is stale data that shouldn't be normally used!
}

export class ItemSet {
  public constructor(iISet?: iItemSet) {
    if (iISet) {
      this.itemSetId = iISet.itemSetId;
      this.setId = iISet.setId;
      this.itemId = iISet.itemId;
      this.contactId = iISet.contactId;
      this.contactName = iISet.contactName;
      this.setName = iISet.setName;
      this.isCompleted = iISet.isCompleted;
      this.isLocked = iISet.isLocked;
      this.itemAttributesList = iISet.itemAttributesList;
      this.isSelected = iISet.isSelected;
      this.OutcomeList = [];
      if (iISet.outcomeItemList.outcomesList) {
        for (let iO of iISet.outcomeItemList.outcomesList) {
          let RealOutcome: Outcome = new Outcome(iO);
          this.OutcomeList.push(RealOutcome);
        }
      }
    }
  }
  itemSetId: number = 0;
  setId: number = 0;
  itemId: number = 0;
  contactId: number = 0;
  contactName: string = "";
  setName: string = "";
  isCompleted: boolean = false;
  isLocked: boolean = true;
  itemAttributesList: ReadOnlyItemAttribute[] = [];
  isSelected: boolean = false;
  //outcomeItemList: OutcomeItemList = new OutcomeItemList();//this is stale data that shouldn't be normally used!
  OutcomeList: Outcome[] = [];
}
export class ReadOnlyItemAttribute {
  attributeId: number = 0;
  itemAttributeId: number = 0;
  additionalText: string = "";
  armId: number = 0;
  armTitle: string = "";
  itemAttributeFullTextDetails: ItemAttributeFullTextDetails[] = [];
}

export interface ItemAttributeFullTextDetails {
  itemDocumentId: number;
  isFromPDF: boolean;
  itemArm: string;
  docTitle: string;
  text: string;
  textTo: number;
  textFrom: number;
  itemSetId: number;
  itemAttributeId: number;
  itemAttributeTextId: number;
}

export class QuickQuestionReportOptions {
  IncludeFullTitle: boolean = false;
  ShowInfobox: boolean = true;
  ShowCodedText: boolean = true;
  ShowCodeIds: boolean = false;
}

export class ItemAttPDFCodingCrit {
  constructor(itemDocId: number, itemAttId: number) {
    this.itemDocumentId = itemDocId;
    this.itemAttributeId = itemAttId;
  }
  public itemDocumentId: number = 0;
  public itemAttributeId: number = 0;
}
export class ItemAttPDFCoding {
  public Criteria: ItemAttPDFCodingCrit = new ItemAttPDFCodingCrit(0, 0);
  public ItemAttPDFCoding: ItemAttributePDF[] | null = null;
}


export class ItemAttributePDF {
  inPageSelections: InPageSelection[] = [];
  itemAttributeId: number = 0;
  itemAttributePDFId: number = 0;
  itemDocumentId: number = 0;
  page: number = 0;
  shapeTxt: string = "";
  pdfTronXml: string = "";
}

export class InPageSelection {
  start: number = 0;
  end: number = 0;
  selTxt: string = "";
}
export class CSLAInPageSelection {
  Start: number = 0;
  End: number = 0;
  SelTxt: string = "";
}

export class MVCiaPDFCodingPage {
  public ItemAttributePDFId: number = 0;
  public ItemDocumentId: number = 0;
  public ItemAttributeId: number = 0;
  public ShapeTxt: string = "";
  public InPageSelections: CSLAInPageSelection[] = [];
  public Page: number = 0;
  public PdfTronXml: string = "";
  public CreateInfo: ItemAttributeSaveCommand | null = null;
}
export interface iCreatePDFCodingPageResult {
  createInfo: ItemAttributeSaveCommand;
  iaPDFpage: ItemAttributePDF;
}
class ReviewSet4ER4Json {
  constructor(rs: ReviewSet) {
    this.SetName = rs.set_name;
    this.SetId = rs.set_id;
    this.ReviewSetId = rs.reviewSetId;
    this.SetDescription = rs.description;
    this.SetType = new SetType4ER4Json(rs.setType);
    this.Attributes = new AttributesList4ER4Json(rs.attributes);
  }
  SetName: string;
  ReviewSetId: number;
  SetId: number;
  SetType: SetType4ER4Json;
  SetDescription: string;
  Attributes: AttributesList4ER4Json;
}
class SetType4ER4Json {
  constructor(type: iSetType) {
    this.SetTypeDescription = type.setTypeDescription;
    this.SetTypeName = type.setTypeName;
    this.SetTypeId = type.setTypeId;
  }
  SetTypeName: string;
  SetTypeId: number;
  SetTypeDescription: string;
}
class AttributesList4ER4Json {
  constructor(atts: SetAttribute[]) {
    this.AttributesList = [];
    for (let a of atts) {
      this.AttributesList.push(new SetAttribute4ER4Json(a));
    }
  }
  AttributesList: SetAttribute4ER4Json[];

}
class SetAttribute4ER4Json {
  constructor(att: SetAttribute) {
    this.AttributeSetId = att.attributeSetId;
    this.AttributeId = att.attribute_id;
    this.AttributeSetDescription = att.attribute_set_desc;
    this.AttributeType = att.attribute_type;
    this.AttributeTypeId = att.attribute_type_id;
    this.AttributeName = att.attribute_name;
    this.ExtURL = att.extURL;
    this.ExtType = att.extType;
    this.OriginalAttributeID = att.originalAttributeID;
    this.Attributes = new AttributesList4ER4Json(att.attributes);
  }
  AttributeSetId: number;
  AttributeId: number;
  AttributeSetDescription: string;
  AttributeType: string;
  AttributeTypeId: number;
  AttributeName: string;
  AttributeDescription: string = "";
  ExtURL: string = "";
  ExtType: string = "";
  OriginalAttributeID: number = 0;
  Attributes: AttributesList4ER4Json;
}
class Item4ER4Json {
  constructor(item: Item) {
    this.ItemId = item.itemId;
    this.Title = item.title;
    this.ParentTitle = item.parentTitle;
    this.ShortTitle = item.shortTitle;
    this.DateCreated = item.dateCreated;
    this.CreatedBy = item.createdBy;
    this.DateEdited = item.dateEdited;
    this.EditedBy = item.editedBy;
    this.Year = item.year;
    this.Month = item.month;
    this.StandardNumber = item.standardNumber;
    this.City = item.city;
    this.Country = item.country;
    this.Publisher = item.publisher;
    this.Institution = item.institution;
    this.Volume = item.volume;
    this.Pages = item.pages;
    this.Edition = item.edition;
    this.Issue = item.issue;
    this.Availability = item.availability;
    this.URL = item.url;
    this.OldItemId = item.oldItemId;
    this.Abstract = item.abstract;
    this.Comments = item.comments;
    this.TypeName = item.typeName;
    this.Authors = item.authors;
    this.ParentAuthors = item.parentAuthors;
    this.DOI = item.doi;
    this.Keywords = item.keywords;
    this.ItemStatus = item.itemStatus;
    this.ItemStatusTooltip = item.itemStatusTooltip;
    this.QuickCitation = item.quickCitation;
  }

  ItemId: number;
  Title: string;
  ParentTitle: string;
  ShortTitle: string;
  DateCreated: string;
  CreatedBy: string;
  DateEdited: string;
  EditedBy: string;
  Year: string;
  Month: string;
  StandardNumber: string;
  City: string;
  Country: string;
  Publisher: string;
  Institution: string;
  Volume: string;
  Pages: string;
  Edition: string;
  Issue: string;
  Availability: string;
  URL: string;
  OldItemId: string;
  Abstract: string;
  Comments: string;
  TypeName: string;
  Authors: string;
  ParentAuthors: string;
  DOI: string;
  Keywords: string;
  ItemStatus: string;
  ItemStatusTooltip: string;
  Codes: Attribute4ER4Json[] = [];
  Outcomes: Outcome4ER4Json[] = [];
  QuickCitation: string;
}
class Outcome4ER4Json {
  constructor(o: Outcome) {
    this.OutcomeId = o.outcomeId;
    this.ItemSetId = o.itemSetId;
    this.OutcomeTypeId = o.outcomeTypeId;
    this.OutcomeTypeName = o.outcomeTypeName;
    this.ItemAttributeIdIntervention = o.itemAttributeIdIntervention;
    this.ItemAttributeIdControl = o.itemAttributeIdControl;
    this.ItemAttributeIdOutcome = o.itemAttributeIdOutcome;
    this.Title = o.title;
    this.ItemTimepointId = o.itemTimepointId;
    this.ItemTimepointMetric = o.itemTimepointMetric;
    this.ItemTimepointValue = o.itemTimepointValue;
    this.TimepointDisplayValue = o.timepointDisplayValue;
    this.ItemArmIdGrp1 = o.itemArmIdGrp1;
    this.ItemArmIdGrp2 = o.itemArmIdGrp2;
    this.grp1ArmName = o.grp1ArmName;
    this.grp2ArmName = o.grp2ArmName;
    this.ShortTitle = o.shortTitle;
    this.OutcomeDescription = o.outcomeDescription;
    this.Data1 = o.data1;
    this.Data2 = o.data2;
    this.Data3 = o.data3;
    this.Data4 = o.data4;
    this.Data5 = o.data5;
    this.Data6 = o.data6;
    this.Data7 = o.data7;
    this.Data8 = o.data8;
    this.Data9 = o.data9;
    this.Data10 = o.data10;
    this.Data11 = o.data11;
    this.Data12 = o.data12;
    this.Data13 = o.data13;
    this.Data14 = o.data14;
    this.InterventionText = o.interventionText;
    this.ControlText = o.controlText;
    this.OutcomeText = o.outcomeText;
    this.feWeight = o.feWeight;
    this.reWeight = o.reWeight;
    this.SMD = o.smd;
    this.SESMD = o.sesmd;
    this.R = o.r;
    this.SER = o.ser;
    this.OddsRatio = o.oddsRatio;
    this.SEOddsRatio = o.seOddsRatio;
    this.RiskRatio = o.riskRatio;
    this.SERiskRatio = o.seRiskRatio;
    this.CIUpperSMD = o.ciUpperSMD;
    this.CILowerSMD = o.ciLowerSMD;
    this.CIUpperR = o.ciUpperR;
    this.CILowerR = o.ciLowerR;
    this.CIUpperOddsRatio = o.ciUpperOddsRatio;
    this.CILowerOddsRatio = o.ciLowerOddsRatio;
    this.CIUpperRiskRatio = o.ciUpperRiskRatio;
    this.CILowerRiskRatio = o.ciLowerRiskRatio;
    this.CIUpperRiskDifference = o.ciUpperRiskDifference;
    this.CILowerRiskDifference = o.ciLowerRiskDifference;
    this.CIUpperPetoOddsRatio = o.ciUpperPetoOddsRatio;
    this.CILowerPetoOddsRatio = o.ciLowerPetoOddsRatio;
    this.CIUpperMeanDifference = o.ciUpperMeanDifference;
    this.CILowerMeanDifference = o.ciLowerMeanDifference;
    this.RiskDifference = o.riskDifference;
    this.SERiskDifference = o.seRiskDifference;
    this.MeanDifference = o.meanDifference;
    this.SEMeanDifference = o.seMeanDifference;
    this.PetoOR = o.petoOR;
    this.SEPetoOR = o.sePetoOR;
    this.ES = o.es;
    this.SEES = o.sees;
    this.NRows = o.nRows;
    this.CILower = o.ciLower;
    this.CIUpper = o.ciUpper;
    this.ESDesc = o.esDesc;
    this.SEDesc = o.seDesc;
    this.Data1Desc = o.data1Desc;
    this.Data2Desc = o.data2Desc;
    this.Data3Desc = o.data3Desc;
    this.Data4Desc = o.data4Desc;
    this.Data5Desc = o.data5Desc;
    this.Data6Desc = o.data6Desc;
    this.Data7Desc = o.data7Desc;
    this.Data8Desc = o.data8Desc;
    this.Data9Desc = o.data9Desc;
    this.Data10Desc = o.data10Desc;
    this.Data11Desc = o.data11Desc;
    this.Data12Desc = o.data12Desc;
    this.Data13Desc = o.data13Desc;
    this.Data14Desc = o.data14Desc;
    this.OutcomeCodes = new OutcomeItemAttributesList4Json(o.outcomeCodes);
  }
  OutcomeId: number;
  ItemSetId: number;
  OutcomeTypeId: number;
  OutcomeTypeName: string;
  ItemAttributeIdIntervention: number;
  ItemAttributeIdControl: number;
  ItemAttributeIdOutcome: number;
  Title: string;
  ItemTimepointId: number;
  ItemTimepointMetric: string;
  ItemTimepointValue: string;
  TimepointDisplayValue: string;
  ItemArmIdGrp1: number;
  ItemArmIdGrp2: number;
  grp1ArmName: string;
  grp2ArmName: string;
  ShortTitle: string;
  OutcomeDescription: string;
  Data1: number;
  Data2: number;
  Data3: number;
  Data4: number;
  Data5: number;
  Data6: number;
  Data7: number;
  Data8: number;
  Data9: number;
  Data10: number;
  Data11: number;
  Data12: number;
  Data13: number;
  Data14: number;
  InterventionText: string;
  ControlText: string;
  OutcomeText: string;
  feWeight: number;
  reWeight: number;
  SMD: number;
  SESMD: number;
  R: number;
  SER: number;
  OddsRatio: number;
  SEOddsRatio: number;
  RiskRatio: number;
  SERiskRatio: number;
  CIUpperSMD: number;
  CILowerSMD: number;
  CIUpperR: number;
  CILowerR: number;
  CIUpperOddsRatio: number;
  CILowerOddsRatio: number;
  CIUpperRiskRatio: number;
  CILowerRiskRatio: number;
  CIUpperRiskDifference: number;
  CILowerRiskDifference: number;
  CIUpperPetoOddsRatio: number;
  CILowerPetoOddsRatio: number;
  CIUpperMeanDifference: number;
  CILowerMeanDifference: number;
  RiskDifference: number;
  SERiskDifference: number;
  MeanDifference: number;
  SEMeanDifference: number;
  PetoOR: number;
  SEPetoOR: number;
  ES: number;
  SEES: number;
  NRows: number;
  CILower: number;
  CIUpper: number;
  ESDesc: string;
  SEDesc: string;
  Data1Desc: string;
  Data2Desc: string;
  Data3Desc: string;
  Data4Desc: string;
  Data5Desc: string;
  Data6Desc: string;
  Data7Desc: string;
  Data8Desc: string;
  Data9Desc: string;
  Data10Desc: string;
  Data11Desc: string;
  Data12Desc: string;
  Data13Desc: string;
  Data14Desc: string;
  OutcomeCodes: OutcomeItemAttributesList4Json;
}
class OutcomeItemAttributesList4Json {
  constructor(oial: OutcomeItemAttributesList) {
    for (let oia of oial.outcomeItemAttributesList) {
      this.OutcomeItemAttributesList.push(new OutcomeItemAttribute4Json(oia));
    }
  }
  OutcomeItemAttributesList: OutcomeItemAttribute4Json[] = [];
}
class OutcomeItemAttribute4Json {
  constructor(o: OutcomeItemAttribute) {
    this.AdditionalText = o.additionalText;
    this.AttributeId = o.attributeId;
    this.AttributeName = o.attributeName;
    this.OutcomeId = o.outcomeId;
    this.OutcomeItemAttributeId = o.outcomeItemAttributeId;
  }
  OutcomeItemAttributeId: number;
  OutcomeId: number;
  AttributeId: number;
  AdditionalText: string;
  AttributeName: string;
}
class Attribute4ER4Json {
  constructor(ia: ReadOnlyItemAttribute) {
    this.AttributeId = ia.attributeId;
    this.AdditionalText = ia.additionalText;
    this.ArmId = ia.armId;
    this.ArmTitle = ia.armTitle;
    this.ItemAttributeFullTextDetails = [];
    for (let ft of ia.itemAttributeFullTextDetails) {
      this.ItemAttributeFullTextDetails.push(new ItemAttributeFullTextDetails4ER4Json(ft));
    }
  }
  AttributeId: number;
  AdditionalText: string;
  ArmId: number;
  ArmTitle: string;
  ItemAttributeFullTextDetails: ItemAttributeFullTextDetails4ER4Json[];
}
class ItemAttributeFullTextDetails4ER4Json {
  constructor(ft: ItemAttributeFullTextDetails) {
    this.ItemDocumentId = ft.itemDocumentId;
    this.TextFrom = ft.textFrom;
    this.TextTo = ft.textTo;
    this.Text = ft.text;
    this.IsFromPDF = ft.isFromPDF;
    this.DocTitle = ft.docTitle;
    this.ItemArm = ft.itemArm;
  }
  ItemDocumentId: number;
  TextFrom: number;
  TextTo: number;
  Text: string;
  IsFromPDF: boolean;
  DocTitle: string;
  ItemArm: string;
}
class JsonReport {
  CodeSets: ReviewSet4ER4Json[] = [];
  References: Item4ER4Json[] = [];
}
class QuickCodingReportDataSelectionCriteria {
  constructor(c: Criteria, sets: string) {
    this.itemsSelectionCriteria = c;
    this.setIds = sets;
  }
  public itemsSelectionCriteria: Criteria = new Criteria();
  public setIds: string = "";
}

interface iQuickCodingReportData {
  pageSize: number;
  pageCount: number;
  pageIndex: number;
  totalItemCount: number;
  items: Item[];
  itemSets: iItemSet[];
}
class OutcomeSimilaritiesKeyValue {//used in more than one place...
  constructor(k: Outcome, v: number) {
    this.key = k;
    this.value = v;
  }
  key: Outcome;
  value: number;
}
