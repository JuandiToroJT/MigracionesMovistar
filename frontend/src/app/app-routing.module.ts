import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MigraManualComponent } from './migra-manual/migra-manual.component';
import { LoginComponent } from './login/login.component';
import { MigraAutomaticoComponent } from './migra-automatico/migra-automatico.component';
import { RegistroUsuarioComponent } from './registro-usuario/registro-usuario.component';
import { PanelProcessComponent } from './panel-process/panel-process.component';
import { AuditoriaComponent } from './auditoria/auditoria.component';


const routes: Routes = [
  { path: '', redirectTo: 'Login', pathMatch: 'full' },
  { path: 'Login', component: LoginComponent },
  { path: 'migra_manual', component: MigraManualComponent },
  { path: 'migra_automatico', component: MigraAutomaticoComponent },
 { path: 'registro_usuario', component: RegistroUsuarioComponent },
  { path: 'panel_process', component: PanelProcessComponent },


    { path: 'registro_usuario', component: RegistroUsuarioComponent },
    { path: 'auditoria', component: AuditoriaComponent },

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
