import { ConfigService } from "../services/config.service";

export abstract class BusyAwareService {
  constructor(private configService: ConfigService) {

  }
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
  protected get _baseUrl(): string {
    return this.configService.baseUrl
  }
}
