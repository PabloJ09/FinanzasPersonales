import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { categoriasApi, type Categoria } from '../webapi/categoriasApi.ts';
import { Card, CardHeader, CardBody, Button, Input, Select, Badge, IconButton } from '../ui/components/index.ts';

export default function CategoriasPage() {
  const queryClient = useQueryClient();
  const [nombre, setNombre] = useState('');
  const [tipo, setTipo] = useState<'Ingreso' | 'Gasto'>('Ingreso');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState('');

  const { data: categorias, isLoading } = useQuery<Categoria[]>({
    queryKey: ['categorias'],
    queryFn: categoriasApi.list,
  });

  const createMutation = useMutation({
    mutationFn: (payload: { nombre: string; tipo: string }) => categoriasApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categorias'] });
      setNombre('');
      setTipo('Ingreso');
      setErrorMsg('');
    },
    onError: (error: any) => {
      if (error.response?.status === 400) {
        setErrorMsg('Datos inválidos. Verifica el nombre y tipo de la categoría.');
      } else if (error.response?.status === 401) {
        setErrorMsg('Sesión expirada. Por favor inicia sesión nuevamente.');
      } else {
        setErrorMsg('Error al crear la categoría. Intenta de nuevo.');
      }
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: { nombre: string; tipo: string } }) =>
      categoriasApi.update(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categorias'] });
      setEditingId(null);
      setNombre('');
      setTipo('Ingreso');
      setErrorMsg('');
    },
    onError: (error: any) => {
      if (error.response?.status === 400) {
        setErrorMsg('Datos inválidos. Verifica el nombre y tipo de la categoría.');
      } else if (error.response?.status === 401) {
        setErrorMsg('Sesión expirada. Por favor inicia sesión nuevamente.');
      } else {
        setErrorMsg('Error al actualizar la categoría. Intenta de nuevo.');
      }
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => categoriasApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categorias'] });
      setDeleteConfirm(null);
      setErrorMsg('');
    },
    onError: (error: any) => {
      if (error.response?.status === 400) {
        setErrorMsg('No se puede eliminar esta categoría.');
      } else if (error.response?.status === 401) {
        setErrorMsg('Sesión expirada. Por favor inicia sesión nuevamente.');
      } else {
        setErrorMsg('Error al eliminar la categoría. Intenta de nuevo.');
      }
      setDeleteConfirm(null);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!nombre.trim()) {
      setErrorMsg('El nombre de la categoría es requerido.');
      return;
    }

    if (editingId) {
      updateMutation.mutate({ id: editingId, payload: { nombre, tipo } });
    } else {
      createMutation.mutate({ nombre, tipo });
    }
  };

  const handleEdit = (categoria: Categoria) => {
    setEditingId(categoria.id);
    setNombre(categoria.nombre);
    setTipo(categoria.tipo);
    setErrorMsg('');
  };

  const handleCancelEdit = () => {
    setEditingId(null);
    setNombre('');
    setTipo('Ingreso');
    setErrorMsg('');
  };

  const handleDelete = (id: string) => {
    if (deleteConfirm === id) {
      deleteMutation.mutate(id);
    } else {
      setDeleteConfirm(id);
      setTimeout(() => setDeleteConfirm(null), 3000);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-slate-400">Cargando categorías...</div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <h1 className="heading-page">Categorías</h1>
      </div>

      {/* Error Message */}
      {errorMsg && (
        <div className="alert-error">
          {errorMsg}
        </div>
      )}

      {/* Create/Edit Form */}
      <Card>
        <CardHeader>
          <h2 className="heading-section">
            {editingId ? 'Editar Categoría' : 'Nueva Categoría'}
          </h2>
        </CardHeader>
        <CardBody>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input
              label="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              placeholder="Ej. Salario, Comida, Transporte"
            />
            
            <Select
              label="Tipo"
              value={tipo}
              onChange={(e) => setTipo(e.target.value as 'Ingreso' | 'Gasto')}
            >
              <option value="Ingreso">Ingreso</option>
              <option value="Gasto">Gasto</option>
            </Select>
            
            <div className="flex gap-3 pt-2">
              <Button type="submit" variant="primary">
                {editingId ? 'Actualizar' : 'Crear Categoría'}
              </Button>
              {editingId && (
                <Button type="button" onClick={handleCancelEdit} variant="secondary">
                  Cancelar
                </Button>
              )}
            </div>
          </form>
        </CardBody>
      </Card>

      {/* Categories Grid */}
      <div>
        <h2 className="heading-section mb-4">Mis Categorías</h2>
        {!categorias?.length ? (
          <Card>
            <CardBody>
              <p className="text-center py-12 text-slate-400">
                No hay categorías todavía. Crea tu primera categoría arriba.
              </p>
            </CardBody>
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {categorias?.map((cat) => (
              <Card key={cat.id}>
                <CardBody>
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex items-center gap-3">
                      <div className={`w-12 h-12 rounded-lg flex items-center justify-center ${
                        cat.tipo === 'Ingreso' 
                          ? 'bg-green-500/20 text-green-400' 
                          : 'bg-red-500/20 text-red-400'
                      }`}>
                        {cat.tipo === 'Ingreso' ? (
                          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 11l5-5m0 0l5 5m-5-5v12" />
                          </svg>
                        ) : (
                          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 13l-5 5m0 0l-5-5m5 5V6" />
                          </svg>
                        )}
                      </div>
                      <div>
                        <h3 className="font-semibold text-slate-100 text-lg">{cat.nombre}</h3>
                        <Badge variant={cat.tipo === 'Ingreso' ? 'income' : 'expense'}>
                          {cat.tipo}
                        </Badge>
                      </div>
                    </div>
                  </div>
                  
                  <div className="flex gap-2 mt-4 pt-4 border-t border-slate-700">
                    <IconButton 
                      onClick={() => handleEdit(cat)}
                      title="Editar"
                    >
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                    </IconButton>
                    <IconButton
                      onClick={() => handleDelete(cat.id)}
                      className={deleteConfirm === cat.id ? 'text-red-400 bg-red-500/20' : ''}
                      title={deleteConfirm === cat.id ? 'Confirmar eliminación' : 'Eliminar'}
                    >
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </IconButton>
                  </div>
                </CardBody>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
