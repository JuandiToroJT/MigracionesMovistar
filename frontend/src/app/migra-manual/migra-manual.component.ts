import { Component, OnInit } from '@angular/core';
import { ConexionApiService } from '../shared/conexion-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-migra-manual',
  templateUrl: './migra-manual.component.html',
  styleUrls: ['./migra-manual.component.scss']
})
export class MigraManualComponent implements OnInit {

    public formulario: FormGroup;

    listaProcesos: any;

  constructor(
    private ConexionApiService: ConexionApiService,
    private route: ActivatedRoute,
    private router: Router,) {

       this.formulario = new FormGroup({

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
