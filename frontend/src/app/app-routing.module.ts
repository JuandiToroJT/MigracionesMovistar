import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MigraManualComponent } from './migra-manual/migra-manual.component';
import { LoginComponent } from './login/login.component';

const routes: Routes = [
    {path: 'Login', component: LoginComponent},
  {path: 'migra_manual', component: MigraManualComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
