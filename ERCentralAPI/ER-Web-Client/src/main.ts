/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */

import { enableProdMode, provideZoneChangeDetection } from '@angular/core';
import { platformBrowser } from "@angular/platform-browser";
//import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

platformBrowser(providers).bootstrapModule(AppModule, { applicationProviders: [provideZoneChangeDetection()], })
  .catch(err => console.log(err));
//platformBrowserDynamic(providers).bootstrapModule(AppModule, { applicationProviders: [provideZoneChangeDetection()], }).catch(err => console.log(err));
