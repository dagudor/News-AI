// src/api-client/configuracion.ts

const API_BASE_URL = 'http://localhost:5033/api'; // Ajusta según tu backend

export interface ConfiguracionData {
  hashtags: string | null;
  profundidad: string | null;
  tono: string | null;
  output?: string | null;
}

export async function guardarConfiguracion(data: ConfiguracionData): Promise<Response> {
  try {
    console.log('Enviando configuración:', data);
    
    const response = await fetch(`${API_BASE_URL}/configuracion/guardar`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify({
        usuarioId: 1, // Por ahora hardcodeado, luego implementar autenticación
        hashtags: data.hashtags || '',
        profundidadResumen: data.profundidad || 'breve',
        tonoResumen: data.tono || 'coloquial',
        accionResumen: data.output || 'email'
      })
    });

    return response;
  } catch (error) {
    console.error('Error en la llamada a la API:', error);
    throw error;
  }
}

export async function obtenerConfiguracion(usuarioId: number): Promise<Response> {
  try {
    const response = await fetch(`${API_BASE_URL}/configuracion/obtener/${usuarioId}`, {
      method: 'GET',
      headers: {
        'Accept': 'application/json'
      }
    });

    return response;
  } catch (error) {
    console.error('Error al obtener configuración:', error);
    throw error;
  }
}