import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { miagra_manualModel } from './migra_manual.model';
import { LoginModel } from './login.model';
import { resgistro_usuarioModel } from './registro_usuario.model';


@Injectable({
  providedIn: 'root'
})
export class ConexionApiService {

  BASE_URL_GET = 'https://migracionproyectjt-d0bpe4g9d4eugzbc.canadacentral-01.azurewebsites.net/'


  constructor(private http: HttpClient, private router: Router) { }

   obtenerProcesos() {
    return this.http.get(this.BASE_URL_GET + '/procesos');
  }

  obtenerAuditoria() {
    return this.http.get(this.BASE_URL_GET + '/auditoria/123456789');
  }

   agregarMigraManual(Migra_manual: miagra_manualModel) {
    console.log("api",Migra_manual )
    return this.http.post<string>(`${this.BASE_URL_GET}/migraciones/${Migra_manual.identificacion}/manual`, Migra_manual)
     
  }

  procesarMigracionMasiva() {
    return this.http.post<string>(`https://localhost:44394/migraciones/123456789/masiva`, null)
     
  }

   obtenerLogin(id: LoginModel)  {
    return this.http.post<LoginModel[]>(`${this.BASE_URL_GET}/usuario/autenticar`, id);
  }

   RegistroUsuario(registro_usuario: resgistro_usuarioModel) {
    return this.http.post<string>(`${this.BASE_URL_GET}/usuario/registrar`, registro_usuario)
     
  }

}
