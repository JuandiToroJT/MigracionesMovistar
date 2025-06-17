import { HttpClient } from '@angular/common/http';
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Data, Router } from '@angular/router';
import { ConexionApiService } from '../shared/conexion-api.service';
import { panel_procesosModel } from '../shared/panel_procesos.model';
import type { EChartsOption } from 'echarts';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-panel-process',
  templateUrl: './panel-process.component.html',
  styleUrls: ['./panel-process.component.scss']
})
export class PanelProcessComponent implements OnInit {

  public formulario: FormGroup;

  panel_process: panel_procesosModel[]=[];
     busqueda: string = '';


 listaProcesos: any ;
 datosFiltrados = [...this.panel_process];
  page: number = 1;
  itemsPerPage: number = 8;

  process: panel_procesosModel | any;

  option: EChartsOption = {}; // gráfico inicial vacío

  constructor(
    private ConexionApiService: ConexionApiService,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) {
    this.formulario = new FormGroup({});
  }
  ngOnInit() {
    this.mostrar_listado();
    this.resetChart();
this.ConexionApiService.obtenerProcesosPanel().subscribe((data) => {
  this.listaProcesos = data;
const procesos = data as any[];
  // 👇 sumar totales
  this.process = {
    exitosos: 0,
    errores: 0,
    duplicados: 0
  };

  procesos.forEach((p: any) => {
    this.process.exitosos += p.exitosos || 0;
    this.process.errores += p.errores || 0;
    this.process.duplicados += p.duplicados || 0;
  });

  console.log('Totales:', this.process);

  // 🧨 limpiar gráfico anterior
  this.resetChart();

  // ⏱️ generar gráfico
  setTimeout(() => {
    this.option = {
      color: ['#25f111','#f13811','#f1e311'],
      title: { text: 'Resumen de procesos', left: 'center' },
      tooltip: { trigger: 'item' as const },
      legend: { bottom: 'bottom' },
      series: [
        {
          name: 'Procesos',
          type: 'pie',
          radius: '80%',
          data: [
            { value: this.process.exitosos, name: 'Éxitos' },
            { value: this.process.errores, name: 'Errores' },
            { value: this.process.duplicados, name: 'Duplicados' }
          ],
          emphasis: {
            itemStyle: {
              shadowBlur: 10,
              shadowOffsetX: 0,
              shadowColor: 'rgba(0, 0, 0, 0.5)'
            }
          }
        }
      ]
    };
  }, 0);
});

  }
 

 OnSumit(){
  
 }
@ViewChild('chart', { static: false }) chartComponent: any;
resetChart() {
  const chartInstance = this.chartComponent?.echartsInstance;
  if (chartInstance) {
    chartInstance.dispose(); // 🔥 destruye la instancia previa
  }
}

 mostrar_listado(){
this.ConexionApiService.obtenerProcesosPanel().subscribe(
    (data)=> {
     this.listaProcesos = data;
    }
  )
 }

buscar() {
  const texto = this.busqueda.toLowerCase();
  this.datosFiltrados = this.panel_process.filter(d =>
    d.tipo.toLowerCase().includes(texto) || String(d.estado).includes(texto)
  );
  this.page = 1;
}
}
