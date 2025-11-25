import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import axios from 'axios';

interface User {
  id: number;
  username: string;
  role: string;
}

interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
  setToken: (token: string, user: User) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,

      login: async (username: string, password: string) => {
        try {
          const response = await axios.post('/api/auth/login', {
            username,
            password,
          });

          if (response.data.success) {
            const { token, user } = response.data;
            set({ token, user, isAuthenticated: true });
            axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
            return true;
          }
          return false;
        } catch (error) {
          console.error('Login failed:', error);
          return false;
        }
      },

      logout: () => {
        set({ token: null, user: null, isAuthenticated: false });
        delete axios.defaults.headers.common['Authorization'];
      },

      setToken: (token: string, user: User) => {
        set({ token, user, isAuthenticated: true });
        axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
      },
    }),
    {
      name: 'auth-storage',
      onRehydrateStorage: () => (state) => {
        if (state?.token) {
          axios.defaults.headers.common['Authorization'] = `Bearer ${state.token}`;
        }
      },
    }
  )
);
