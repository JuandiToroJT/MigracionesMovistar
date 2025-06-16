import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { miagra_manualModel } from './migra_manual.model';


@Injectable({
  providedIn: 'root'
})
export class ConexionApiService {

  BASE_URL_GET = 'https://migracionproyectjt-d0bpe4g9d4eugzbc.canadacentral-01.azurewebsites.net/'

  constructor(private http: HttpClient, private router: Router) { }

   obtenerProcesos() {
    return this.http.get(this.BASE_URL_GET + '/procesos');
  }

   agregarMigraManual(Migra_manual: miagra_manualModel) {
    console.log("api",Migra_manual )
    return this.http.post<string>(`${this.BASE_URL_GET}/migraciones/${Migra_manual.identificacion}/manual`, Migra_manual)
     
  }

  procesarMigracionMasiva() {
    return this.http.post<string>(`${this.BASE_URL_GET}/migraciones/123/masiva`, null)
     
  }
}
