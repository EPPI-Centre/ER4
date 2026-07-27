import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { ConfirmationDialogService } from './confirmation-dialog.service';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  constructor(
    private _httpC: HttpClient,
    private confirmationDialogService: ConfirmationDialogService
  ) { }

  public baseUrl: string = "https://localhost:1234";
  public EnableGPTInvestigateGlobally: boolean = false;
  public GPTinvestigateEnabledAccounts: number[] = [];
  public vInfo: versionInfo = new versionInfo();
  public versionIsNew: boolean = false;
  public HasConnectionError: boolean = false;
  public IsGettingVersionInfo: boolean = true;

  public getVinfo() {
    this.HasConnectionError = false;
    this.IsGettingVersionInfo = true;
    lastValueFrom(this._httpC.get<versionInfo>(this.baseUrl + 'api/Login/VersionInfo')
    ).then(
      result => {
        this.IsGettingVersionInfo = false;
        const oldVinfo = this.vInfo;
        this.vInfo = result;
        if (this.vInfo.versionN.startsWith("4.")) {
          this.vInfo.versionN = "6" + this.vInfo.versionN.substring(1);
        }
        this.CheckIfVersionIsNew(oldVinfo);
      }, error => {
        this.HasConnectionError = true;
        this.IsGettingVersionInfo = false;
        console.error(error);
      }
    );
  }

  private CheckIfVersionIsNew(oldVinfo: versionInfo) {
    if (oldVinfo.versionN != "" && oldVinfo.versionN != this.vInfo.versionN) {
      //client just learned that the vInfo has changed, so we need to reload the client.
      this.confirmationDialogService.confirm("Update your Client?"
        , "EPPI Reviewer has just been <strong>updated</strong>!<br />"
        + "As a consequence, the client that you're using right now is probably <strong>outdated</strong> and might not work correctly.<br />"
        + "Click <em>\"Reload\"</em> to refresh this page, which will automatically fetch the up-to-date version of the EPPI Reviewer client."
        , false, "", "Reload", "Cancel")
        .then((res: any) => {
          if (res === true) {
            try { window.location.reload(); }
            catch { alert("Auto refresh failed - please click on \"Refresh\\Reload\\F5\" manually."); }
          }
        }
        ).catch(() => { });
    }
    const date = new Date();
    date.setDate(date.getDate() - 10);
    const vDT = this.vInfo.date.split(' ');
    if (!vDT || vDT.length != 2) {
      this.versionIsNew = false;
      return;
    }
    const dmy = vDT[0].split('/');
    if (!dmy || dmy.length != 3) {
      this.versionIsNew = false;
      return;
    }
    const vD = new Date(parseInt(dmy[2]), parseInt(dmy[1]) - 1, parseInt(dmy[0]));
    if (vD > date) this.versionIsNew = true;
    else this.versionIsNew = false;
  }
}
export class versionInfo {
  date: string = "";
  description: string = "";
  url: string = "";
  versionN: string = "";
}
