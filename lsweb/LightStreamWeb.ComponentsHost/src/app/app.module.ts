import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Injector } from '@angular/core';
import { createCustomElement } from '@angular/elements';

import { AppComponent } from './app.component';
import { LsComponentModule, LsWebComponentService } from '@lightstream/marketing-components';

@NgModule({
  imports: [
    BrowserModule,
    LsComponentModule
  ],
  declarations: [
    AppComponent
  ],
  bootstrap: [],
  entryComponents: []
})

export class AppModule {

  // just one component for now.  add or modify more as needed.
  componentsToLoad = [
    'lswc-ratings-reviews',
    'lswc-section',
    'lswc-legal-banner'
  ];

  constructor(
    private injector: Injector,
    private webComponentService: LsWebComponentService
  ) { }

  ngDoBootstrap() {
    this.webComponentService.bootstrapWebComponents(this.injector, new Set(this.componentsToLoad));
  }

}
