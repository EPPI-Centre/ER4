import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule  } from '@angular/common/http';
import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { FormsModule } from '@angular/forms';
import { Test1Component } from './test1/test1.component';
import { ReviewerIdentityService } from './Services/revieweridentity.service';

@NgModule({
  declarations: [
    AppComponent
    , LoginComponent, Test1Component
    
  ],
  imports: [
    BrowserModule
    , HttpClientModule 
    , FormsModule
    , RouterModule.forRoot([])
  ],
  providers: [
    { provide: 'BASE_URL', useFactory: getBaseUrl }
    , { provide: 'API_BASE_URL', useFactory: getAPIBaseUrl }],

  bootstrap: [AppComponent]
})
export class AppModule {
  constructor(
    //private ReviewerIdentity: ReviewerIdentityService
  ) { }
}

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}
export function getAPIBaseUrl() {
  let tmp: string = document.getElementsByTagName('base')[0].href;
  let i: number = tmp.lastIndexOf(':');
  if (i != -1) {
    tmp = tmp.substr(0, i);
    return tmp + '/ERxWebClient2/';
  }
  return tmp;
}
