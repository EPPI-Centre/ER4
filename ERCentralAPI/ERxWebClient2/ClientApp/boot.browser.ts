import 'reflect-metadata';
import * as $ from 'jquery';
import 'zone.js';
import 'bootstrap';
import 'angular-tree-component';
import 'angular-datatables';
import 'datatables.net';
import 'datatables.net-dt'
import 'angular-tree-component/dist/angular-tree-component.css';
import { enableProdMode, NgModuleRef, ApplicationRef } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.browser.module';
import { createNewHosts } from '@angularclass/hmr';
//import 'font-awesome/css/font-awesome.css';



const modulePromise1 = () => platformBrowserDynamic().bootstrapModule(AppModule);;


if (module.hot) {
    let ngModule: NgModuleRef<any>;
    module.hot.accept();

    modulePromise1().then(mod => ngModule = mod);
    module.hot.dispose(() => {
        const appRef: ApplicationRef = ngModule.injector.get(ApplicationRef);
        const elements = appRef.components.map(c => c.location.nativeElement);
        const makeVisible = createNewHosts(elements);
        ngModule.destroy();
        makeVisible();
    });
} else {
    enableProdMode();
}

// Note: @ng-tools/webpack looks for the following expression when performing production
// builds. Don't change how this line looks, otherwise you may break tree-shaking.
const modulePromise = platformBrowserDynamic().bootstrapModule(AppModule);