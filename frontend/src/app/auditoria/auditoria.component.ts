import { Component, OnInit } from '@angular/core';
import { ConexionApiService } from '../shared/conexion-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import Swal from 'sweetalert2';

@Component({
  selector: 'auditoria',
  templateUrl: './auditoria.component.html',
  styleUrls: ['./auditoria.component.scss'],
})
export class AuditoriaComponent {

  listaAuditoria: any;

  constructor(
    private ConexionApiService: ConexionApiService,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) {

  }

  cargarListado() {
    this.ConexionApiService.obtenerAuditoria().subscribe(
      (data) => {
        this.listaAuditoria = data;
      },
      (error) => {
        Swal.fire({
          icon: 'error',
          title: ' ❌ ocurrio un error: ' + error.error.mensajeError,
          //text: ".",
          // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
      }
    );
  }

  ngOnInit() {
    this.cargarListado();
  }
}
