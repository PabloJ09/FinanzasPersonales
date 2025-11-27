import { ReactNode } from 'react';
import { NavLink } from 'react-router-dom';

interface LayoutProps {
  children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
  const handleLogout = () => {
    localStorage.removeItem('token');
    window.location.href = '/login';
  };

  return (
    <div className="min-h-screen flex flex-col">
      <nav className="bg-slate-800 border-b border-slate-700 sticky top-0 z-50 shadow-xl">
        <div className="container-dashboard">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg flex items-center justify-center shadow-lg">
                <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <span className="text-xl font-bold text-slate-50">Finanzas Personales</span>
            </div>

            {/* Navigation Links */}
            <div className="flex items-center gap-1">
              <NavLink
                to="/categorias"
                className={({ isActive }) =>
                  `px-4 py-2 rounded-lg font-medium transition-colors ${
                    isActive
                      ? 'bg-blue-600 text-white'
                      : 'text-slate-300 hover:bg-slate-700 hover:text-white'
                  }`
                }
              >
                Categorías
              </NavLink>
              <NavLink
                to="/transacciones"
                className={({ isActive }) =>
                  `px-4 py-2 rounded-lg font-medium transition-colors ${
                    isActive
                      ? 'bg-blue-600 text-white'
                      : 'text-slate-300 hover:bg-slate-700 hover:text-white'
                  }`
                }
              >
                Transacciones
              </NavLink>
              <button
                onClick={handleLogout}
                className="ml-2 px-4 py-2 rounded-lg font-medium text-slate-300 hover:bg-red-600/10 hover:text-red-400 transition-colors"
              >
                Cerrar Sesión
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="flex-1 py-8">
        <div className="container-dashboard">
          {children}
        </div>
      </main>
    </div>
  );
}
