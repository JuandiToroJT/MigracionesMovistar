
import { Component, OnInit } from '@angular/core';
import { ConexionApiService } from '../shared/conexion-api.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmailValidator, FormControl, FormGroup, Validators } from '@angular/forms';
import Swal from 'sweetalert2'
import { resgistro_usuarioModel } from '../shared/registro_usuario.model';


@Component({
  selector: 'app-registro-usuario',
  templateUrl: './registro-usuario.component.html',
  styleUrls: ['./registro-usuario.component.scss']
})
export class RegistroUsuarioComponent implements OnInit {

  public formulario_registro_usuario: FormGroup;

  registro_usuario = new resgistro_usuarioModel("", "", "", "", "", "");
  listaProcesos: any;

  cargando: boolean = false;

  constructor(
    private ConexionApiService: ConexionApiService,
    private route: ActivatedRoute,
    private router: Router,) {

    this.formulario_registro_usuario = new FormGroup({

      txtnombre: new FormControl('', Validators.required),
      txtemail: new FormControl('', [Validators.required, Validators.pattern('^[^@]+@[^@]+\.[a-zA-Z]{2,}$')]),
      txttel: new FormControl('', Validators.required),
      txtselecIdent: new FormControl('', Validators.required),
      txtnumIdent: new FormControl('', Validators.required),
      txtclave: new FormControl('', Validators.required)
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
    if (this.formulario_registro_usuario.invalid) {
      //this.formulario.markAllAsTouched(); // ← Marca todos los campos para que muestren errores
      Swal.fire({
        icon: "info",
        title: " Todos los campos son obligatorios",
        //text: ".",
        // footer: '<a href="#">Why do I have this issue?</a>'
      });
      return;
    }
    console.log("Formulario:", this.formulario_registro_usuario.value);

    // Actualizar el modelo con los datos del formulario
    this.registro_usuario = new resgistro_usuarioModel(
      this.formulario_registro_usuario.value.txtnombre.toString(),
      this.formulario_registro_usuario.value.txttel.toString(),
      this.formulario_registro_usuario.value.txtemail.toString(),
      this.formulario_registro_usuario.value.txtselecIdent.toString(),
      this.formulario_registro_usuario.value.txtnumIdent.toString(),
      this.formulario_registro_usuario.value.txtclave.toString(),

    );

    this.cargando = true; // Mostrar spinner
    this.ConexionApiService.RegistroUsuario(this.registro_usuario).subscribe({
      next: (respuesta) => {
        console.log('✅ registro subida exitosamente:', respuesta);
        Swal.fire({
          icon: "success",
          title: "✅ Registro de usuario realizado con exito ",
          //text: ".",
          // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
         this.router.navigate(['/migra_automatico']); // Redirigir tras éxito
      },
      error: (error) => {
        console.error('❌ ocurrio un error:', error.error.mensajeError);
        Swal.fire({
          icon: "error",
          title: " ❌ ocurrio un error: " + error.error.mensajeError,
          //text: ".",
          // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
        this.cargando = false; // Ocultar spinner en cualquier caso 
      }
    });
  }

  validacionNom() {
    const campo = this.formulario_registro_usuario.get('txtnombre');
    if (campo && campo.invalid && campo.touched) {

      Swal.fire({
        icon: "info",
        title: " Nombre es obligatorio",
        text: "Campo obligatorio",
        // footer: '<a href="#">Why do I have this issue?</a>'
      });
    }
  }
  validacionTipoident() {
    const campo = this.formulario_registro_usuario.get('txtselecIdent');
    if (campo && campo.invalid && campo.touched) {

      Swal.fire({
        icon: "info",
        title: " tipo de identificacion es obligatorio",
        text: "Campo obligatorio",
        // footer: '<a href="#">Why do I have this issue?</a>'
      });
    }
  }
  validaciontel() {
    const campo = this.formulario_registro_usuario.get('txttel');
    if (campo && campo.invalid && campo.touched) {

      Swal.fire({
        icon: "info",
        title: " Numero de telefono obligatorio",
        text: "Campo obligatorio",
        // footer: '<a href="#">Why do I have this issue?</a>'
      });
    }
  }

  validacionEmail() {
    const campo = this.formulario_registro_usuario.get('txtemail');
    if (campo && campo.invalid && campo.touched) {

      Swal.fire({
        icon: "info",
        title: " Correo es obligatorio",
        text: "Campo obligatorio",
        // footer: '<a href="#">Why do I have this issue?</a>'
      });
    }
  }
  validacionCedula(): void {
    const campo = this.formulario_registro_usuario.get('txtnumIdent');

    if (campo && campo.invalid && campo.touched) {
      Swal.fire({
        icon: 'info',
        title: 'Número de identificacion es obligatorio',
        text: 'Campo obligatorio',
      });
    }
  }
  validacionClave(): void {
    const campo = this.formulario_registro_usuario.get('txtclave');

    if (campo && campo.invalid && campo.touched) {
      Swal.fire({
        icon: 'info',
        title: 'clave es obligatoria',
        text: 'Campo obligatorio',
      });
    }
  }

  validarTodosCampo() {
    if (this.formulario_registro_usuario.invalid) {
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
  exit(): void {
    Swal.fire({
      title: '¿Estás seguro de que deseas salir?',
      text: 'Perderás los cambios no guardados.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sí, salir',
      cancelButtonText: 'Cancelar'
    }).then((resultado) => {
      if (resultado.isConfirmed) {
        this.router.navigate(['/Login']);
      }
    });
  }
}

