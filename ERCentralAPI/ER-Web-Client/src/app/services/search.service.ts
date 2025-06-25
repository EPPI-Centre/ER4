import { Inject, Injectable, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { ConfigService } from './config.service';
import { Helpers } from '../helpers/HelperMethods';
import { GridDataResult, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';

@Injectable({

  providedIn: 'root',

})

export class searchService extends BusyAwareService implements OnDestroy {

  constructor(
    private _httpC: HttpClient,
    private modalService: ModalService,
    private EventEmitterService: EventEmitterService,
    configService: ConfigService
  ) {
    super(configService);
    console.log("On create search service");
    this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
  }

  ngOnDestroy() {
    console.log("Destroy search service");
    if (this.clearSub != null) this.clearSub.unsubscribe();
  }
  private clearSub: Subscription | null = null;
  public cmdSearches: SearchCodeCommand = new SearchCodeCommand();

  private _SearchList: Search[] = [];
  public selectedSourceDropDown: string = '';


  //@Output() searchesChanged = new EventEmitter();
  //public crit: CriteriaSearch = new CriteriaSearch();
  public searchToBeUpdated: string = '';//WHY string?

  public get SearchList(): Search[] {

    return this._SearchList;

  }

  public set SearchList(searches: Search[]) {
    this._SearchList = searches;
    //this.searchesChanged.emit();
  }

  ///members/methods for Telerik Table management////////////////////////////
  public get DataSourceSearches(): GridDataResult {
    //console.log("DataSourceSearches");
    return process(this.SearchList, this.state);
  }
  public sortSearches: SortDescriptor[] = [{
    field: 'searchNo',
    dir: 'desc'
  }];

  public state: State = {
    skip: 0,
    take: 100,
    sort: this.sortSearches,
    filter: {
      logic: "and",
      filters: [],
    },
    group: []
  };
  public dataStateChange(state: DataStateChangeEvent): void {
    //console.log("dataStateChange");
    this.state = state;
    if (state.sort) this.sortSearches = state.sort;
    //this.DataSourceSearches; //makes sure it's "processed"
  }

  public sortChangeSearches(sort: SortDescriptor[]): void {
    this.sortSearches = sort;
    //console.log('sorting?' + this.sortSearches[0].field + " ");
  }
  //END of members/methods for Telerik Table management/////////////////////

  private _SearchVisualiseData: iSearchVisualise[] = [];
  public get SearchVisualiseData(): iSearchVisualise[] {
    return this._SearchVisualiseData;
  }
  public set SearchVisualiseData(searches: iSearchVisualise[]) {
    this._SearchVisualiseData = searches;
  }

  Fetch() {
    this._BusyMethods.push("Fetch");
    lastValueFrom(this._httpC.get<iSearch[]>(this._baseUrl + 'api/SearchList/GetSearches'))
      .then(result => {
        let resList: Search[] = [];
        this.RemoveBusy("Fetch");
        for (const iSrc of result) {
          resList.push(new Search(iSrc));
        }
        this.SearchList = resList;
        //this.searchesChanged.emit();
      },
        error => {
          this.RemoveBusy("Fetch");
          this.modalService.GenericError(error);
          this.Clear();
        }
      ).catch(caught => {
        this.RemoveBusy("Fetch");
        this.modalService.GenericError(caught);
      });
  }
  public Clear() {
    console.log("clear in Searches Service");
    this._SearchList = [];
    this.searchToBeUpdated = "";
    this.cmdSearches = new SearchCodeCommand();
    //this.crit = new CriteriaSearch();
    //this._isBusy = false;
  }


  Delete(value: string) {

    this._BusyMethods.push("Delete");
    let body = JSON.stringify({ Value: value });
    const TheList = this.SearchList;
    this.SearchList = [];
    lastValueFrom(this._httpC.post<string>(this._baseUrl + 'api/SearchList/DeleteSearch',
      body)).then(result => {
        const splitted = value.split(',');
        for (const deleted of splitted) {
          if (deleted.length > 0) {
            const Id = Helpers.SafeParseInt(deleted);
            if (Id) {
              const index = TheList.findIndex(f => f.searchId == Id);
              if (index != -1) {
                TheList.splice(index, 1);
              }
            }
          }
        }
        this.SearchList = TheList;
        //this.dataStateChange(this.state as DataStateChangeEvent);
        //const throwaway = this.DataSourceSearches;
        //console.log(throwaway, throwaway.data, this.state);
        //const temp = this.DataSourceSearches;
        this.RemoveBusy("Delete");
      }, error => {
        this.Fetch();
        this.RemoveBusy("Delete");
        this.modalService.GenericError(error);
      }
      ).catch(caught => {
        this.Fetch();
        this.RemoveBusy("Delete");
        this.modalService.GenericError(caught);
      });
  }

  CreateSearch(cmd: SearchCodeCommand, apiStr: string) {
    this._BusyMethods.push("CreateSearch");
    apiStr = 'api/SearchList/' + apiStr;
    //different controller methods return different things, hence the 'post<any>' form
    lastValueFrom(this._httpC.post<any>(this._baseUrl + apiStr,
      cmd)).then(() => {
        this.RemoveBusy("CreateSearch");
        this.Fetch();
      }, error => {
        this.RemoveBusy("CreateSearch");
        this.modalService.GenericError(error);
      }
    ).catch(caught => {
      this.RemoveBusy("CreateSearch");
      this.modalService.GenericError(caught);
    });
  }


  public async UpdateSearchName(searchName: string, searchId: string): Promise<boolean> {
    this._BusyMethods.push("UpdateSearchName");
    let _SearchName: SearchNameUpdate = { SearchId: searchId, SearchName: searchName };
    let body = JSON.stringify(_SearchName);

    return lastValueFrom(this._httpC.post<Search>(this._baseUrl + 'api/SearchList/UpdateSearchName',
      body))
      .then(
        (result) => {
          this.RemoveBusy("UpdateSearchName");

          // just update that line (rather than reloading all searches).
          let parsedInt: number = parseInt(searchId);
          let tmpIndex: number = this.SearchList.findIndex(x => x.searchId == parsedInt);
          if (tmpIndex > -1) this.SearchList[tmpIndex].title = _SearchName.SearchName;
          return true;
        }, error => {
          this.modalService.GenericError(error); //actual error
          //this.modalService.GenericErrorMessage("There was an error updating the search name. Please contact eppisupport@ucl.ac.uk");
          this.RemoveBusy("UpdateSearchName");
          return false;
        }
    ).catch(caught => {
      this.RemoveBusy("UpdateSearchName");
      this.modalService.GenericError(caught);
      return false;
    });
  }

  public CreateVisualiseData(searchId: number): any[] {

    this._BusyMethods.push("CreateVisualiseData");
    let body = JSON.stringify({ searchId: searchId });

    this._httpC.post<any[]>(this._baseUrl + 'api/SearchList/CreateVisualiseData', body)
      .subscribe(result => {

        this.SearchVisualiseData = result;
        this.RemoveBusy("CreateVisualiseData");
        return result;

      },
        error => {
          this.RemoveBusy("CreateVisualiseData");
          this.modalService.GenericError(error);
          return [];
        }
      );

    return this.SearchVisualiseData;
  }

}

export interface iSearch {
  searchNo: number;
  selected: boolean;
  searchId: number;
  hitsNo: number;
  title: string;
  searchDate: string;
  contactName: string;
  isClassifierResult: boolean;
}

export class Search implements iSearch {
  constructor(iSrc: iSearch | undefined) {
    if (iSrc) {
      this.searchNo = iSrc.searchNo;
      this.selected = iSrc.selected;
      this.searchId = iSrc.searchId;
      this.hitsNo = iSrc.hitsNo;
      this.title = iSrc.title;
      this.searchDate = iSrc.searchDate;
      this.contactName = iSrc.contactName;
      this.isClassifierResult = iSrc.isClassifierResult;
      this.add = false;
      this.javaScriptDate = new Date(Helpers.StringWithSlashesToDate(iSrc.searchDate));
    }
  }
  searchNo: number = 0;
  selected: boolean = false;
  searchId: number = 0;
  hitsNo: number = 0;
  title: string = '';
  searchDate: string = '';
  javaScriptDate: Date = new Date();
  contactName: string = '';
  isClassifierResult: boolean = false;
  add: boolean = false;
}
export class SearchCodeCommand {

  public _searches: string = '';
  public _logicType: string = '';
  public _setID: number = 0;
  public _searchText: string = '';
  public _IDs: string = '';
  public _title: string = '';
  public _answers: string = '';
  public _included: string = 'false';
  public _withCodes: string = 'false';
  public _searchId: number = 0;
  public _contactId: number = 0;
  public _contactName: string = '';
  public _searchType: string = '';
  public _scoreOne: number = 0;
  public _scoreTwo: number = 0;
  public _sourceIds: string = '';
  public _searchWhat: string = '';
}

export interface SearchNameUpdate {
  SearchId: string;
  SearchName: string;
}

export interface iSearchVisualise {
  count: number;
  range: string;
}


