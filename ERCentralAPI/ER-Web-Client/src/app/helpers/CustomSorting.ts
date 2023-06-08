
//class implements STATIC methods to operate of sorting for tables, regardless of what is being sorted

import { orderBy, SortDescriptor } from "@progress/kendo-data-query";

export class CustomSorting {
  public static SortBy<T>(fieldName: string, listToSort: T[], sortingStorage: LocalSort): T[] {
    //console.log("SortBy", fieldName);
    //let start = Date.now();
    if (listToSort.length == 0) return listToSort;
    for (let property of Object.getOwnPropertyNames(listToSort[0])) {
      //console.log("SortByP", property);
      if (property == fieldName) {
        if (sortingStorage.SortBy == fieldName) {
          //console.log("SortBy", 1);
          sortingStorage.Direction = !sortingStorage.Direction;
        } else {
          //console.log("SortBy", 2);
          sortingStorage.Direction = true;
          sortingStorage.SortBy = fieldName;
        }
        listToSort = CustomSorting.DoSort(listToSort, sortingStorage);
        break;
      }
    }
    //let timetaken = Date.now() - start;
    //console.log("Total time taken (my method): " + timetaken + " milliseconds");
    return listToSort;
  }
  public static DoSort<T>(listToSort: T[], sortingStorage: LocalSort): T[] {
    //console.log("doSort", this.LocalSort);

    let Tsort: SortDescriptor[] = [{
      field: sortingStorage.SortBy,
      dir: sortingStorage.Direction ? 'asc' : 'desc'
    }];

    listToSort = orderBy(listToSort, Tsort);
    return listToSort;
  }
  private static OldDoSort(listToSort: object[], sortingStorage: LocalSort) { 

    //deprecated!

    if (listToSort.length == 0 || sortingStorage.SortBy == "") return;
    for (let property of Object.getOwnPropertyNames(listToSort[0])) {
      //console.log("doSort2", property);
      if (property == sortingStorage.SortBy) {
        const ObjTypeChk: any = listToSort[0];
        const TypeChecker: any = ObjTypeChk[property];
        if (typeof TypeChecker == "string") {
          if (sortingStorage.Direction) {
            listToSort.sort((a: any, b: any) => {
              return a[property].localeCompare(b[property], "en-001", { sensitivity: "accent" });
            });
          } else {
            listToSort.sort((b: any, a: any) => {
              return a[property].localeCompare(b[property], "en-001", { sensitivity: "accent" });
            });
          }
        }
        else {
          listToSort.sort((a: any, b: any) => {
            if (sortingStorage.Direction) {
              if (a[property] > b[property]) {
                return 1;
              } else if (a[property] < b[property]) {
                return -1;
              } else {
                return 0;
              }
            } else {
              if (a[property] > b[property]) {
                return -1;
              } else if (a[property] < b[property]) {
                return 1;
              } else {
                return 0;
              }
            }
          });
        }
        break;
      }
    }
  }
  public static SortingSymbol(fieldName: string, sortingStorage: LocalSort): string {
    if (sortingStorage.SortBy !== fieldName) return "";
    else if (sortingStorage.Direction) return '&uarr;';
    else return '&darr;';
  }
}

export class LocalSort {
  SortBy: string = "";
  Direction: boolean = true;//Ascending if true
}
