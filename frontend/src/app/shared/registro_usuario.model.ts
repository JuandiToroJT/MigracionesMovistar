export class resgistro_usuarioModel {
    constructor(
        public nombre: string,
        public correo: string,
        public celular: string,
        public tipoIdentificacion: string,
        public identificacion: string,
        public clave: string
    ) { }
}