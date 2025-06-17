export class panel_procesosModel {
    constructor(
        public idProceso: string,
        public tipo: string,
        public estado: string,
        public total: string,
        public exitosos: number,
        public errores: number,
        public duplicados: number,
        public fecha: string,
        public notas: string
    ) { }
}

