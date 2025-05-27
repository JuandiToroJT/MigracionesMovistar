import { Component, OnInit } from '@angular/core';
import { ConexionApiService } from '../shared/conexion-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import Swal from 'sweetalert2'

@Component({
  selector: 'app-migra-automatico',
  templateUrl: './migra-automatico.component.html',
  styleUrls: ['./migra-automatico.component.scss']
})
export class MigraAutomaticoComponent {
  public formulario: FormGroup;

  listaProcesos: any;

  constructor(
    private ConexionApiService: ConexionApiService,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,) {

    this.formulario = new FormGroup({

    });
  }

  procesarMigracion() {
    const url = 'http://localhost:44394/migraciones/1001/masiva'; // Ajusta el host si es necesario

    this.ConexionApiService.procesarMigracionMasiva().subscribe({
      next: (respuesta) => {
        this.cargarListado()
        Swal.fire({
          icon: "success",
          title: "✅ Migracion iniciada ",
          //text: ".",
        // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
      },
      error: (error) => {
        Swal.fire({
          icon: "error",
          title: " ❌ ocurrio un error: "+ error.error.mensajeError,
          //text: ".",
        // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
      }
    });
  }

  cargarListado(){
    this.ConexionApiService.obtenerProcesos().subscribe(
        (data) => {
          this.listaProcesos = data;
          console.log("✅ lista de procesos - Datos cargados:");
          // console.log("✅ Datos cargados:", this.listaUsuarios);
        },
        (error) => {
          Swal.fire({
          icon: "error",
          title: " ❌ ocurrio un error: "+ error.error.mensajeError,
          //text: ".",
        // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
        }
      );
  }

    ngOnInit() {
      this.cargarListado()
    }
  }
