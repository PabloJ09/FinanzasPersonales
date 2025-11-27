import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../webapi/authApi.ts';
import { Card, CardHeader, CardBody, Button, Input, Select } from '../ui/components/index.ts';

export default function RegisterPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: '', password: '', role: 'usuario' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await authApi.register(form);
      alert('Usuario registrado correctamente. Por favor inicia sesión.');
      navigate('/login');
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Error al registrar usuario');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-900 via-blue-900 to-slate-900 p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-blue-500 to-blue-600 rounded-2xl shadow-2xl mb-4">
            <svg className="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-white mb-2">Finanzas Personales</h1>
          <p className="text-slate-300">Crea tu cuenta y comienza a gestionar tus finanzas</p>
        </div>

        <Card>
          <CardHeader>
            <h2 className="heading-section">Registro de Usuario</h2>
          </CardHeader>
          <CardBody>
            <form onSubmit={onSubmit} className="space-y-4">
              <Input
                label="Usuario"
                value={form.username}
                onChange={(e) => setForm({ ...form, username: e.target.value })}
                placeholder="Mínimo 4 caracteres"
                required
                minLength={4}
              />

              <Input
                label="Contraseña"
                type="password"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                placeholder="Mínimo 8 caracteres"
                required
                minLength={8}
              />

              <Select
                label="Rol"
                value={form.role}
                onChange={(e) => setForm({ ...form, role: e.target.value })}
              >
                <option value="usuario">Usuario</option>
                <option value="admin">Administrador</option>
              </Select>

              {error && (
                <div className="alert-error">
                  {error}
                </div>
              )}

              <Button
                type="submit"
                variant="primary"
                className="w-full"
                disabled={loading}
              >
                {loading ? 'Registrando...' : 'Crear Cuenta'}
              </Button>
            </form>

            <p className="mt-6 text-center text-sm text-slate-400">
              ¿Ya tienes cuenta?{' '}
              <Link to="/login" className="text-blue-400 hover:text-blue-300 font-medium">
                Inicia sesión aquí
              </Link>
            </p>
          </CardBody>
        </Card>
      </div>
    </div>
  );
}
