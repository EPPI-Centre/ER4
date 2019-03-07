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
}