import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ConexionApiService } from '../shared/conexion-api.service';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  user: string | any = {
    usuario: '',
    clave: '',

  }

   public formulario: FormGroup;


  constructor(private router: Router,  private ConexionApiService: ConexionApiService) {

    this.formulario = new FormGroup({
      txtusuario: new FormControl('', Validators.required),
      txtpass: new FormControl('', Validators.required)

    });
  }
  ngOnInit() {
    
  }

  login() {
     if (this.formulario.invalid) {
       Swal.fire({
              icon: "info",
              title: " Todos los campos son obligatorios",
              //text: ".",
              // footer: '<a href="#">Why do I have this issue?</a>'
            });
      return;
    }
   this.ConexionApiService.obtenerLogin(this.user).subscribe({
  next: (response) => {
    // Mensaje de bienvenida
    Swal.fire({
      title: "Bienvenido",
      text: "Hola", // Aquí puedes personalizar con el nombre del usuario si lo tienes: `Hola ${response.nombre}`
      icon: "success",
      timer: 1500,
      showConfirmButton: false
    });

    // Redirigir al home
    this.router.navigate(['/migra_automatico']);
  },
  error: () => {
    // Mensaje de error
    Swal.fire({
      icon: "error",
      title: "Datos Incorrectos",
      text: "Usuario o contraseña incorrectos!",
    });
  }
});
  }
}
