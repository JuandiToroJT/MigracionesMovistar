import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HttpClient } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { MigraManualComponent } from './migra-manual/migra-manual.component';
import { LoginComponent } from './login/login.component';
import { MigraAutomaticoComponent } from './migra-automatico/migra-automatico.component';
import { AuditoriaComponent } from './auditoria/auditoria.component';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RegistroUsuarioComponent } from './registro-usuario/registro-usuario.component';
import { PanelProcessComponent } from './panel-process/panel-process.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgxEchartsModule } from 'ngx-echarts';


@NgModule({
  declarations: [
    AppComponent,
    MigraManualComponent,
    LoginComponent,
    MigraAutomaticoComponent,
    RegistroUsuarioComponent,

    PanelProcessComponent,
    AuditoriaComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule, // ✅ Agrega esto
    NgxPaginationModule,
    FormsModule,
     NgxEchartsModule.forRoot({
      echarts: () => import('echarts')
    }),
  ],
  providers: [
    HttpClient
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
