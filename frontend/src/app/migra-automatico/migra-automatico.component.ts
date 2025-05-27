import { Component, OnInit } from '@angular/core';
import { ConexionApiService } from '../shared/conexion-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

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
    const url = 'http://localhost:3000/migraciones/1001/masiva'; // Ajusta el host si es necesario

    this.http.get(url).subscribe({
      next: (respuesta) => {
        console.log('Migración procesada:', respuesta);
        alert('Migración procesada correctamente.');
      },
      error: (error) => {
        console.error('Error en la migración:', error);
        alert('Error al procesar la migración.');
      }
    });
  }

    ngOnInit() {
      this.ConexionApiService.obtenerProcesos().subscribe(
        (data) => {
          this.listaProcesos = data;
          console.log("✅ lista de procesos - Datos cargados:");
          // console.log("✅ Datos cargados:", this.listaUsuarios);
        },
        (error) => {
          console.error("❌ Error al cargar los datos:", error);
        }
      );
    }
  }
