import { Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { iArm } from './ArmTimepointLinkList.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { Helpers } from '../helpers/HelperMethods';
import { ReadOnlySource } from './sources.service';
import { EventEmitterService } from './EventEmitter.service';
import { iTimePoint } from './ArmTimepointLinkList.service';
import { ConfigService } from './config.service';
import { LastValueFromConfig } from 'rxjs/internal/lastValueFrom';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';



@Injectable({

  providedIn: 'root',

}
)

export class ItemListService extends BusyAwareService implements OnDestroy {

  private _itemListOptions: ItemListOptions = new ItemListOptions();
  constructor(
    private _httpC: HttpClient,
    configService: ConfigService,
    private EventEmitterService: EventEmitterService,
    private ModalService: ModalService
  ) {
    super(configService);
    //console.log("On create ItemListService");
    this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });

  }
  ngOnDestroy() {
    console.log("Destroy MAGRelatedRunsService");
    if (this.clearSub != null) this.clearSub.unsubscribe();
  }
  private clearSub: Subscription | null = null;

  public get GetListItemOptions(): ItemListOptions {
    return this._itemListOptions;
  }
  private _IsInScreeningMode: boolean | null = null;
  public get IsInScreeningMode(): boolean {
    if (this._IsInScreeningMode !== null) return this._IsInScreeningMode;
    else return false;
  }
  public set IsInScreeningMode(state: boolean) {
    this._IsInScreeningMode = state;
    //this.Save();
  }

  private _ItemList: ItemList = new ItemList();
  private _Criteria: Criteria = new Criteria();
  private _currentItem: Item = new Item();
  private _ItemTypes: any[] = [];
  public get ItemTypes(): any[] {
    //console.log("Get ItemTypes");
    return this._ItemTypes;
  }
  public ListDescription: string = "";
  @Output() ItemChanged = new EventEmitter();
  @Output() ListChanged = new EventEmitter();
  @Output() ReconcileListChanged = new EventEmitter();
  public get ItemList(): ItemList {
    return this._ItemList;
  }
  public get ListCriteria(): Criteria {
    return this._Criteria;
  }
  public get currentItem(): Item {
    return this._currentItem;
  }
  private _CurrentItemAdditionalData: iAdditionalItemDetails | null = null;
  public get CurrentItemAdditionalData(): iAdditionalItemDetails | null {
    if (!this._currentItem || !this._CurrentItemAdditionalData) return null;
    else if (this._currentItem.itemId !== this._CurrentItemAdditionalData.itemID) return null;
    else return this._CurrentItemAdditionalData;
  }

  public FetchWithCrit(crit: Criteria, listDescription: string, save: boolean = true)/*: Promise<ItemList | boolean>*/ {
    this._BusyMethods.push("FetchWithCrit");
    if (save) {
      this._Criteria = crit;
      if (this._ItemList && this._ItemList.pagesize > 0 && this._ItemList.pagesize <= 4000 && this._ItemList.pagesize != crit.pageSize
      ) {
        crit.pageSize = this._ItemList.pagesize;
      }
      // console.log("FetchWithCrit", this._Criteria.listType);
      this.ListDescription = listDescription;
    }
    return lastValueFrom(this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/Fetch', crit))
      .then(
        list => {
          this._Criteria.totalItems = this.ItemList.totalItemCount;
          if (save) {
            this.SaveItems(list, this._Criteria);
            if (this._itemListOptions.showInfo == false && this._Criteria.showInfoColumn == true) this._itemListOptions.showInfo = true;
            if (this._itemListOptions.showScore == false && this._Criteria.showScoreColumn == true) this._itemListOptions.showScore = true;
            this.ListChanged.emit();
          }
          this.RemoveBusy("FetchWithCrit");
          return list;
          //console.log('aksdjh: CHEKC: ', JSON.stringify(this.ItemList.items.length));
        }, error => {
          this.ModalService.GenericError(error);
          this.RemoveBusy("FetchWithCrit");
          return false;
        }
      ).catch(caught => {
        this.ModalService.GenericErrorMessage(caught.toString());
        this.RemoveBusy("FetchWithCrit");
        return false;
      });
  }

  public FetchWithCritReconcile(crit: Criteria, listDescription: string) {
    this._BusyMethods.push("FetchWithCritReconcile");
    this._Criteria = crit;
    if (this._ItemList && this._ItemList.pagesize > 0
      && this._ItemList.pagesize <= 4000
      && this._ItemList.pagesize != crit.pageSize
    ) {
      crit.pageSize = this._ItemList.pagesize;
    }

    this.ListDescription = listDescription;
    this._httpC.post<ItemList>(this._baseUrl + 'api/ItemList/Fetch', crit)
      .subscribe(
        list => {
          this._Criteria.totalItems = this.ItemList.totalItemCount;
          console.log();
          this.SaveItems(list, this._Criteria);
          this.ReconcileListChanged.emit();
        }, error => {
          this.ModalService.GenericError(error);
          this.RemoveBusy("FetchWithCritReconcile");
        }
        , () => { this.RemoveBusy("FetchWithCritReconcile"); }
      );
  }
  public Refresh() {
    if (this._Criteria && this._Criteria.listType && this._Criteria.listType != "") {
      //we have something to do
      this.FetchWithCrit(this._Criteria, this.ListDescription);
    }
  }
  public FetchItemTypes() {
    this._BusyMethods.push("FetchItemTypes");
    this._httpC.get<StringKeyValue[]>(this._baseUrl + 'api/ItemList/ItemTypes')
      .subscribe(
        (res) => {
          this.RemoveBusy("FetchItemTypes");
          //putting the "journal" type close to the top...
          let i = res.findIndex(found => found.key == '14');
          if (i > -1) {
            let j = res.splice(i, 1);
            res.splice(1, 0, j[0]);
          }
          this._ItemTypes = res;
          //console.log(res);
        }
        , (err) => {
          this.RemoveBusy("FetchItemTypes");
          this.ModalService.GenericError(err);
        }
      );
  }






  public UpdateItem(item: Item) {
    this._BusyMethods.push("UpdateItem");
    this._httpC.post<Item>(this._baseUrl + 'api/ItemList/UpdateItem', item)
      .subscribe(
        result => {
          //if we get an item back, put it in the list substituting it via itemID
          if (item.itemId == 0) {
            //we created a new item, add to current list, so users can see it immediately...
            //this._currentItem = result;//not sure we need this...
            this.ItemList.items.push(result);
          }
          else {
            //try to replace item in current list. We use the client side object 'cause the typename might otherwise be wrong.
            let i = this.ItemList.items.findIndex(found => found.itemId == item.itemId);
            if (i !== -1) {
              //console.log("replacing updated item.", this.ItemList.items[i]);
              this.ItemList.items[i] = item;
              console.log("replaced updated item.");//, this.ItemList.items[i]);
            }
            else {
              console.log("updated item not replaced: could not find it...");
            }
          }
          this.RemoveBusy("UpdateItem");
        }, error => {
          this.ModalService.GenericError(error);
          this.RemoveBusy("UpdateItem");
        }
        , () => { this.RemoveBusy("UpdateItem"); }
      );

  }
  public AssignDocumentsToIncOrExc(include: string, itemids: string,
    attributeid: number, setid: number): Promise<Item> {

    let body = JSON.stringify({
      include: include, itemids: itemids,
      attributeid: attributeid, setid: setid
    })
    let inc: boolean = false;
    if (include == 'true') {
      inc = true;
    } else {
      inc = false;
    }
    this._BusyMethods.push("AssignDocumentsToIncOrExc");
    return lastValueFrom(this._httpC.post<Item>(this._baseUrl + 'api/ItemList/AssignDocumentsToIncOrExc', body)
    ).then(
      (result) => {

        result.isIncluded = inc;
        result.isItemDeleted = false;
        this.RemoveBusy("AssignDocumentsToIncOrExc");
        return result;

      }, error => {
        this.ModalService.GenericError(error);
        this.RemoveBusy("AssignDocumentsToIncOrExc");
        return error;
      }
    );
  }

  public GetIncludedItems() {
    let cr: Criteria = new Criteria();
    cr.listType = 'StandardItemList';
    this.FetchWithCrit(cr, "Included Items");
  }
  public GetExcludedItems() {
    let cr: Criteria = new Criteria();
    cr.listType = 'StandardItemList';
    cr.onlyIncluded = false;
    this.FetchWithCrit(cr, "Excluded Items");
  }
  public GetDeletedItems() {
    let cr: Criteria = new Criteria();
    cr.listType = 'StandardItemList';
    cr.onlyIncluded = false;
    cr.showDeleted = true;
    this.FetchWithCrit(cr, "Deleted Items");
  }
  public GetCitationForExport(Item: Item) {

    let retVal: any;
    retVal = {
      Included: Item.itemStatus,
    };
    if (this.GetListItemOptions.showId) retVal["ID"] = Item.itemId;
    if (this.GetListItemOptions.showShortTitle) retVal["ShortTitle"] = Item.shortTitle;
    if (this.GetListItemOptions.showTitle) retVal["Title"] = Item.title;
    if (this.GetListItemOptions.showJournal) retVal["Journal"] = Item.parentTitle;
    if (this.GetListItemOptions.showInfo) retVal["Info"] = Item.attributeAdditionalText;
    if (this.GetListItemOptions.showImportedId) retVal["Your Id"] = Item.oldItemId;
    if (this.GetListItemOptions.showAuthors) retVal["Authors"] = Item.authors;
    if (this.GetListItemOptions.showYear) retVal["Year"] = Item.year;
    if (this.GetListItemOptions.showDocType) retVal["Ref. Type"] = Item.typeName;
    if (this.GetListItemOptions.showScore) retVal["Score"] = Item.rank;
    //console.log(retVal);
    return retVal;

  }
  public static GetHISCitationForExport(Item: Item) {

    let retVal: any;
    retVal = {};
    retVal["ID"] = Item.itemId;
    retVal["Short Title"] = Item.shortTitle;
    retVal["Authors"] = Item.authors;
    retVal["Year"] = Item.year;
    retVal["Full Title"] = Item.title;
    retVal["Abstract"] = Item.abstract;
    retVal["Journal"] = Item.parentTitle;
    retVal["Volume"] = Item.volume;
    retVal["Issue"] = Item.issue;
    retVal["Page(s)"] = Item.pages;
    retVal["Ref. Type"] = Item.typeName;
    retVal["Url"] = Item.url;
    retVal["DOI"] = Item.doi;
    retVal["Include (?)"] = "";
    retVal["Exclude (?)"] = "";
    retVal["Comments (if any)"] = "";
    return retVal;

  }

  public static GetCitation(Item: Item): string {
    let retVal: string = "";
    switch (Item.typeId) {
      case 1: //Report
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". <i>" + Item.title.replace(/</g, "&lt;") + "</i>. " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ". ";
        break;
      case 2: //Book, Whole
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". <i>" + Item.title.replace(/</g, "&lt;") + "</i>. " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
        break;
      case 3: //Book, Chapter
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". " + Item.title.replace(/</g, "&lt;") + ". In <i>" + Item.parentTitle.replace(/</g, "&lt;") + "</i>, edited by " + ItemListService.CleanAuthors(Item.parentAuthors) + ", " +
          Item.pages + ". " + Item.city + ": " + Item.publisher + ".";
        break;
      case 4: //Dissertation
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.edition.replace(/</g, "&lt;") + ", " + Item.institution.replace(/</g, "&lt;") + ".";
        break;
      case 5: //Conference Proceedings
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". " + Item.title.replace(/</g, "&lt;") + ". Paper presented at " + Item.parentTitle.replace(/</g, "&lt;") + ", " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
        break;
      case 6: //Document From Internet Site
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.publisher.replace(/</g, "&lt;") + ". " + Item.url +
          (Item.availability == "" ? "" : " [Accessed " + Item.availability.replace(/</g, "&lt;") + "] ") + ".";
        break;
      case 7: //Web Site
        retVal = ItemListService.CleanAuthors(Item.authors) + ". (" + Item.year + "). <i>" + Item.title.replace(/</g, "&lt;") + "</i>. " + Item.publisher.replace(/</g, "&lt;") + ". " + Item.url +
          (Item.availability == "" ? "" : " [Accessed " + Item.availability.replace(/</g, "&lt;") + "] ") + ".";
        break;
      case 8: //DVD, Video, Media
        retVal = "\"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.year + ". " + (Item.availability == "" ? "" : " [" + Item.availability.replace(/</g, "&lt;") + "] ") +
          Item.city + ": " + ItemListService.CleanAuthors(Item.authors) + ".";
        break;
      case 9: //Research project
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
        break;
      case 10: //Article In A Periodical
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". <i>" + Item.parentTitle.replace(/</g, "&lt;") + "</i> " + Item.volume + (Item.issue != "" ? "(" + Item.issue + ")" : "") + ":" + Item.pages + ".";
        break;
      case 11: //Interview
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". ";
        break;
      case 12: //Generic
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title.replace(/</g, "&lt;") + "\". " + Item.city.replace(/</g, "&lt;") + ": " + Item.publisher.replace(/</g, "&lt;") + ".";
        break;
      case 14: //Journal, Article
        retVal = ItemListService.CleanAuthors(Item.authors) + ". " + Item.year + ". \"" + Item.title + "\". <i>" + Item.parentTitle + "</i> " + Item.volume + (Item.issue != "" ? "(" + Item.issue + ")" : "") + ":" + Item.pages + ".";
        break;
    }
    //console.log("GetCitation for Item: ", Item, retVal);
    return retVal;
  }
  public static CleanAuthors(inputAuthors: string): string {
    if (inputAuthors != "") {
      inputAuthors = inputAuthors.replace(" ;", ",");
      inputAuthors = inputAuthors.replace(";", ",");
      inputAuthors = inputAuthors.replace(/</g, "&lt;");
      inputAuthors = inputAuthors.trim();
      if (inputAuthors.endsWith(',')) inputAuthors = inputAuthors.substring(0, inputAuthors.length - 1);
    }
    let commaCount = 0;
    for (let i = 0; i < inputAuthors.length; i++) if (inputAuthors[i] == ',') commaCount++;
    if (commaCount > 0) {
      let cI = inputAuthors.lastIndexOf(',');
      inputAuthors = inputAuthors.substring(0, cI) + " and" + inputAuthors.substring(cI + 1);//.(inputAuthors.LastIndexOf(",") + 1, " and");
    }
    return inputAuthors;
  }
  public static GetNICECitation(currentItem: Item): any {
    let retVal: string = "";

    switch (currentItem.typeId) {

      case 1: //Report
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". " + currentItem.city + ": " + currentItem.publisher + ", " + currentItem.pages;
        break;
      case 2: //Book, Whole
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". " + currentItem.city + ": " + currentItem.publisher;
        break;
      case 3: //Book, Chapter
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". In: " + this.CleanAuthors(currentItem.parentAuthors) + ", editors. " + currentItem.parentTitle + ". " + currentItem.city + ": " + currentItem.publisher + ", p" + currentItem.pages;
        break;
      case 4: //Dissertation
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". " + currentItem.edition + ", " + currentItem.institution + ".";
        break;
      case 5: //Conference Proceedings
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". In: " + currentItem.parentTitle + ", " + currentItem.city + ". " + currentItem.publisher + ", p" + currentItem.pages;
        break;
      case 6: //Document From Internet Site
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") <a href='" + currentItem.url + "'>" + currentItem.title + "</a>. " + currentItem.publisher;
        break;
      case 7: //Web Site
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") <a href='" + currentItem.url + "'>" + currentItem.title + "</a> " + (currentItem.availability == "" ? "" : " [online; accessed: " + currentItem.availability + "]");
        break;
      case 8: //DVD, Video, Media
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + (currentItem.availability == "" ? "" : " [online; accessed: " + currentItem.availability + "]");
        break;
      case 9: //Research project
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". " + currentItem.city + ": " + currentItem.publisher + ", ";
        break;
      case 10: //Article In A Periodical
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". " + this.CleanAuthors(currentItem.parentTitle) + " " + currentItem.volume + (currentItem.issue != "" ? "(" + currentItem.issue + ")" : "") + ", " + currentItem.pages;
        break;
      case 11: //Interview
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". ";
        break;
      case 12: //Generic
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". " + currentItem.city + ": " + currentItem.publisher;
        break;
      case 14: //Journal, Article
        retVal = this.CleanAuthors(currentItem.authors) + " (" + currentItem.year + ") " + currentItem.title + ". " + this.CleanAuthors(currentItem.parentTitle) + " " + currentItem.volume + (currentItem.issue != "" ? "(" + currentItem.issue + ")" : "") + ", " + currentItem.pages;
        break;
    }
    return retVal;
  }
  public static GetHarvardCitation(currentItem: Item): any {
    console.log('Current item: ', currentItem);
    let retVal: string = "";
    switch (currentItem.typeId) {
      case 1: //Report
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. " + currentItem.city + ": " + currentItem.publisher + ", pp." + currentItem.pages + ". " +
          (currentItem.url == "" ? "" : "Available at: ") + currentItem.url + ".";
        break;
      case 2: //Book, Whole
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. " + currentItem.city + ": " + currentItem.publisher + ".";
        break;
      case 3: //Book, Chapter
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). " + currentItem.title + ". In: " + this.CleanAuthors(currentItem.parentAuthors) + ", ed., <i>" + currentItem.parentTitle + ".</i> " + currentItem.city + ": " + currentItem.publisher + ", pp." + currentItem.pages + ".";
        break;
      case 4: //Dissertation
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. " + currentItem.edition + ". " + currentItem.institution + ".";
        break;
      case 5: //Conference Proceedings
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). " + currentItem.title + ". In: " + currentItem.parentTitle + ". " + currentItem.city + ": " + currentItem.publisher + ", pp." + currentItem.pages + ". " +
          (currentItem.url == "" ? "" : "Available at: ") + currentItem.url + ".";
        break;
      case 6: //Document From Internet Site
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. [online] " + currentItem.publisher + ". Available at: " + currentItem.url +
          (currentItem.availability == "" ? "" : " [Accessed: " + currentItem.availability + "] ") + ".";
        break;
      case 7: //Web Site
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. [online] " + currentItem.publisher + ". Available at: " + currentItem.url +
          (currentItem.availability == "" ? "" : " [Accessed: " + currentItem.availability + "] ") + ".";
        break;
      case 8: //DVD, Video, Media
        retVal = "<i>" + currentItem.title + "</i>. " + " (" + currentItem.year + "). " + (currentItem.availability == "" ? "" : " [" + currentItem.availability + "] ") +
          currentItem.city + ": " + this.CleanAuthors(currentItem.authors) + ".";
        break;
      case 9: //Research project
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. " + currentItem.city + ": " + currentItem.publisher + ".";
        break;
      case 10: //Article In A Periodical
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). " + currentItem.title + ". <i>" + this.CleanAuthors(currentItem.parentTitle) + "</i>, " + currentItem.volume + (currentItem.issue != "" ? "(" + currentItem.issue + ")" : "") + ", pp." + currentItem.pages + ".";
        break;
      case 11: //Interview
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. ";
        break;
      case 12: //Generic
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). <i>" + currentItem.title + "</i>. " + currentItem.city + ": " + currentItem.publisher + ".";
        break;
      case 14: //Journal, Article
        retVal = this.CleanAuthors(currentItem.authors) + ". (" + currentItem.year + "). " + currentItem.title + ". <i>" + this.CleanAuthors(currentItem.parentTitle) + "</i>, " + currentItem.volume + (currentItem.issue != "" ? "(" + currentItem.issue + ")" : "") + ", pp." + currentItem.pages + ".";
        break;
    }
    return retVal;
  }

  public static GetLinks(currentItem: Item, linksToShow: iItemLink[] | true, lastItemID: number): any {
    let retVal: string = "";
    let lastRowInTable: boolean = false;
    retVal += "<tr>";
    retVal += "<td>" + currentItem.itemId + "</td>"
    retVal += "<td>" + currentItem.shortTitle + "</td>"
    retVal += "<td>" + currentItem.title + "</td>"
    if (linksToShow != true) {
      for (var j = 0; j < linksToShow.length; j++) {
        let currentLink: iItemLink = linksToShow[j];
        if (currentLink.itemIdPrimary == lastItemID) {
          lastRowInTable = true;
        }
        if (j == 0) {
          if (currentLink.itemIdPrimary == currentLink.itemIdSecondary) {
            // the first row is also the master item (left over from ER3?) so get the rest from next row
            j += 1;
            let currentLink: iItemLink = linksToShow[1];
            retVal += "<td>" + currentLink.itemIdSecondary + ": " + currentLink.shortTitle + "</td>";
            retVal += "<td>" + currentLink.title + "</td>";
            retVal += "<td>" + currentLink.description + "</td>";
            retVal += "</tr>";
          }
          else {
            retVal += "<td>" + currentLink.itemIdSecondary + ": " + currentLink.shortTitle + "</td>";
            retVal += "<td>" + currentLink.title + "</td>";
            retVal += "<td>" + currentLink.description + "</td>";
            retVal += "</tr>";
          }
        }
        else {
          // subsequent links
          retVal += "<tr>";
          retVal += "<td></td>";
          retVal += "<td></td>";
          retVal += "<td></td>";
          retVal += "<td>" + currentLink.itemIdSecondary + ": " + currentLink.shortTitle + "</td>";
          retVal += "<td>" + currentLink.title + "</td>";
          retVal += "<td>" + currentLink.description + "</td>";
          retVal += "</tr>";
        }
      }
    }
    else {
      // no linked items to show
      retVal += "<td colspan='3'>No linked items</td>";
      retVal += "</tr>";
    }

    if (lastRowInTable == true) {
      retVal += "</table><p>&nbsp;</p>";
    }

    return retVal;
  }


  public FetchAdditionalItemDetails() {
    if (this._currentItem.itemId > 0) {
      this.FetchAdditionalItemDetailsAsync(this._currentItem.itemId).then(
        //using the "then((val)=> {...}) structure makes this method "fire and forget":
        //code inside {...} executes whenever FetchAdditionalItemDetailsAsync returns some "val"
        (val) => {
          if (typeof (val) != "boolean") {
            this._CurrentItemAdditionalData = val;
          }
        });
    }
  }


  public FetchAdditionalItemDetailsAsync(Id: number): Promise<iAdditionalItemDetails | boolean> {
    this._BusyMethods.push("FetchAdditionalItemDetailsAsync");
    let body = JSON.stringify({ Value: Id });

    return lastValueFrom(this._httpC.post<iAdditionalItemDetails>(this._baseUrl + 'api/ItemList/FetchAdditionalItemData',
      body))
      .then(
        (result) => {
          this.RemoveBusy("FetchAdditionalItemDetailsAsync");
          return result;
        }
        , (error) => {
          this.ModalService.GenericError(error);
          this.RemoveBusy("FetchAdditionalItemDetailsAsync");
          return false;
        }
      ).catch((caught) => {
        this.ModalService.GenericError(caught);
        this.RemoveBusy("FetchAdditionalItemDetailsAsync");
        return false;
      });
  }


  public async GetDuplicatesReport01(currentItem: Item, lastItemID: number): Promise<string> {
    let retVal: string = "";
    let res = await this.FetchAdditionalItemDetailsAsync(currentItem.itemId);

    if ((res != false) && (res != true)) {
      let lastRowInTable: boolean = false;
      let additionalDetails: iAdditionalItemDetails = res;

      retVal += "<tr style=\"vertical-align:top;\">";
      retVal += "<td>" + currentItem.itemId + "</td>";
      retVal += "<td>" + currentItem.typeName + "</td>";
      retVal += "<td>" + currentItem.shortTitle + "</td>";
      retVal += "<td>" + currentItem.title + "</td>";
      retVal += "<td>" + currentItem.parentTitle + "</td>";
      retVal += "<td>" + additionalDetails.source.source_Name + "</td>";

      if (additionalDetails.duplicates.length > 0) {
        retVal += "<td>";
        for (var j = 0; j < additionalDetails.duplicates.length; j++) {
          let currentDuplicate: iItemDuplicatesReadOnly = additionalDetails.duplicates[j];
          if (j == 0) retVal += "<div>" + currentDuplicate.sourceName + " (" + currentDuplicate.itemId + ")</div>";
          else retVal += "<div class='mt-1'>" + currentDuplicate.sourceName + " (" + currentDuplicate.itemId + ")</div>";
        }
        retVal += "</td>"
      }
      else {
        retVal += "<td>" + "No duplicates" + "</td>";
      }
      retVal += "</tr>";
    }
    return retVal;
  }

  public async GetDuplicatesReport02(currentItem: Item, uniqueSources: string[]): Promise<string> {
    let retVal: string = "";
    let res = await this.FetchAdditionalItemDetailsAsync(currentItem.itemId);

    if ((res != false) && (res != true)) {
      let additionalDetails: iAdditionalItemDetails = res;
      let listOfDuplicateIDs = "";

      for (let j = 0; j < additionalDetails.duplicates.length; j++) {
        let currentDuplicate: iItemDuplicatesReadOnly = additionalDetails.duplicates[j];
        listOfDuplicateIDs += currentDuplicate.itemId;
        listOfDuplicateIDs += ",";
      }
      // remove the trailing '.'
      listOfDuplicateIDs = listOfDuplicateIDs.substring(0, listOfDuplicateIDs.length - 1)

      retVal += "<tr style=\"vertical-align:top;\">";
      retVal += "<td>" + currentItem.shortTitle + "</td>";
      retVal += "<td>" + currentItem.year + "</td>";
      if (listOfDuplicateIDs == "") {
        retVal += "<td>" + currentItem.itemId + "</td>";
      }
      else {
        retVal += "<td>" + currentItem.itemId + "<br style=\"mso-data-placement: same-cell;\">(" + listOfDuplicateIDs + ")" + "</td>";
      }
      retVal += "<td>" + currentItem.typeName + "</td>";

      // Get the master source
      const sourcePosition = [];
      for (let m = 0; m < uniqueSources.length; m++) {
        if (additionalDetails.source.source_Name == uniqueSources[m]) {
          sourcePosition[m] = 'm'
        }
      }

      // get the duplicate source       
      for (let m = 0; m < uniqueSources.length; m++) {
        for (let j = 0; j < additionalDetails.duplicates.length; j++) {
          const currentDuplicate: iItemDuplicatesReadOnly = additionalDetails.duplicates[j];
          if (currentDuplicate.sourceName == uniqueSources[m]) {
            if (sourcePosition[m] == 'xm') {
              sourcePosition[m] = 'xm';
            }
            else if (sourcePosition[m] == 'm') {
              sourcePosition[m] = 'xm';
            }
            else {
              sourcePosition[m] = 'x';
            }
          }
        }
      }

      // use sourcePosition[] to build the row
      for (let m = 0; m < uniqueSources.length; m++) {
        if (sourcePosition[m] == 'x') {
          retVal += "<td style='text-align:center'>X</td>";
        }
        else if (sourcePosition[m] == 'm') {
          retVal += "<td style='text-align:center'>M</td>";
        }
        else if (sourcePosition[m] == 'xm') {
          retVal += "<td style='text-align:center'>MX</td>";
        }
        else {
          retVal += "<td></td>";
        }
      }

      retVal += "</tr>";
    }
    return retVal;
  }

  public async GetSourcesDuplicatesReport02(currentItem: Item): Promise<string> {
    let retVal: string = "";
    let res = await this.FetchAdditionalItemDetailsAsync(currentItem.itemId);

    if ((res != false) && (res != true)) {
      let additionalDetails: iAdditionalItemDetails = res;

      retVal += additionalDetails.source.source_Name + "⌐";

      if (additionalDetails.duplicates.length > 0) {
        for (var j = 0; j < additionalDetails.duplicates.length; j++) {
          let currentDuplicate: iItemDuplicatesReadOnly = additionalDetails.duplicates[j];
          retVal += currentDuplicate.sourceName + "⌐";
        }
      }
    }
    return retVal;
  }


  public SaveItems(items: ItemList, crit: Criteria) {
    //console.log('saving items');
    items.items = orderBy(items.items, this.sort);
    this._ItemList = items;
    this._Criteria = crit;
    //this.Save();
  }
  private ChangingItem(newItem: Item) {
    //console.log('ChangingItem');
    this._currentItem = newItem;
    this._CurrentItemAdditionalData = null;
    this.FetchAdditionalItemDetails();
    //this.SaveCurrentItem();
    //console.log('This is when this is emitted actually');
    this.ItemChanged.emit(newItem);
  }
  public getItem(itemId: number): Item {

    //console.log('getting item');
    let ff = this.ItemList.items.find(found => found.itemId == itemId);
    if (ff != undefined && ff != null) {
      //console.log('first emit');
      this.ChangingItem(ff);
      return ff;
    }
    else {
      this.ChangingItem(new Item());
      return new Item();
    }
  }
  public hasPrevious(itemId: number): boolean {
    //if (!this.IsInScreeningMode && this._PriorityScreeningService && this._PriorityScreeningService.TrainingList && this._PriorityScreeningService.TrainingList.length > 0) {
    //    return this._PriorityScreeningService.HasPrevious();
    //}
    //else {
    let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
    if (ff != undefined && ff != null && ff > 0 && ff != -1) {
      //console.log('Has prev (yes)' + ff);
      return true;
    }
    else {
      //console.log('Has prev (no)' + ff);
      return false;
    }
    //}
  }
  public getFirst(): Item {
    let ff = this.ItemList.items[0];
    if (ff != undefined && ff != null) {
      //this.ChangingItem(ff);
      return ff;
    }
    else {
      //this.ChangingItem(new Item());
      return new Item();
    }
  }
  public getPrevious(itemId: number): Item {

    let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
    if (ff != undefined && ff != null && ff > -1 && ff < this._ItemList.items.length) {
      //this.ChangingItem(this._ItemList.items[ff - 1]);
      return this._ItemList.items[ff - 1];
    }
    else {
      //this.ChangingItem(new Item());
      return new Item();
    }

  }
  public hasNext(itemId: number): boolean {
    //if (!this.IsInScreeningMode && this._PriorityScreeningService && this._PriorityScreeningService.TrainingList && this._PriorityScreeningService.TrainingList.length > 0) {
    //    return this._PriorityScreeningService.HasNext();
    //}
    //else {
    let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
    if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._ItemList.items.length) return true;
    else return false;
    //}
  }
  public getNext(itemId: number): Item {
    //console.log('getNext');
    let ff = this.ItemList.items.findIndex(found => found.itemId == itemId);
    //console.log(ff);
    if (ff != undefined && ff != null && ff > -1 && ff + 1 < this._ItemList.items.length) {
      //console.log('I am emitting');
      //this.ChangingItem(this._ItemList.items[ff + 1]);
      return this._ItemList.items[ff + 1];
    }
    else {
      //this.ChangingItem(new Item());
      return new Item();
    }
  }
  public getLast(): Item {
    let ff = this.ItemList.items[this._ItemList.items.length - 1];
    if (ff != undefined && ff != null) {
      //this.ChangingItem(ff);
      return ff;
    }
    else {
      //this.ChangingItem(new Item());
      return new Item();
    }
  }


  public FetchNextPage() {

    if (this.ItemList.pageindex < this.ItemList.pagecount - 1) {
      this._Criteria.pageNumber += 1;
    } else {
    }
    this.FetchWithCrit(this._Criteria, this.ListDescription)
  }
  public FetchPrevPage() {
    if (this.ItemList.pageindex == 0) {
      return this.FetchWithCrit(this._Criteria, this.ListDescription);
    } else {
      this._Criteria.pageNumber -= 1;
      return this.FetchWithCrit(this._Criteria, this.ListDescription);
    }
  }
  public FetchLastPage() {
    this._Criteria.pageNumber = this.ItemList.pagecount - 1;
    return this.FetchWithCrit(this._Criteria, this.ListDescription);
  }
  public FetchFirstPage() {
    this._Criteria.pageNumber = 0;
    return this.FetchWithCrit(this._Criteria, this.ListDescription);
  }
  public FetchParticularPage(pageNum: number) {
    this._Criteria.pageNumber = pageNum;
    return this.FetchWithCrit(this._Criteria, this.ListDescription);
  }

  public sort: SortDescriptor[] = [{
    field: 'shortTitle',
    dir: 'asc'
  }];
  public sortChange(sort: SortDescriptor[]): void {
    this.sort = sort;
    //console.log('sorting items by ' + this.sort[0].field + " ");
    this._ItemList.items = orderBy(this._ItemList.items, this.sort);
  }
  public get HasSelectedItems(): boolean {
    //return true;
    //console.log("HasSelectedItems?", this._ItemList.items[0].isSelected, this._ItemList.items[1].isSelected);
    if (this._ItemList.items.findIndex(found => found.isSelected == true) > -1) return true;
    else return false;
  }
  public get SelectedItems(): Item[] {
    return this._ItemList.items.filter(found => found.isSelected == true);
  }
  public SelectedItemsToRIStext(): string {
    let res: string = "";
    for (let Itm of this.SelectedItems) {
      res += ItemListService.ExportItemToRIS(Itm);
    }
    //console.log("SelectedItemsToRIStext", res);
    return res;
  }

  public async AllPagesToRIStext() {

    let res = "";
    let dataURI = "";
    let TheList: ItemList | boolean;

    let numberOfPages = Math.ceil(this.ItemList.totalItemCount / 4000);

    let MyCrit = this._Criteria.Clone();
    MyCrit.pageSize = 4000;

    for (let j = 0; j < numberOfPages; j++) {
      res = "";
      MyCrit.pageNumber = j;

      TheList = await this.FetchWithCrit(MyCrit, this.ListDescription, false);

      if ((TheList !== false) && (TheList !== true)) {
        for (let i = 0; i < TheList.items.length; i++) {
          res += ItemListService.ExportItemToRIS(TheList.items[i]);
        }
      }
      dataURI = "data:text/plain;base64," + encodeBase64(res);
      saveAs(dataURI, "ExportedRis_file_" + (j + 1) + "_of_" + numberOfPages + ".txt");
    }

    return res;

  }

  public static ExportItemToRIS(it: Item): string {
    const calend: string[] = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul",
      "Aug", "Sep", "Oct", "Nov", "Dec"];
    const newLine: string = "\r\n";
    let res: string = "TY  - ";
    let tmp: string = "";
    switch (it.typeId) {
      case 14:
        res += "JOUR" + newLine;
        break;
      case 1:
        res += "RPRT" + newLine;
        break;
      case 2:
        res += "BOOK" + newLine;
        break;
      case 3:
        res += "CHAP" + newLine;
        break;
      case 4:
        res += "THES" + newLine;
        break;
      case 5:
        res += "CONF" + newLine;
        break;
      case 6:
        res += "ELEC" + newLine;
        break;
      case 7:
        res += "ELEC" + newLine;
        break;
      case 8:
        res += "ADVS" + newLine;
        break;
      case 10:
        res += "MGZN" + newLine;
        break;
      default:
        res += "GEN" + newLine;
        break;
    }
    res += "T1  - " + it.title + newLine;
    if (it.typeId == 10 || it.typeId == 14)
      res += "JF  - " + it.parentTitle + newLine;
    else
      res += "T2  - " + it.parentTitle + newLine;
    for (let au of it.authors.split(';')) {
      tmp = au.trim();
      if (tmp != "") res += "A1  - " + tmp + newLine;
    }
    for (let au of it.parentAuthors.split(';')) {
      tmp = au.trim();
      if (tmp != "") res += "A2  - " + tmp + newLine;
    }
    res += "ST  - " + it.shortTitle + newLine;
    //new on April 2023: one Keyword per "KW" tag, not all in one big field!
    res += "KW  - eppi-reviewer" + newLine;
    if (it.keywords) {
      if (it.keywords.length > 6) {
        let FullRegexString: string = "";
        for (let exp of ItemListService.FindKeywordSeparator(it.keywords)) {
          FullRegexString += exp.source + "|";
        }
        FullRegexString = FullRegexString.substring(0, FullRegexString.length - 1);
        let regX: RegExp = new RegExp(FullRegexString, "g");
        const kWords = it.keywords.split(regX);
        let kWords2: string[] = [];
        for (let kw of kWords) {
          //we only add keywords that have content and don't add the same kw twice
          kw = kw.trim();
          if (kw.length > 0 && kWords2.indexOf(kw) == -1) kWords2.push(kw);
          }
        for (let kw of kWords2) {
          res += "KW  - " + kw + newLine;
        }
      }
      else {
        res += "KW  - " + it.keywords + newLine;
      }
    }
      + ((it.keywords != null && it.keywords.length > 2) ? it.keywords.trim() + newLine : "");
    let Month: number | null, Yr: number | null;
    let tmpDate: string = "";
    Month = Helpers.SafeParseInt(it.month);
    if (!Month || (Month < 1 || Month > 12)) {
      Month = 1 + it.month.length > 2 ? calend.indexOf(it.month.substring(0, 3)) + 1 : 0;
    }
    Yr = Helpers.SafeParseInt(it.year);
    if (it.year !== "" && Yr) {
      if (Yr > 0) {
        if (Yr < 20) Yr += 1900;
        else if (Yr < 100) Yr += 2000;
        if ((Yr.toString()).length == 4) {
          res += "PY  - " + Yr.toString() + newLine;
          if (Month != 0) {

            tmpDate += it.year + "/" +
              ((Month.toString().length == 1 ? "0" + Month.toString() : Month.toString()))
              + "//";
          }
          else {
            tmpDate += it.year + "///" + it.month;//"Y1  - " 
          }
        }
      }
    }
    if (tmpDate.length > 0) {
      res += "DA  - " + tmpDate + newLine;
      res += "Y1  - " + tmpDate;


      //little trick: edition information is supposed to be the additional info at the end of the 
      //Y1 filed. For Thesis pubtype (4) we use the edition field to hold the thesys type,
      //the following finishes up the Y1 field keeping all this into account

      if (it.typeId == 4 && it.edition.length > 0)
        res += newLine + "KW  - " + it.edition + newLine;
      else if (it.edition.length > 0)
        res += " " + it.edition + newLine;
      else res += newLine;

    }
    else if (it.typeId == 4 && it.edition.length > 0) {
      res += newLine + "KW  - " + it.edition + newLine;
    }//end of little trick

    //res += "N2  - " + it.abstract + newLine;
    res += "AB  - " + it.abstract + newLine;
    if (it.doi.length > 0) res += "DO  - " + it.doi + newLine;
    res += "VL  - " + it.volume + newLine;
    res += "IS  - " + it.issue + newLine;
    let split = '-';
    Yr = it.pages.indexOf(split);
    if (Yr > 0) {
      let pgs = it.pages.split(split);
      res += "SP  - " + pgs[0] + newLine;
      res += "EP  - " + pgs[1] + newLine;
    }
    else if (it.pages.length > 0) res += "SP  - " + it.pages + newLine;
    res += "CY  - " + it.city + (it.country.length > 0 ? " " + it.country : "") + newLine;
    if (it.url.length > 0)
      res += "UR  - " + it.url + newLine;
    if (it.availability.length > 0)
      res += "AV  - " + it.availability + newLine;
    if (it.publisher.length > 0)
      res += "PB  - " + it.publisher + newLine;
    if (it.standardNumber.length > 0)
      res += "SN  - " + it.standardNumber + newLine;
    res += "U1  - " + it.itemId.toString() + newLine;
    if (it.oldItemId.length > 0)
      res += "U2  - " + it.oldItemId + newLine;


    res += "N1  - " + it.comments + newLine;

    res += "ER  - " + newLine + newLine;

    res = res.replace("     ", " ");
    res = res.replace("    ", " ");
    res = res.replace("   ", " ");
    res = res.replace("   ", " ");
    return res;
  }

  private static FindKeywordSeparator(Keywords: string): RegExp[]{
    let res: RegExp[] = [];
    let scores: StringKeyValue[] = [];
    const len = Keywords.length;
    if (len <= 6) return res;
    const separators4Keywords: RegExp[] = [new RegExp("\r\n", "g"), new RegExp("\r", "g"), new RegExp("\n", "g"), new RegExp(";", "g")
      , new RegExp("\t", "g"), new RegExp(",", "g"), new RegExp("\\.", "g"), new RegExp(":", "g")]; //Does not include space!!
    //we don't want keywords to be more that 30 chars long, on average
    const aimFor = Math.ceil(len / 30.0);
    //we don't want keywords to be less than 7 chars long, on average (mean word-length in Eng, including articles, prepositions, etc. is around 5)
    const tooMany = Math.ceil(len / 7.0);

    //first simple attempt, use all separators, see if it gives us a nice result
    //at this stage, if any given separator is good, we return only that, else we'll evaluate how it goes for ALL separators
    let matchesCount: number = 0;

    for (let re of separators4Keywords) {
      const OneSepCnt = (Keywords.match(re) || []).length;
      if (OneSepCnt >= aimFor && OneSepCnt <= tooMany) {
        //perfect! We found one ideal separator;
        res = [];
        res.push(re);
        return res;
      }
      else if (OneSepCnt > 0 && OneSepCnt <= tooMany) {
        //matched something, but not too much, so might be one of many separators
        matchesCount = matchesCount + OneSepCnt;
        res.push(re);
        const kvp = new StringKeyValue(re.source, OneSepCnt.toString());
        scores.push(kvp);//used later, perhaps, for sorting our separators that match
      }
    }
    if (matchesCount >= aimFor && matchesCount <= tooMany) return res;
    
    //if we reached this point, no SINGLE separator appeared to be good enough :-(
    //so we'll return a number of separators, based on the ones we've collected so far.
    //we want to return the minimum number of separators, so we sort them in desc order
    scores.sort((a, b) => {
      const aval = parseInt(a.value);
      if (isNaN(aval)) return 0;
      const bval = parseInt(b.value);
      if (isNaN(bval)) return 0;
      return aval - bval;
    });
    matchesCount = 0;
    res = [];
    for (let kvp of scores)
    {
      const aval = parseInt(kvp.value);
      if (!isNaN(aval)) {
        matchesCount = matchesCount + aval;
        res.push(new RegExp(kvp.key));
      }
      if (matchesCount > aimFor) {//alright, we have what we came for...
        return res;
      }
    }
    //if we reached this point, no combination of separators appeared to be good enough :-(
    //so, does a simple "spaces" work well?
    const spaces: RegExp[] = [new RegExp("\s", "g")];
    const OneSepCnt = (Keywords.match(spaces[0]) || []).length;
    if (OneSepCnt >= aimFor && OneSepCnt <= tooMany) return spaces;
    //meh, not even "just space" worked, we'll add "space" to our results, return and hope for the best...
    res = [];
    res = res.concat(separators4Keywords);
    res = res.concat(spaces);
    return res;
  }


  DeleteSelectedItems(ItemIds: Item[]) {

    this._BusyMethods.push("DeleteSelectedItems");
    let Ids = ItemIds.map(x => x.itemId);
    //console.log("IDs:", Ids);
    //var strItemIds = ItemIds.map(x => x.itemId).toString();

    //let body = JSON.stringify({ ItemIds: strItemIds });

    this._httpC.post<any>(this._baseUrl + 'api/ItemList/DeleteSelectedItems',
      Ids)
      .subscribe(
        list => {

          //var ItemIdStr = list.toString().split(",");
          //var wholListItemIdStr = this.ItemList.items.map(x => x.itemId);
          //for (var i = 0; i < ItemIdStr.length; i++) {
          //	var id = Number(ItemIdStr[i]);
          //	var ind = wholListItemIdStr.indexOf(id);
          //	this.ItemList.items.slice(ind, 1);
          //}
          //this._Criteria.totalItems = this.ItemList.totalItemCount;
          //this.SaveItems(this.ItemList, this._Criteria);
          //this.ListChanged.emit();
          this.Refresh();
          //this.FetchWithCrit(this._Criteria, "StandardItemList");


        }, error => {
          this.ModalService.GenericError(error);
          this.RemoveBusy("DeleteSelectedItems");
        }
        , () => { this.RemoveBusy("DeleteSelectedItems"); }
      );
  }

  public FetchSingleItem(ItemId: number): Promise<Item | null> {
    this._BusyMethods.push("FetchSingleItem");
    let body = JSON.stringify({ Value: ItemId });

    return lastValueFrom(this._httpC.post<Item>(this._baseUrl + 'api/ItemList/GetSingleItem',
      body)).then(
        result => {
          this.RemoveBusy("FetchSingleItem");
          //console.log("FetchSingleItem, fetched this:", result);
          return result;
        },
        (error) => {
          this.ModalService.SendBackHomeWithError(error);
          this.RemoveBusy("FetchSingleItem");
          return null;
        }
      );
  }

  //public Save() {
  //    if (this._ItemList.items.length > 0) {
  //        localStorage.setItem('ItemsList', JSON.stringify(this._ItemList));
  //    }
  //    else if (localStorage.getItem('ItemsList')) {
  //        localStorage.removeItem('ItemsList');
  //    }
  //    if (this._Criteria.listType != "") {
  //        localStorage.setItem('ItemsListCriteria', JSON.stringify(this._Criteria));
  //    }
  //    else if (localStorage.getItem('ItemsListCriteria')) {
  //        localStorage.removeItem('ItemsListCriteria');
  //    }
  //    if (this._IsInScreeningMode !== null) localStorage.setItem('ItemListIsInScreeningMode', JSON.stringify(this._IsInScreeningMode));
  //    else if (localStorage.getItem('ItemListIsInScreeningMode')) {
  //        localStorage.removeItem('ItemListIsInScreeningMode');
  //    }
  //    this.SaveCurrentItem();
  //}
  public Clear() {
    this._ItemList = new ItemList();
    this._Criteria = new Criteria();
    this._currentItem = new Item();
    this._IsInScreeningMode = null;
    this.ListDescription = "";
    this._CurrentItemAdditionalData = null;
  }
}

export class ItemListOptions {

  public showId: boolean = true;
  public showImportedId: boolean = false;
  public showShortTitle: boolean = true;
  public showTitle: boolean = true;
  public showYear: boolean = true;
  public showAuthors: boolean = false;
  public showJournal: boolean = false;
  public showDocType: boolean = false;
  public showInfo: boolean = false;
  public showScore: boolean = false;

}


export class ItemList {
  pagesize: number = 0;
  pageindex: number = 1;
  pagecount: number = 0;
  totalItemCount: number = 0;
  items: Item[] = [];
}
export class Item {
  itemId: number = 0;
  masterItemId: number = 0;
  isDupilcate: boolean = false;
  typeId: number = 0;
  title: string = "";
  parentTitle: string = "";
  shortTitle: string = "";
  dateCreated: string = "";
  createdBy: string = "";
  dateEdited: string = "";
  editedBy: string = "";
  year: string = "";
  month: string = "";
  standardNumber: string = "";
  city: string = "";
  country: string = "";
  publisher: string = "";
  institution: string = "";
  volume: string = "";
  pages: string = "";
  edition: string = "";
  issue: string = "";
  isLocal: string = "";
  availability: string = "";
  url: string = "";
  oldItemId: string = "";
  abstract: string = "";
  comments: string = "";
  typeName: string = "";
  authors: string = "";
  parentAuthors: string = "";
  doi: string = "";
  keywords: string = "";
  attributeAdditionalText: string = "";
  rank: number = 0;
  isItemDeleted: boolean = false;
  isIncluded: boolean = true;
  isSelected: boolean = false;
  itemStatus: string = "";
  itemStatusTooltip: string = "";
  arms: iArm[] = [];
  timepoints: iTimePoint[] = [];
  quickCitation: string = "";
}
export class Criteria {
  public Clone(): Criteria {
    let interimCrit: Criteria = new Criteria();
    interimCrit.onlyIncluded = this.onlyIncluded;
    interimCrit.showDeleted = this.showDeleted;
    interimCrit.sourceId = this.sourceId;
    interimCrit.searchId = this.searchId;
    interimCrit.xAxisSetId = this.xAxisSetId;
    interimCrit.xAxisAttributeId = this.yAxisAttributeId;
    interimCrit.yAxisSetId = this.yAxisSetId;
    interimCrit.yAxisAttributeId = this.yAxisAttributeId;
    interimCrit.filterSetId = this.filterSetId;
    interimCrit.filterAttributeId = this.filterAttributeId;
    interimCrit.attributeSetIdList = this.attributeSetIdList;
    interimCrit.listType = this.listType;
    interimCrit.attributeid = this.attributeid;

    interimCrit.pageNumber = 0;
    interimCrit.pageSize = this.pageSize;
    interimCrit.totalItems = this.totalItems;
    interimCrit.startPage = this.startPage;
    interimCrit.endPage = this.endPage;
    interimCrit.startIndex = this.startIndex;
    interimCrit.endIndex = this.endIndex;
    interimCrit.magSimulationId = this.magSimulationId;
    interimCrit.workAllocationId = this.workAllocationId;
    interimCrit.comparisonId = this.comparisonId;
    interimCrit.description = this.description;
    interimCrit.contactId = this.contactId;
    interimCrit.setId = this.setId;
    interimCrit.showInfoColumn = this.showInfoColumn;
    interimCrit.showScoreColumn = this.showScoreColumn;
    interimCrit.withOutAttributesIdsList = this.withOutAttributesIdsList;
    interimCrit.withAttributesIds = this.withAttributesIds;
    interimCrit.withSetIdsList = this.withSetIdsList;
    interimCrit.withOutSetIdsList = this.withOutSetIdsList;

    return interimCrit;
  }
  onlyIncluded: boolean | null = true;
  showDeleted: boolean = false;
  sourceId: number = 0;
  searchId: number = 0;
  xAxisSetId: number = 0;
  xAxisAttributeId: number = 0;
  yAxisSetId: number = 0;
  yAxisAttributeId: number = 0;
  filterSetId: number = 0;
  filterAttributeId: number = 0;
  attributeSetIdList: string = "";
  listType: string = "";
  attributeid: number = 0;

  pageNumber: number = 0;
  pageSize: number = 100;
  totalItems: number = 0;
  startPage: number = 0;
  endPage: number = 0;
  startIndex: number = 0;
  endIndex: number = 0;
  magSimulationId: number = 0;
  workAllocationId: number = 0;
  comparisonId: number = 0;
  description: string = "";
  contactId: number = 0;
  setId: number = 0;
  showInfoColumn: boolean = false;
  showScoreColumn: boolean = false;
  withOutAttributesIdsList: string = "";
  withAttributesIds: string = "";
  withSetIdsList: string = "";
  withOutSetIdsList: string = "";
}



export class ItemDocumentList {

  ItemDocuments: ItemDocument[] = [];
}
export class ItemDocument {

  public itemDocumentId: number = 0;
  public itemId: number = 0;
  public shortTitle: string = '';
  public extension: string = '';
  public title: string = '';
  public text: string = "";
  public binaryExists: boolean = false;
  public textFrom: number = 0;
  public textTo: number = 0;
  public freeNotesStream: string = "";
  public freeNotesXML: string = '';
  public isBusy: boolean = false;
  public isChild: boolean = false;
  public isDeleted: boolean = false;
  public isDirty: boolean = false;
  public isNew: boolean = false;
  public isSavable: boolean = false;
  public isSelfBusy: boolean = false;
  public isSelfDirty: boolean = false;
  public isSelfValid: boolean = false;
  public isValid: boolean = false;
}
export interface iAdditionalItemDetails {
  itemID: number;
  duplicates: iItemDuplicatesReadOnly[];
  source: ReadOnlySource;
}
export interface iItemDuplicatesReadOnly {
  itemId: number;
  shortTitle: string;
  sourceName: string;
}
export class StringKeyValue {//used in more than one place...
  constructor(k: string, v: string) {
    this.key = k;
    this.value = v;
  }
  key: string;
  value: string;
}
export interface iItemLink {
  itemLinkId: number;
  itemIdPrimary: number;
  itemIdSecondary: number;
  title: string;
  shortTitle: string;
  description: string;
}

