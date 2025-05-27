import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';


@Injectable({
  providedIn: 'root'
})
export class ConexionApiService {

  BASE_URL_GET = 'https://localhost:44394'

  constructor(private http: HttpClient, private router: Router) { }

   obtenerProcesos() {
    return this.http.get(this.BASE_URL_GET + '/migraciones/procesos');
  }
}
