import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppModuleShared } from './app.shared.module';
import { AppComponent } from './components/app/app.component';
import { JwtInterceptor } from './components/helpers/jwt.interceptor'
import { HomeComponent } from './components/home/home.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

@NgModule({
    bootstrap: [AppComponent
        
        
    ],
    imports: [
        BrowserModule,
        HttpClientModule,
        AppModuleShared
    ],
    providers: [
        //ReviewerIdentityService ,
        { provide: 'BASE_URL', useFactory: getBaseUrl },
        { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
    ]
})
export class AppModule {
    constructor() { }
}

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}
