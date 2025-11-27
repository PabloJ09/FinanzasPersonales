import { Route, Routes, Navigate } from 'react-router-dom';
import LoginPage from '../views/LoginPage.tsx';
import RegisterPage from '../views/RegisterPage.tsx';
import CategoriasPage from '../views/CategoriasPage.tsx';
import TransaccionesPage from '../views/TransaccionesPage.tsx';
import ProtectedRoute from './ProtectedRoute.tsx';
import { Layout } from './components/Layout.tsx';

export default function App() {
  const token = localStorage.getItem('token');

  return (
    <Routes>
      <Route path="/" element={<Navigate to={token ? '/transacciones' : '/login'} replace />} />
      <Route path="/login" element={token ? <Navigate to="/transacciones" replace /> : <LoginPage />} />
      <Route path="/register" element={token ? <Navigate to="/transacciones" replace /> : <RegisterPage />} />
      <Route
        path="/categorias"
        element={
          <ProtectedRoute>
            <Layout>
              <CategoriasPage />
            </Layout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/transacciones"
        element={
          <ProtectedRoute>
            <Layout>
              <TransaccionesPage />
            </Layout>
          </ProtectedRoute>
        }
      />
    </Routes>
  );
}
