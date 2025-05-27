import { Component, OnInit } from '@angular/core';
import { ConexionApiService } from '../shared/conexion-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { EmailValidator, FormControl, FormGroup, Validators } from '@angular/forms';
import { miagra_manualModel } from '../shared/migra_manual.model';
import Swal from 'sweetalert2'


@Component({
  selector: 'app-migra-manual',
  templateUrl: './migra-manual.component.html',
  styleUrls: ['./migra-manual.component.scss']
})
export class MigraManualComponent implements OnInit {

  public formulario_migra_manual: FormGroup;

  migracion_manual = new miagra_manualModel("", "", "", "");
  listaProcesos: any;


  constructor(
    private ConexionApiService: ConexionApiService,
    private route: ActivatedRoute,
    private router: Router,) {

    this.formulario_migra_manual = new FormGroup({

      txttel: new FormControl('', Validators.required),
      txtcuenta: new FormControl('', Validators.required),
      txtemail: new FormControl('', [Validators.required, Validators.pattern('^[^@]+@[^@]+\.[a-zA-Z]{2,}$')]),
      txtcedula: new FormControl('', Validators.required)
    });
  }


  ngOnInit() {

    // this.ConexionApiService.obtenerProcesos().subscribe(
    //   (data) => {
    //     this.listaProcesos = data;
    //     console.log("✅ lista de procesos - Datos cargados:");
    //     // console.log("✅ Datos cargados:", this.listaUsuarios);
    //   },
    //   (error) => {
    //     console.error("❌ Error al cargar los datos:", error);
    //   }
    // );
  }
  onSumit() {
    if (this.formulario_migra_manual.invalid) {
    //this.formulario.markAllAsTouched(); // ← Marca todos los campos para que muestren errores
     Swal.fire({
             icon: "info",
             title: " Campos incorrectos",
             //text: ".",
            // footer: '<a href="#">Why do I have this issue?</a>'
           });
    return;
          }
    console.log("Formulario:", this.formulario_migra_manual.value);

    // Actualizar el modelo con los datos del formulario
    this.migracion_manual = new miagra_manualModel(
      this.formulario_migra_manual.value.txtcedula.toString(),
      this.formulario_migra_manual.value.txttel.toString(),
      this.formulario_migra_manual.value.txtcuenta.toString(),
      this.formulario_migra_manual.value.txtemail.toString(),
    );

  this.ConexionApiService.agregarMigraManual(this.migracion_manual).subscribe({
        next: (respuesta) => {
          console.log('✅ Imagen subida exitosamente:', respuesta);
           Swal.fire({
             icon: "success",
             title: "✅ Migracion realizada con exito ",
             //text: ".",
            // footer: '<a href="#">Why do I have this issue?</a>'
             timer: 2100,
           });
          // this.router.navigate(['/listaUsuarios']); // Redirigir tras éxito
        },
        error: (error) => {
          console.error('❌ ocurrio un error:', error.error.mensajeError);
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

  validacionCampos(){
    this.formulario_migra_manual.get('txttel')?.valid
     Swal.fire({
             icon: "info",
             title: " campo Obilgatorio",
             //text: ".",
            // footer: '<a href="#">Why do I have this issue?</a>'
           });
  }

  validarTodosCampo(){
    if (this.formulario_migra_manual.invalid) {
    //this.formulario.markAllAsTouched(); // ← Marca todos los campos para que muestren errores
     Swal.fire({
             icon: "info",
             title: " todos los campos son  Obilgatorio",
             //text: ".",
            // footer: '<a href="#">Why do I have this issue?</a>'
           });
    return;
  }
  }
  }

