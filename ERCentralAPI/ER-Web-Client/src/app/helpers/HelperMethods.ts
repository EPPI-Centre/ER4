

//this class should only contain static methods.
//it's a collection of methods that can be used by any component/service. Main purpose is to avoid replication.

import { min } from "rxjs-compat/operator/min";

//Please include a short description of the purpose of any method added in this class.
export class Helpers {
  //Used to format dates when the date string is like "DD-MM-YYYY[...]".
  //By using toLocaleDateString() we should be showing the date in the locale-specific way that applies to the current client...
  public static FormatDate(DD_MM_YYYY_: string): string {
    if (!DD_MM_YYYY_ || DD_MM_YYYY_.length < 10) return "";
    else {
      const year = parseInt(DD_MM_YYYY_.substr(6, 4));
      const month = parseInt(DD_MM_YYYY_.substr(3, 2)) - 1;
      const day = parseInt(DD_MM_YYYY_.substr(0, 2));
      const date: Date = new Date(year, month, day);
      return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
    }
  }
  //Used to format dates when the date string is like "YYYY-MM-DD[...]".
  //By using toLocaleDateString() we should be showing the date in the locale-specific way that applies to the current client...
  public static FormatDate2(YYYY_MM_DD_: string): string {
    if (!YYYY_MM_DD_ || YYYY_MM_DD_.length < 10) return "";
    else {
      const year = parseInt(YYYY_MM_DD_.substr(0, 4));
      const month = parseInt(YYYY_MM_DD_.substr(5, 2)) - 1;
      const day = parseInt(YYYY_MM_DD_.substr(8, 2));
      //console.log(year);
      const date: Date = new Date(year, month, day);
      return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
    }
  }
  public static FormatDateWithInputSlashes(DfsMfsYYYY: string): string {
    //this is marginally slower that FormatDate(DD_MM_YYYY_: string)
    if (!DfsMfsYYYY || DfsMfsYYYY.length < 8 || DfsMfsYYYY.indexOf('/') == -1) return "";
    else {
      const splitted = DfsMfsYYYY.split("/");
      if (splitted.length != 3) return "";
      const year = parseInt(splitted[2]);
      const month = parseInt(splitted[1]) - 1;
      const day = parseInt(splitted[0]);
      const date: Date = new Date(year, month, day);
      return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
    }
  }
  public static FormatDateTime(YYYY_MM_DD_: string): string {  //2021-11-22T11:51:32.517
    if (!YYYY_MM_DD_ || YYYY_MM_DD_.length < 19) return "";
    else {
      const year = parseInt(YYYY_MM_DD_.substr(0, 4));
      const month = parseInt(YYYY_MM_DD_.substr(5, 2)) - 1;
      const day = parseInt(YYYY_MM_DD_.substr(8, 2));
      const hour = parseInt(YYYY_MM_DD_.substr(11, 2));
      const minute = parseInt(YYYY_MM_DD_.substr(14, 2));
      const second = parseInt(YYYY_MM_DD_.substr(17, 2));
      const date: Date = new Date(year, month, day, hour, minute, second);
      return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric', hour: 'numeric', minute: 'numeric', second: 'numeric' });
    }
  }
  public static StringWithSlashesToDate(DfsMfsYYYY: string): Date {
    const splitted = DfsMfsYYYY.split("/");
    if (splitted.length != 3) return new Date();
    const year = parseInt(splitted[2]);
    const month = parseInt(splitted[1]) - 1;
    const day = parseInt(splitted[0]);
    const date: Date = new Date(year, month, day);
    return date;
  }
  //used when we want a component to wait some time in a sync manner (for example to check when a service stops being busy).
  //Correct way to call this function requires the calling method to be async. 
  //If done within a loop, should also include a safety to avoid infinite loops!!!
  //Example: 
  //async CheckServiceX() {
  //    let counter: number = 0;
  //    while (this.ServiceX.IsBusy && counter < 5 * 120) {
  //        counter++;
  //        await Helpers.Sleep(200);
  //    }
  //    will remain here for up to 2 minutes (200ms*5*120)... counter ensures we won't have an endless loop.
  //    this.DoSomething("Service isn't busy anymore or we gave up on it...");
  //}
  public static Sleep(milliseconds: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, milliseconds));
  }


  //should not crash, irrespective of input.
  //will return integer OR null, so caller needs to check for nulls!
  public static SafeParseInt(str: string): number | null {
    let retValue: number | null = null;
    if (str !== null) {
      if (str.length > 0) {
        let tmp = Number(str);
        if (!isNaN(tmp)) {
          retValue = parseInt(str);
        }
      }
    }
    return retValue;
  }

  //should not crash, irrespective of input.
  //will return number OR null, so caller needs to check for nulls!
  public static SafeParseNumber(str: string): number | null {
    let retValue: number | null = null;
    if (str !== null) {
      if (str.length > 0) {
        let tmp = Number(str);
        if (!isNaN(tmp)) {
          retValue = tmp;
        }
      }
    }
    return retValue;
  }

  //used in 2 item details pages (view, edit) may be useful in other places.
  //is used to drive the appeareance and display name of fields that change meaning depending on pub-type.
  public static FieldsByPubType(typeId: number) {
    //if (typeId == 0) return null;
    //else
    if (typeId == 14) {
      return {
        parentTitle: { txt: 'Journal', optional: false }
        , parentAuthors: { txt: 'Parent Authors', optional: true }
        , standardNumber: { txt: 'ISSN', optional: false }

      };
    }
    else if (typeId == 2) {
      return {
        parentTitle: { txt: 'Parent Title', optional: true }
        , parentAuthors: { txt: 'Parent Authors', optional: true }
        , standardNumber: { txt: 'ISBN', optional: false }
      };
    }
    else if (typeId == 3) {
      return {
        parentTitle: { txt: 'Book Title', optional: false }
        , parentAuthors: { txt: 'Editors', optional: false }
        , standardNumber: { txt: 'ISBN', optional: false }
      };
    }
    else if (typeId == 4) {
      return {
        parentTitle: { txt: 'Publ. Title', optional: false }
        , parentAuthors: { txt: 'Parent Authors', optional: true }
        , standardNumber: { txt: 'ISSN/ISBN', optional: false }
      };
    }
    else if (typeId == 5) {
      return {
        parentTitle: { txt: 'Conference', optional: false }
        , parentAuthors: { txt: 'Parent Authors', optional: true }
        , standardNumber: { txt: 'ISSN/ISBN', optional: false }
      };
    }
    else if (typeId == 10) {
      return {
        parentTitle: { txt: 'Periodical', optional: false }
        , parentAuthors: { txt: 'Parent Authors', optional: true }
        , standardNumber: { txt: 'ISSN/ISBN', optional: true }
      };
    }
    else if (typeId == 0) {
      return {
        parentTitle: { txt: 'Parent Title', optional: false }
        , parentAuthors: { txt: 'Parent Authors', optional: false }
        , standardNumber: { txt: 'Standard Number', optional: false }
      };
    }
    else {
      return {
        parentTitle: { txt: 'Parent Title', optional: true }
        , parentAuthors: { txt: 'Parent Authors', optional: true }
        , standardNumber: { txt: 'Standard Number', optional: true }
      };
    }
  }

  //used to add link to stylesheet and HTML frame to HTML content, usually for reports
  //gets used to show and save reports.
  public static AddHTMLFrame(report: string, baseUrl: string, title?: string, addExcelStyleHack: boolean = false): string {
    //used to save reports
    if (title === undefined) title = "EPPI-Reviewer Coding Report";
    //let res = "<HTML id='content'><HEAD><title>" + title + "</title><link rel='stylesheet' href='" + baseUrl + "styles.css' />";
    let res = "<HTML id='content'><HEAD><title>" + title + "</title>"
      + "<style type='text/css'> table {border-collapse: collapse;} .border {border: 1px solid #dee2e6!important;} .border-dark { border-color: #343a40!important;}"
      + " body { font-family: Roboto, Arial, sans-serif; font-size: 14px;} .alert-info { color: #0c5460; background-color: #d1ecf1; border-color: #bee5eb;}"
      + " .small, small { font-size: .875em; font-weight: 400;} .text-info { color: #17a2b8!important; } code { color: #e01a76;}"
      + " .text-success { color: #28a745!important;} .font-weight-bold { font-weight: 700!important;} .light-yellow-bg { background-color: #fff3cd; }"
      + " .ItemsTable th, .ItemsTable td {border: 1px solid #dee2e6!important; border-color: #343a40!important;} .m-2 { margin: 0.5rem!important;}";
      //+ "<link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap.min.css' integrity='sha384-xOolHFLEh07PJGoPkLv1IbcEPTNtaed2xpHsD9ESMhqIYd0nLMwNLD69Npy4HI+N' crossorigin='anonymous'>";
    if (addExcelStyleHack) res += " br { mso-data-placement: same-cell;} </style>";
    else res += "</style>";
    res += "</HEAD><BODY class='m-2' id='body'>" + report;
    //res += "<br /><a download='report.html' href='data:text/html;charset=utf-8," + report + "'>Save...</a></BODY></HTML>";
    //res += "<br />" + this.AddSaveMe() + "</BODY></HTML>";
    res += "</BODY></HTML>";
    return res;
  }
  public static CleanHTMLforExport(text: string, substitutions: SubstituteString[]): string {
    for (let ss of substitutions) {
      text = text.split(ss.searchFor).join(ss.changeTo);
    }
    return text;
  }
  //used to show reports in a throwaway new tab.
  public static OpenInNewWindow(ReportHTML: any, baseUrl: string) {
    if (ReportHTML.length < 1) return;

    let Pagelink = "about:blank";
    let pwa = window.open(Pagelink, "_new");
    //let pwa = window.open("data:text/plain;base64," + btoa(this.AddHTMLFrame(this.ReportHTML)), "_new");
    if (pwa) {
      pwa.document.open();
      pwa.document.write(this.AddHTMLFrame(ReportHTML, baseUrl));
      pwa.document.close();
    }
  }
  public static LevDist(a: string, b: string) {
    //ADAPTED  from: Clément kigiri https://gist.github.com/andrei-m/982927

    if (a.length === 0) return b.length;
    if (b.length === 0) return a.length;
    let tmp, i, j, prev, val, row;
    let len = Math.max(a.length, b.length);
    // swap to save some memory O(min(a,b)) instead of O(a)
    if (a.length > b.length) {
      tmp = a;
      a = b;
      b = tmp;
    }

    row = Array(a.length + 1);
    // init the row
    for (i = 0; i <= a.length; i++) {
      row[i] = i;
    }

    // fill in the rest
    for (i = 1; i <= b.length; i++) {
      prev = i;
      for (j = 1; j <= a.length; j++) {
        if (b[i - 1] === a[j - 1]) {
          val = row[j - 1]; // match
        } else {
          val = Math.min(row[j - 1] + 1, // substitution
            Math.min(prev + 1,     // insertion
              row[j] + 1));  // deletion
        }
        row[j - 1] = prev;
        prev = val;
      }
      row[a.length] = prev;
    }
    //return row[a.length];
    let tmp2 = row[a.length] / len;
    let res = 1 - tmp2;
    //console.log("Lev dist st1: ", a);
    //console.log("Lev dist st2: ", b);
    //console.log("Lev dist: ", res);
    //console.log("Lev dist: ", row[a.length], len, tmp2);
    return res;
  }

  //returns an HTML encoded string, useful for when we show an alert that is fed HTML formatted string, 
  //but therein we also put the name of a code (which could include things like "<", ">", etc...).
  //for example MainfFull.BulkAssignRemoveCodes(...)
  //from https://stackoverflow.com/a/7124052
  public static htmlEncode(str: string) {
    return str
      .replace(/&/g, '&amp;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\//g, '&#x2F;');
  }
  //the opposite of htmlEncode (hopefully!), not sure what for!
  public static htmlUnescape(str: string) {
    return str
      .replace(/&quot;/g, '"')
      .replace(/&#39;/g, "'")
      .replace(/&lt;/g, '<')
      .replace(/&gt;/g, '>')
      .replace(/&amp;/g, '&')
      .replace(/'&#x2F;'/g, '/');
  }

  public static DOILink(DOIst: string): string {
    if (DOIst.trim() == "") return "";
    else {
      const chk = DOIst.toLowerCase();
      const ind = chk.indexOf('doi.org/');
      if (chk.startsWith("http://")
        || chk.startsWith("https://")
      ) {
        if (ind > 6 && ind < 12) return DOIst;
        else return "";
      }
      else if (ind == -1 && chk.indexOf('/') > 0) {
        return "https://doi.org/" + DOIst;
      }
      else return "";
    }
  }
  //adapted from:https://stackoverflow.com/a/43467144 
  public static URLLink(UrlSt: string): string {
    if (UrlSt.trim() == "") return "";
    else {
      const st: string = UrlSt;
      let url;
      try {
        url = new URL(st);
      } catch (_) {
        return "";
      }
      if (url.protocol === "http:" || url.protocol === "https:") {
        return url.href;
      } else return "";
    }
  }


  //METHOD to ensure a given string does not contain bits that have special regex meanings
  //used when we need to dynamically create regexes from strings at runtime
  public static cleanSpecialRegexChars(input: string): string {
    //need to replace these: [\^$.|?*+(){}
    let result = input.replace(/\\/g, "\\\\");
    result = result.replace(/\[/g, "\\[");
    result = result.replace(/\^/g, "\\^");
    result = result.replace(/\$/g, "\\$");
    result = result.replace(/\./g, "\\.");
    result = result.replace(/\|/g, "\\|");
    result = result.replace(/\?/g, "\\?");
    result = result.replace(/\*/g, "\\*");
    result = result.replace(/\+/g, "\\+");
    result = result.replace(/\(/g, "\\(");
    result = result.replace(/\)/g, "\\)");
    result = result.replace(/\{/g, "\\{");
    result = result.replace(/\}/g, "\\}");
    //console.log(input, result);
    return result;
  }

  public static dataURItoBlob(dataURI: string): Blob {
    // convert base64/URLEncoded data component to raw binary data held in a string
    var byteString;
    if (dataURI.split(',')[0].indexOf('base64') >= 0)
      byteString = atob(dataURI.split(',')[1]);
    else
      byteString =  unescape(dataURI.split(',')[1]);

    // separate out the mime component
    var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

    // write the bytes of the string to a typed array
    var ia = new Uint8Array(byteString.length);
    for (var i = 0; i < byteString.length; i++) {
      ia[i] = byteString.charCodeAt(i);
    }

    let res = new Blob([ia], { type: mimeString });
    return res;
  }
}
export interface SubstituteString {
  searchFor: string;
  changeTo: string;
}
