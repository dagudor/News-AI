export interface ConfiguracionDTO {
  id: number;
  hashtags: string;
  gradoDesarrolloResumen: string;
  lenguaje: string;
  email: boolean;
  audio: boolean;
  usuarioId: number;
}

export interface UsuarioDTO {
  id: number;
  nombre: string;
  email: string;
  login: string;
  password: string;
  fechaAlta?: string; // formato ISO string
  configuraciones: ConfiguracionDTO[];
}
