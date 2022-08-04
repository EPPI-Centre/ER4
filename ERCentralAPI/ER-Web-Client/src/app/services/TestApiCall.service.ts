import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class TestApiCallService {
  private _baseUrl: string;
  ERversionN: string = "N/A";
  ERversionMessage: string = "N/A";
  ERversionUrl: string = "N/A";
  constructor(private _httpC: HttpClient
    , configService: ConfigService
  ) {
    this._baseUrl = configService.baseUrl + "/";
  }

  public GetMessage(): void {
    this._httpC.get<versionInfo>(this._baseUrl + 'api/Login/VersionInfo'
      //, {
      //headers:
      //{
      //  'Content-Type': 'application/json',
      //  'Access-Control-Allow-Origin': 'http://localhost:4200',
      //  'Access-Control-Allow-Credentials': 'true',
      //  'Access-Control-Allow-Headers': 'Content-Type',
      //    'Access-Control-Allow-Methods': 'GET,PUT,POST,DELETE'
      //  }
      //}
    ).subscribe(
      result => {
        this.ERversionN = result.versionN;
        this.ERversionMessage = result.description;
        this.ERversionUrl = result.url;
      }, error => {
        console.error(error);
      }
    );
  }
}
class versionInfo {
  date: string = "";
  description: string = "";
  url: string = "";
  versionN: string = "";
}
