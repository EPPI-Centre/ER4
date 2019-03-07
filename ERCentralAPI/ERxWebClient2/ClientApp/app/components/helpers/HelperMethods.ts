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
}