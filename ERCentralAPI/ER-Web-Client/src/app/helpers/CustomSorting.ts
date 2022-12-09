
//class implements STATIC methods to operate of sorting for tables, regardless of what is being sorted

export class CustomSorting {
  public static SortBy(fieldName: string, listToSort: object[], sortingStorage: LocalSort) {
    //console.log("SortBy", fieldName);
    if (listToSort.length == 0) return;
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
        CustomSorting.DoSort(listToSort, sortingStorage);
        break;
      }
    }
  }
  public static DoSort(listToSort: object[], sortingStorage: LocalSort) {
    //console.log("doSort", this.LocalSort);
    if (listToSort.length == 0 || sortingStorage.SortBy == "") return;
    for (let property of Object.getOwnPropertyNames(listToSort[0])) {
      //console.log("doSort2", property);
      if (property == sortingStorage.SortBy) {
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
