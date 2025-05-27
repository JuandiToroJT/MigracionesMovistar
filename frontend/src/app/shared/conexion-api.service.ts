import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { miagra_manualModel } from './migra_manual.model';


@Injectable({
  providedIn: 'root'
})
export class ConexionApiService {

  BASE_URL_GET = 'https://localhost:44394'

  constructor(private http: HttpClient, private router: Router) { }

   obtenerProcesos() {
    return this.http.get(this.BASE_URL_GET + '/migraciones/procesos');
  }

   agregarMigraManual(Migra_manual: miagra_manualModel) {
    console.log("api",Migra_manual )
    return this.http.post<string>(`${this.BASE_URL_GET}/migraciones/${Migra_manual.identificacion}/manual`, Migra_manual)
     
  }

  procesarMigracionMasiva() {
    return this.http.post<string>(`${this.BASE_URL_GET}/migraciones/1001/masiva`, null)
     
  }
}
