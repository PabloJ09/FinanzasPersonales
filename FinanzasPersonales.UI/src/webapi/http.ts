import axios from 'axios';

const http = axios.create({
  baseURL: (globalThis as any).__API_BASE__ ?? 'http://localhost:5000',
});

http.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export default http;
