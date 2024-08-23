# EPPI Reviewer 6 (ER6) Client ReadMe

The purpose of this readme is mostly to mention dependencies and licensing requirements for the ER-Web-Client project, which constitutes the ER6 Client.

This project is a straightforward **Angular** project created using a Visual Studio template called "Standalone TypeScript Angular project".

Importantly, this project was created without ticking the "Add integration for Empty ASP.NET Web API Project" option, which means:

- It does not use the Angular client-side proxy to route API calls from client to server side (would be used in development phase only).
- The project is closer to a vanilla Angular app in some ways (no additional, pre configured proxy).
- It includes one special (ad-hoc) provision to allow specifying "where" the API endpoints are to be found.

For more details about how the ER6 Client "works together" with the ER6 API, please see the [ER6-README](../ER6-README.md) file.

A the bottom of this file you can find the "original" readme file created by the template itself. This clarifies what was the original Angular CLI version used and contains some generic links and hints.

## Dependencies and Licensing

At the time of writing (Aug 2024), this project uses Angular v.14.x, which rests on Node.js (v.16.20.0 is currently installed). The Angular CLI version is 13.3.9.
Naturally, the project depends on a number of npm packages (and the astronomical amount of packages they depend on!), as per specification in the `package.json` file.
To the best of our knowledge, the only packages for which a paid-for license is required are the ones from progress/telerik. Developer licenses can be bought from the [Telerik](https://www.telerik.com) website, the product to be licensed is called **Kendo UI for Angular**.

Please note that npm packages will be downloaded automatically upon running/debugging this project, this includes the Telerik packages. If all works as expected (it's not always the case!), you will be able to run this project "automatically", which however does NOT mean you do not have the obligation to procure your own Kendo UI license!

The ER6 client also depends on the [PDFTron WebViewer](https://www.pdftron.com/webviewer/index.html). This is a pure Javascript commercial product for which you will also need to procure a license. The licensed files are stored in `[..]\ERCentralAPI\ER-Web-Client\src\assets\lib\` within this repository.

------------------

# (Automatically generated) ERWebClient Original Readme

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 13.3.6.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.
