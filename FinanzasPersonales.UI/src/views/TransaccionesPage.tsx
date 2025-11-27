import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { transaccionesApi, type Transaccion, type TransaccionCreateDto } from '../webapi/transaccionesApi.ts';
import { categoriasApi, type Categoria } from '../webapi/categoriasApi.ts';
import { Card, CardHeader, CardBody, Button, Input, Select, Badge, IconButton, Table, TableHeader, TableBody, TableRow, TableCell, TableHeadCell } from '../ui/components/index.ts';

export default function TransaccionesPage() {
  const queryClient = useQueryClient();
  
  const { data: transacciones, isLoading } = useQuery<Transaccion[]>({
    queryKey: ['transacciones'],
    queryFn: transaccionesApi.list
  });
  
  const { data: categorias } = useQuery<Categoria[]>({
    queryKey: ['categorias'],
    queryFn: categoriasApi.list
  });

  const [form, setForm] = useState<TransaccionCreateDto>({
    tipo: 'Gasto',
    monto: 0,
    descripcion: '',
    categoriaId: '',
    fecha: new Date().toISOString().split('T')[0]
  });
  
  const [editingId, setEditingId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);

  const createMutation = useMutation({
    mutationFn: (payload: TransaccionCreateDto) => transaccionesApi.create(payload),
    onSuccess: () => {
      setForm({ tipo: 'Gasto', monto: 0, descripcion: '', categoriaId: '', fecha: new Date().toISOString().split('T')[0] });
      queryClient.invalidateQueries({ queryKey: ['transacciones'] });
    }
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: TransaccionCreateDto }) =>
      transaccionesApi.update(id, payload),
    onSuccess: () => {
      setEditingId(null);
      setForm({ tipo: 'Gasto', monto: 0, descripcion: '', categoriaId: '', fecha: new Date().toISOString().split('T')[0] });
      queryClient.invalidateQueries({ queryKey: ['transacciones'] });
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => transaccionesApi.remove(id),
    onSuccess: () => {
      setDeleteConfirm(null);
      queryClient.invalidateQueries({ queryKey: ['transacciones'] });
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = { ...form, fecha: new Date(form.fecha).toISOString() };
    if (editingId) {
      updateMutation.mutate({ id: editingId, payload });
    } else {
      createMutation.mutate(payload);
    }
  };

  const handleEdit = (t: Transaccion) => {
    setEditingId(t.id);
    setForm({
      tipo: t.tipo,
      monto: t.monto,
      descripcion: t.descripcion,
      categoriaId: t.categoriaId,
      fecha: new Date(t.fecha).toISOString().split('T')[0]
    });
  };

  const handleCancelEdit = () => {
    setEditingId(null);
    setForm({ tipo: 'Gasto', monto: 0, descripcion: '', categoriaId: '', fecha: new Date().toISOString().split('T')[0] });
  };

  const handleDelete = (id: string) => {
    if (deleteConfirm === id) {
      deleteMutation.mutate(id);
    } else {
      setDeleteConfirm(id);
      setTimeout(() => setDeleteConfirm(null), 3000);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', maximumFractionDigits: 2 }).format(amount);
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('es-MX', { year: 'numeric', month: '2-digit', day: '2-digit' });
  };

  const getCategoriaName = (catId: string) => {
    return categorias?.find((c: Categoria) => c.id === catId)?.nombre || 'Sin categoría';
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-slate-400">Cargando transacciones...</div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <h1 className="heading-page">Transacciones</h1>
      </div>

      {/* Create/Edit Form */}
      <Card>
        <CardHeader>
          <h2 className="heading-section">
            {editingId ? 'Editar Transacción' : 'Nueva Transacción'}
          </h2>
        </CardHeader>
        <CardBody>
          <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
            <Select
              label="Tipo"
              value={form.tipo}
              onChange={(e) => setForm({ ...form, tipo: e.target.value })}
              required
            >
              <option value="Gasto">Gasto</option>
              <option value="Ingreso">Ingreso</option>
            </Select>
            
            <Select
              label="Categoría"
              value={form.categoriaId}
              onChange={(e) => setForm({ ...form, categoriaId: e.target.value })}
              required
            >
              <option value="">Seleccionar...</option>
              {(categorias ?? []).map((c: Categoria) => (
                <option key={c.id} value={c.id}>{c.nombre}</option>
              ))}
            </Select>
            
            <Input
              label="Monto (CRC)"
              type="number"
              step="0.01"
              min="0.01"
              placeholder="0.00"
              value={form.monto || ''}
              onChange={(e) => setForm({ ...form, monto: Number(e.target.value) })}
              required
            />
            
            <Input
              label="Fecha"
              type="date"
              value={form.fecha}
              onChange={(e) => setForm({ ...form, fecha: e.target.value })}
              required
            />
            
            <div className="form-group">
              <label className="form-label">Descripción</label>
              <input
                className="input-modern"
                type="text"
                placeholder="Descripción de la transacción"
                value={form.descripcion}
                onChange={(e) => setForm({ ...form, descripcion: e.target.value })}
                required
              />
            </div>

            <div className="flex items-end gap-2 md:col-span-2 lg:col-span-5">
              <Button type="submit" variant="primary">
                {editingId ? 'Actualizar' : 'Crear Transacción'}
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

      {/* Transactions Table */}
      <div>
        <h2 className="heading-section mb-4">Mis Transacciones</h2>
        {!transacciones?.length ? (
          <Card>
            <CardBody>
              <p className="text-center py-12 text-slate-400">
                No hay transacciones todavía. Crea tu primera transacción arriba.
              </p>
            </CardBody>
          </Card>
        ) : (
          <Card>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHeadCell>Tipo</TableHeadCell>
                  <TableHeadCell>Categoría</TableHeadCell>
                  <TableHeadCell>Descripción</TableHeadCell>
                  <TableHeadCell className="text-right">Monto</TableHeadCell>
                  <TableHeadCell>Fecha</TableHeadCell>
                  <TableHeadCell className="text-center">Acciones</TableHeadCell>
                </TableRow>
              </TableHeader>
              <TableBody>
                {transacciones?.map((t) => (
                  <TableRow key={t.id}>
                    <TableCell>
                      <Badge variant={t.tipo === 'Ingreso' ? 'income' : 'expense'}>
                        {t.tipo}
                      </Badge>
                    </TableCell>
                    <TableCell>{getCategoriaName(t.categoriaId)}</TableCell>
                    <TableCell className="text-slate-300">{t.descripcion}</TableCell>
                    <TableCell className={`text-right font-semibold ${
                      t.tipo === 'Ingreso' ? 'text-green-400' : 'text-red-400'
                    }`}>
                      {formatCurrency(t.monto)}
                    </TableCell>
                    <TableCell className="text-slate-400">{formatDate(t.fecha)}</TableCell>
                    <TableCell>
                      <div className="flex items-center justify-center gap-2">
                        <IconButton onClick={() => handleEdit(t)} title="Editar">
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                        </IconButton>
                        <IconButton
                          onClick={() => handleDelete(t.id)}
                          className={deleteConfirm === t.id ? 'text-red-400 bg-red-500/20' : ''}
                          title={deleteConfirm === t.id ? 'Confirmar eliminación' : 'Eliminar'}
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                        </IconButton>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Card>
        )}
      </div>
    </div>
  );
}
