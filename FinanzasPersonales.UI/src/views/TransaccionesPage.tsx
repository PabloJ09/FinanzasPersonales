import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { transaccionesApi, type Transaccion, type TransaccionCreateDto } from '../webapi/transaccionesApi.ts';
import { categoriasApi, type Categoria } from '../webapi/categoriasApi.ts';
import { procesarTextoIA, confirmarTransaccion, type RespuestaIA } from '../webapi/iaApi.ts';
import { Card, CardHeader, CardBody, Button, Input, Select, Badge, IconButton, Table, TableHeader, TableBody, TableRow, TableCell, TableHeadCell } from '../ui/components/index.ts';

export default function TransaccionesPage() {
  const queryClient = useQueryClient();

  const toDateInput = (iso?: string | null) => {
    if (!iso) return '';
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return '';
    return d.toISOString().slice(0, 10);
  };
  
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

  // Estados para IA
  const [tabActiva, setTabActiva] = useState<'formulario' | 'ia'>('formulario');
  const [textoLibre, setTextoLibre] = useState('');
  const [respuestaIA, setRespuestaIA] = useState<RespuestaIA | null>(null);
  const [pasoIA, setPasoIA] = useState<'entrada' | 'revision' | 'confirmacion'>('entrada');
  const [loadingIA, setLoadingIA] = useState(false);
  const [errorIA, setErrorIA] = useState<string | null>(null);

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

  // Funciones para el flujo de IA
  const handleProcesarTextoIA = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorIA(null);

    if (!textoLibre.trim()) {
      setErrorIA('Por favor, escribe algo');
      return;
    }

    try {
      setLoadingIA(true);
      const userId = localStorage.getItem('userId') || 'user123';
      const respuesta = await procesarTextoIA(textoLibre, userId);
      setRespuestaIA(respuesta);
      setPasoIA('revision');
    } catch (err) {
      setErrorIA(err instanceof Error ? err.message : 'Error al procesar el texto');
    } finally {
      setLoadingIA(false);
    }
  };

  const handleEditarCategoriaIA = (campo: string, valor: any) => {
    if (respuestaIA) {
      setRespuestaIA({
        ...respuestaIA,
        categoria: { ...respuestaIA.categoria, [campo]: valor }
      });
    }
  };

  const handleEditarTransaccionIA = (campo: string, valor: any) => {
    if (respuestaIA) {
      setRespuestaIA({
        ...respuestaIA,
        transaccion: { ...respuestaIA.transaccion, [campo]: valor }
      });
    }
  };

  const handleConfirmarIA = async () => {
    if (!respuestaIA) return;

    try {
      setLoadingIA(true);
      const userId = localStorage.getItem('userId') || 'user123';
      await confirmarTransaccion({
        categoria: respuestaIA.categoria,
        transaccion: respuestaIA.transaccion,
        usuarioId: userId
      });

      // Limpiar y recargar
      setTextoLibre('');
      setRespuestaIA(null);
      setPasoIA('confirmacion');
      queryClient.invalidateQueries({ queryKey: ['transacciones'] });
      queryClient.invalidateQueries({ queryKey: ['categorias'] });
    } catch (err) {
      setErrorIA(err instanceof Error ? err.message : 'Error al guardar la transacción');
    } finally {
      setLoadingIA(false);
    }
  };

  const handleVolverIA = () => {
    setTextoLibre('');
    setRespuestaIA(null);
    setPasoIA('entrada');
    setErrorIA(null);
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
    const confirm = window.confirm('¿Eliminar esta transacción? Esta acción no se puede deshacer.');
    if (confirm) {
      deleteMutation.mutate(id);
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
      <p className="text-sm text-slate-400">
        Tip: usa el lápiz para editar (los datos se cargan en el formulario) y el basurero para eliminar (te pediremos confirmación).
      </p>

      {/* Create/Edit Form with Tabs */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <h2 className="heading-section">
              {editingId ? 'Editar Transacción' : 'Nueva Transacción'}
            </h2>
            <div className="flex gap-2">
              <button
                onClick={() => setTabActiva('formulario')}
                className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                  tabActiva === 'formulario'
                    ? 'bg-blue-600 text-white'
                    : 'bg-slate-700 text-slate-300 hover:bg-slate-600'
                }`}
              >
                Formulario
              </button>
              <button
                onClick={() => setTabActiva('ia')}
                className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                  tabActiva === 'ia'
                    ? 'bg-blue-600 text-white'
                    : 'bg-slate-700 text-slate-300 hover:bg-slate-600'
                }`}
              >
                Con IA
              </button>
            </div>
          </div>
          {editingId && (
            <p className="text-xs text-amber-300 mt-2">Estás editando una transacción existente. Los campos del formulario ya están cargados.</p>
          )}
        </CardHeader>
        <CardBody>
          {/* Tab: Formulario Tradicional */}
          {tabActiva === 'formulario' && (
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
          )}

          {/* Tab: Con IA */}
          {tabActiva === 'ia' && (
            <div className="space-y-4">
              {pasoIA === 'entrada' && (
                <form onSubmit={handleProcesarTextoIA} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-slate-200 mb-2">
                      Describe tu transacción en texto libre
                    </label>
                    <textarea
                      value={textoLibre}
                      onChange={(e) => setTextoLibre(e.target.value)}
                      placeholder="Ej: Compré pan y leche por 5000 colones; Recibí pago por consultoría..."
                      className="w-full px-4 py-3 bg-slate-700 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 resize-none"
                      rows={4}
                      disabled={loadingIA}
                    />
                  </div>
                  {errorIA && (
                    <div className="p-3 bg-red-500/10 border border-red-500/50 rounded-lg text-red-400 text-sm">
                      {errorIA}
                    </div>
                  )}
                  <Button
                    type="submit"
                    variant="primary"
                    disabled={loadingIA || !textoLibre.trim()}
                  >
                    {loadingIA ? 'Procesando con IA...' : 'Procesar con IA'}
                  </Button>
                </form>
              )}

              {pasoIA === 'revision' && respuestaIA && (
                <div className="space-y-4">
                  {respuestaIA.advertencias && respuestaIA.advertencias.length > 0 && (
                    <div className="p-4 bg-yellow-500/10 border border-yellow-500/50 rounded-lg">
                      <h4 className="font-medium text-yellow-400 mb-2">Advertencias</h4>
                      <ul className="space-y-1 text-sm text-yellow-300">
                        {respuestaIA.advertencias.map((adv, i) => (
                          <li key={i}>• {adv}</li>
                        ))}
                      </ul>
                    </div>
                  )}

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-stretch">
                    <div className="h-full">
                      <h4 className="font-medium text-slate-200 mb-2">Categoría</h4>
                      <div className="space-y-2 p-3 bg-slate-700 rounded-lg h-full">
                        <Input
                          label="Nombre"
                          value={respuestaIA.categoria.nombre ?? ''}
                          onChange={(e) => handleEditarCategoriaIA('nombre', e.target.value)}
                        />
                        <Select
                          label="Tipo"
                          value={respuestaIA.categoria.tipo || 'Gasto'}
                          onChange={(e) => handleEditarCategoriaIA('tipo', e.target.value)}
                        >
                          <option value="Gasto">Gasto</option>
                          <option value="Ingreso">Ingreso</option>
                        </Select>
                      </div>
                    </div>

                    <div className="h-full">
                      <h4 className="font-medium text-slate-200 mb-2">Transacción</h4>
                      <div className="p-3 bg-slate-700 rounded-lg h-full">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                          <Input
                            label="Monto (CRC)"
                            type="number"
                            step="0.01"
                            min="0.01"
                            value={respuestaIA.transaccion.monto || ''}
                            onChange={(e) => handleEditarTransaccionIA('monto', Number(e.target.value))}
                          />
                          <Input
                            label="Fecha"
                            type="date"
                            value={toDateInput(respuestaIA.transaccion.fecha)}
                            onChange={(e) => handleEditarTransaccionIA(
                              'fecha',
                              e.target.value ? new Date(`${e.target.value}T00:00:00`).toISOString() : null
                            )}
                          />
                          <div className="md:col-span-2">
                            <Input
                              label="Descripción"
                              value={respuestaIA.transaccion.descripcion ?? ''}
                              onChange={(e) => handleEditarTransaccionIA('descripcion', e.target.value)}
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="flex gap-2">
                    <Button
                      type="button"
                      onClick={handleConfirmarIA}
                      variant="primary"
                      disabled={loadingIA}
                    >
                      {loadingIA ? 'Guardando...' : 'Confirmar'}
                    </Button>
                    <Button
                      type="button"
                      onClick={handleVolverIA}
                      variant="secondary"
                    >
                      Cancelar
                    </Button>
                  </div>
                </div>
              )}

              {pasoIA === 'confirmacion' && (
                <div className="space-y-4">
                  <div className="p-4 bg-green-500/10 border border-green-500/50 rounded-lg">
                    <h4 className="font-medium text-green-400 mb-2">Transacción Guardada</h4>
                    <p className="text-sm text-green-300">
                      Tu transacción se ha guardado correctamente en la base de datos.
                    </p>
                  </div>
                  <Button
                    type="button"
                    onClick={handleVolverIA}
                    variant="primary"
                  >
                    Crear Otra Transacción
                  </Button>
                </div>
              )}
            </div>
          )}
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
                          title="Eliminar"
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
