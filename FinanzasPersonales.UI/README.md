# FinanzasPersonales.UI

Interfaz React + Vite + TypeScript para la API de Finanzas Personales.

## 🎯 Características

- ✅ Autenticación JWT con registro e inicio de sesión
- ✅ Gestión completa de categorías (Ingreso/Gasto)
- ✅ Gestión completa de transacciones con todos los campos
- ✅ Edición inline de registros
- ✅ Confirmación de eliminación
- ✅ Formato de moneda y fechas
- ✅ Validación de formularios
- ✅ Estados de carga y mensajes de error
- ✅ Rutas protegidas
- ✅ Diseño responsive con Tailwind CSS

## 📋 Requisitos

- Node.js 18+ 
- npm 8+
- .NET 9 SDK (para la API)
- MongoDB en ejecución

## 🚀 Configuración

1. **Copiar variables de entorno**:
```powershell
Copy-Item ".env.example" ".env"
```

2. **Ajustar la URL de la API** (si es necesario):
Edita `.env` y cambia `VITE_API_BASE` al puerto donde corre tu API.

3. **Instalar dependencias**:
```powershell
npm install
```

## 💻 Desarrollo

### Opción 1: Levantar servicios por separado

**Terminal 1 - API**:
```powershell
Set-Location "d:\Pablo\Proyecto Finanzas\FinanzasPersonales"
dotnet run --project ".\FinanzasPersonales.csproj"
```

**Terminal 2 - UI**:
```powershell
Set-Location "d:\Pablo\Proyecto Finanzas\FinanzasPersonales.UI"
npm run dev
```

### Opción 2: Usar el script automático

Desde la raíz del proyecto:
```powershell
Set-Location "d:\Pablo\Proyecto Finanzas"
.\run-dev.ps1
```

La UI estará disponible en `http://localhost:5173`

## 🏗️ Build de producción

```powershell
npm run build
```

El resultado queda en `dist/`. Puedes servirlo con cualquier servidor estático.

## 📡 Endpoints de la API

La UI consume los siguientes endpoints:

### Auth
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/login` - Inicio de sesión (retorna JWT)

### Categorías
- `GET /api/categorias` - Listar categorías del usuario
- `POST /api/categorias` - Crear categoría
- `PUT /api/categorias/{id}` - Actualizar categoría
- `DELETE /api/categorias/{id}` - Eliminar categoría

### Transacciones
- `GET /api/transacciones` - Listar transacciones del usuario
- `POST /api/transacciones` - Crear transacción
- `PUT /api/transacciones/{id}` - Actualizar transacción
- `DELETE /api/transacciones/{id}` - Eliminar transacción

## 🎨 Estructura del Proyecto

```
src/
├── main.tsx              # Punto de entrada
├── ui/
│   ├── App.tsx           # Rutas y layout principal
│   ├── ProtectedRoute.tsx # HOC para rutas protegidas
│   └── index.css         # Estilos globales Tailwind
├── views/
│   ├── LoginPage.tsx     # Página de login
│   ├── RegisterPage.tsx  # Página de registro
│   ├── CategoriasPage.tsx    # CRUD de categorías
│   └── TransaccionesPage.tsx # CRUD de transacciones
└── webapi/
    ├── http.ts           # Cliente Axios con interceptor JWT
    ├── authApi.ts        # Servicios de autenticación
    ├── categoriasApi.ts  # Servicios de categorías
    └── transaccionesApi.ts # Servicios de transacciones
```

## 🔐 Flujo de Autenticación

1. El usuario se registra o inicia sesión
2. El token JWT se guarda en `localStorage`
3. Todas las peticiones subsiguientes incluyen el token en el header `Authorization: Bearer {token}`
4. Si el token no existe o es inválido, el usuario es redirigido a `/login`
5. El botón "Cerrar Sesión" elimina el token y redirige a login

## 🛠️ Tecnologías

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool
- **React Router** - Enrutamiento
- **TanStack Query** - Data fetching y cache
- **Axios** - HTTP client
- **Tailwind CSS** - Estilos

## ❓ Troubleshooting

### Error de CORS
Asegúrate de que la API tenga CORS habilitado para `http://localhost:5173` en `Program.cs`.

### Token inválido
Si ves errores 401, verifica que:
- El token esté guardado en `localStorage`
- El backend esté configurado para aceptar el token JWT
- La clave secreta JWT coincida en ambos lados

### Campos faltantes
Si un campo no aparece, verifica que el DTO en la API coincida con el tipo TypeScript en `src/webapi/*.ts`.

