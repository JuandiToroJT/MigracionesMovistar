import { Component, OnInit } from '@angular/core';
import { ConexionApiService } from '../shared/conexion-api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-migra-automatico',
  templateUrl: './migra-automatico.component.html',
  styleUrls: ['./migra-automatico.component.scss'],
})
export class MigraAutomaticoComponent {
  public formulario: FormGroup;

  listaProcesos: any;

  constructor(
    private ConexionApiService: ConexionApiService,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) {
    this.formulario = new FormGroup({});
  }

  procesarMigracion() {
    const url =
      'https://migracionproyectjt-d0bpe4g9d4eugzbc.canadacentral-01.azurewebsites.net/migraciones/123/masiva'; // Ajusta el host si es necesario

    this.ConexionApiService.procesarMigracionMasiva().subscribe({
      next: (respuesta) => {
        this.intervalo = setInterval(() => {
          this.cargarListado();
        }, 400);
        Swal.fire({
          icon: 'success',
          title: '✅ Migracion iniciada ',
          //text: ".",
          // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
      },
      error: (error) => {
        Swal.fire({
          icon: 'error',
          title: ' ❌ ocurrio un error: ' + error.error.mensajeError,
          //text: ".",
          // footer: '<a href="#">Why do I have this issue?</a>'
          timer: 2100,
        });
      },
    });
  }

  cargarListado() {
    this.ConexionApiService.obtenerProcesos().subscribe(
      (data) => {
        this.listaProcesos = data;
        if (
          Array.isArray(this.listaProcesos) &&
          this.listaProcesos.length > 0
        ) {
          const todosFinalizados = this.listaProcesos.every(
            (proceso: any) =>
              proceso.estado &&
              (proceso.estado.toLowerCase() === 'fin' ||
                proceso.estado.toLowerCase() === 'err')
          );
          if (todosFinalizados && this.intervalo) {
            clearInterval(this.intervalo);
            this.intervalo = null;
          }
        }
        console.log('✅ lista de procesos - Datos cargados:');
        // console.log("✅ Datos cargados:", this.listaUsuarios);
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
  getPorcentaje(valor: number, total: number): number {
    if (!total || total === 0) return 0;
    return Math.round((valor / total) * 100);
  }
  intervalo: any;
  currentPage: number = 1;
  itemsPerPage: number = 9;

  get procesosPaginados(): any[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    return this.listaProcesos.slice(start, end);
  }

  get totalPaginas(): number[] {
    return Array.from(
      { length: Math.ceil(this.listaProcesos.length / this.itemsPerPage) },
      (_, i) => i + 1
    );
  }

  cambiarPagina(pagina: number) {
    if (pagina >= 1 && pagina <= this.totalPaginas.length) {
      this.currentPage = pagina;
    }
  }
  archivoBase64: string | null = null;
  nombreArchivo: string = '';
  usuario: string = '123'; // Puedes obtenerlo dinámicamente si lo deseas

  onArchivoSeleccionado(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const archivo = input.files[0];
      this.nombreArchivo = archivo.name;

      const reader = new FileReader();
      reader.onload = () => {
        const contenido = reader.result as string;
        // Base64 sin el prefijo data:
        this.archivoBase64 = contenido.split(',')[1];
      };
      reader.readAsDataURL(archivo);
    }
  }

  subirArchivoExcel() {
    const extension =
      this.nombreArchivo.split('.').pop()?.toLowerCase() || null;
    const payload = {
      archivo: this.archivoBase64,
      formato: extension,
    };

    const url = `https://migracionproyectjt-d0bpe4g9d4eugzbc.canadacentral-01.azurewebsites.net/cargue/${this.usuario}/masivo`;

    this.http.post(url, payload).subscribe({
      next: () => {
        this.intervalo = setInterval(() => {
          this.cargarListado();
        }, 400);
        Swal.fire({
          icon: 'success',
          title: '✅ Archivo enviado correctamente',
          timer: 2000,
        });
        this.archivoBase64 = null;
        this.nombreArchivo = '';
      },
      error: (error) => {
        Swal.fire({
          icon: 'error',
          title: '❌ Error al subir el archivo',
          text: error.error?.mensajeError || 'Ocurrió un error inesperado.',
        });
      },
    });
  }
  mostrarModal: boolean = false;
  procesoSeleccionado: number | null = null;
  historialProceso: any[] = [];
  verHistorial(idProceso: number) {
    const url = `https://migracionproyectjt-d0bpe4g9d4eugzbc.canadacentral-01.azurewebsites.net/procesos/detalle?idProceso=${idProceso}`;
    this.http.get<any[]>(url).subscribe({
      next: (data) => {
        this.historialProceso = data;
        this.procesoSeleccionado = idProceso;
        this.mostrarModal = true;
      },
      error: (err) => {
        Swal.fire('Error', 'No se pudo cargar el historial.', 'error');
      },
    });
  }

  cerrarModal() {
    this.mostrarModal = false;
    this.procesoSeleccionado = null;
    this.historialProceso = [];
  }

  ngOnInit() {
    this.cargarListado();
    this.intervalo = setInterval(() => {
      this.cargarListado();
    }, 400);
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
      cancelButtonText: 'Cancelar',
    }).then((resultado) => {
      if (resultado.isConfirmed) {
        this.router.navigate(['/Login']);
      }
    });
  }
  exit1(): void {
    Swal.fire({
      title: '¿Estás seguro ingresar a formulario de migracion manual?',
      text: 'Perderás los cambios no guardados.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sí, salir',
      cancelButtonText: 'Cancelar',
    }).then((resultado) => {
      if (resultado.isConfirmed) {
        this.router.navigate(['/migra_manual']);
      }
    });
  }
}
