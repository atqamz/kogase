import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { authApi, User, LoginRequest, RegisterRequest } from '@/lib/api/auth';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Check if we have a token in localStorage
    const token = localStorage.getItem('kogase-token');
    if (!token) {
      setLoading(false);
      return;
    }

    // We have a token, try to get the user profile
    authApi.getProfile()
      .then(user => {
        setUser(user);
        setError(null);
      })
      .catch(err => {
        console.error('Failed to fetch user profile:', err);
        // If we can't get the profile, clear the token
        localStorage.removeItem('kogase-token');
        setError('Session expired. Please login again.');
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  const login = async (data: LoginRequest) => {
    try {
      setLoading(true);
      const response = await authApi.login(data);
      // Store the token in localStorage
      localStorage.setItem('kogase-token', response.token);
      setUser(response.user);
      setError(null);
    } catch (err) {
      console.error('Login failed:', err);
      setError('Invalid email or password');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const register = async (data: RegisterRequest) => {
    try {
      setLoading(true);
      const response = await authApi.register(data);
      // Store the token in localStorage
      localStorage.setItem('kogase-token', response.token);
      setUser(response.user);
      setError(null);
    } catch (err) {
      console.error('Registration failed:', err);
      setError('Registration failed. Please try again.');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const logout = async () => {
    try {
      setLoading(true);
      await authApi.logout();
      // Clear the user state
      setUser(null);
      setError(null);
    } catch (err) {
      console.error('Logout failed:', err);
      setError('Logout failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthContext.Provider value={{ user, loading, error, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
} 