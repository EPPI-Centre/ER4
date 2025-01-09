import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  baseUrl: string = "https://localhost:1234";
  EnableGPTInvestigateGlobally: boolean = false;
  GPTinvestigateEnabledAccounts: number[] = [];

  constructor() { }
}
