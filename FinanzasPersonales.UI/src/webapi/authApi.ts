import http from './http';

export const authApi = {
  async login(payload: { username: string; password: string }): Promise<string> {
    const { data } = await http.post('/api/auth/login', payload);
    return data?.token ?? data;
  },
  async register(payload: { username: string; password: string; role: string }): Promise<void> {
    await http.post('/api/auth/register', payload);
  }
};
