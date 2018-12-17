export class BusyAwareService {
    protected _BusyMethods: string[] = [];
    //private _IsBusy: boolean = false;
    public get IsBusy(): boolean {
        return this._BusyMethods.length != 0;
    }
    protected RemoveBusy(MethodName: string) {
        var index = this._BusyMethods.indexOf(MethodName);
        if (index > -1) {
            this._BusyMethods.splice(index, 1);
        }
    }
}