import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MigraManualComponent } from './migra-manual/migra-manual.component';
import { LoginComponent } from './login/login.component';
import { MigraAutomaticoComponent } from './migra-automatico/migra-automatico.component';

const routes: Routes = [
  { path: '', redirectTo: 'migra_automatico', pathMatch: 'full' },
    {path: 'Login', component: LoginComponent},
  {path: 'migra_manual', component: MigraManualComponent},
  { path: 'migra_automatico', component: MigraAutomaticoComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
