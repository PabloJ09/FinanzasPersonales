import http from './http';

export type Transaccion = {
  id: string;
  tipo: 'Ingreso' | 'Gasto';
  monto: number;
  descripcion: string;
  categoriaId: string;
  fecha: string;
  usuarioId: string;
};

export type TransaccionCreateDto = {
  tipo: string;
  monto: number;
  descripcion: string;
  categoriaId: string;
  fecha: string;
};

export const transaccionesApi = {
  async list(): Promise<Transaccion[]> {
    const { data } = await http.get('/api/transacciones');
    return data;
  },
  async create(payload: TransaccionCreateDto): Promise<Transaccion> {
    const { data } = await http.post('/api/transacciones', payload);
    return data;
  },
  async update(id: string, payload: TransaccionCreateDto): Promise<void> {
    await http.put(`/api/transacciones/${id}`, payload);
  },
  async remove(id: string): Promise<void> {
    await http.delete(`/api/transacciones/${id}`);
  }
};
