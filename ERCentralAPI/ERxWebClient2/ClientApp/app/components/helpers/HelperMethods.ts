

//this class should only contain static methods.
//it's a collection of methods that can be used by any component/service. Main purpose is to avoid replication.
//Please include a short description of the purpose of any method added in this class.
export class Helpers {
    //Used to format dates when the date string is like "DD-MM-YYYY[...]".
    //By using toLocaleDateString() we should be showing the date in the locale-specific way that applies to the current client...
    public static FormatDate(DD_MM_YYYY_: string): string {
        if (!DD_MM_YYYY_ || DD_MM_YYYY_.length < 10) return "";
        else {
            const year = parseInt(DD_MM_YYYY_.substr(6, 4));
            const month = parseInt(DD_MM_YYYY_.substr(3, 2)) -1;
            const day = parseInt(DD_MM_YYYY_.substr(0, 2));
            const date: Date = new Date(year, month, day);
            return date.toLocaleDateString();
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
            return date.toLocaleDateString();
        }
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
    public static AddHTMLFrame(report: string, baseUrl: string, title?: string): string {
        //used to save reports
        if (title === undefined) title = ">EPPI-Reviewer Coding Report";
        let res = "<HTML id='content'><HEAD><title>"+ title +"</title><link rel='stylesheet' href='" + baseUrl + "/dist/vendor.css' /></HEAD><BODY class='m-2' id='body'>" + report;
        //res += "<br /><a download='report.html' href='data:text/html;charset=utf-8," + report + "'>Save...</a></BODY></HTML>";
        //res += "<br />" + this.AddSaveMe() + "</BODY></HTML>";
        res += "</BODY></HTML>";
        return res;
    }
    public static CleanHTMLforExport(text: string, substitutions: SubstituteString[]):string {
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
}
export interface SubstituteString {
    searchFor: string;
    changeTo: string;
}