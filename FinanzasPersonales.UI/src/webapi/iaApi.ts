// 🤖 API para integración con IA
import http from './http';
import axios from 'axios';

const parseError = (error: unknown): Error => {
  if (axios.isAxiosError(error)) {
    const backendMessage = (error.response?.data as any)?.message;
    if (backendMessage) return new Error(String(backendMessage));
  }
  return error instanceof Error ? error : new Error('Error desconocido');
};

/**
 * Request para procesar texto libre
 */
export interface TextoLibreRequest {
  texto: string;
  usuarioId: string;
}

/**
 * Categoría estructurada por IA
 */
export interface CategoriaIA {
  id: string | null;
  nombre: string | null;
  tipo: 'Ingreso' | 'Gasto' | null;
  usuarioId: string;
}

/**
 * Transacción estructurada por IA
 */
export interface TransaccionIA {
  id: string | null;
  tipo: 'Ingreso' | 'Gasto' | null;
  monto: number | null;
  descripcion: string | null;
  categoriaId: string | null;
  fecha: string | null;
  usuarioId: string;
}

/**
 * Respuesta del procesamiento de IA
 */
export interface RespuestaIA {
  categoria: CategoriaIA;
  transaccion: TransaccionIA;
  textoOriginal: string;
  advertencias: string[];
}

/**
 * Request para confirmar transacción
 */
export interface ConfirmarTransaccionRequest {
  categoria: CategoriaIA;
  transaccion: TransaccionIA;
  usuarioId: string;
}

/**
 * Procesa texto libre y devuelve datos estructurados para revisión
 */
export const procesarTextoIA = async (
  texto: string,
  usuarioId: string
): Promise<RespuestaIA> => {
  try {
    const response = await http.post<{ data: RespuestaIA }>('/api/ia/procesar-texto', {
      texto,
      usuarioId,
    });
    return response.data.data;
  } catch (error) {
    throw parseError(error);
  }
};

/**
 * Confirma y guarda la transacción procesada por IA
 */
export const confirmarTransaccion = async (
  request: ConfirmarTransaccionRequest
): Promise<{ categoria: any; transaccion: any }> => {
  try {
    const response = await http.post<{ data: { categoria: any; transaccion: any } }>(
      '/api/ia/confirmar-transaccion',
      request
    );
    return response.data.data;
  } catch (error) {
    throw parseError(error);
  }
};

/**
 * Verifica el estado del servicio de IA
 */
export const verificarEstadoIA = async (): Promise<{
  iaConfigurada: boolean;
  proveedor: string;
  modelo: string;
}> => {
  try {
    const response = await http.get<{
      data: { iaConfigurada: boolean; proveedor: string; modelo: string };
    }>('/api/ia/health');
    return response.data.data;
  } catch (error) {
    throw parseError(error);
  }
};
