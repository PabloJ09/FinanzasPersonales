import http from './http';

export type Categoria = {
  id: string;
  nombre: string;
  tipo: 'Ingreso' | 'Gasto';
  usuarioId: string;
};

export const categoriasApi = {
  async list(): Promise<Categoria[]> {
    const { data } = await http.get('/api/categorias');
    return data;
  },
  async create(payload: { nombre: string; tipo: string }): Promise<Categoria> {
    const { data } = await http.post('/api/categorias', payload);
    return data;
  },
  async update(id: string, payload: { nombre: string; tipo: string }): Promise<void> {
    await http.put(`/api/categorias/${id}`, payload);
  },
  async remove(id: string): Promise<void> {
    await http.delete(`/api/categorias/${id}`);
  }
};
