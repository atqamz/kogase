import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080/api/v1';

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to add the auth token to every request
apiClient.interceptors.request.use(
  (config) => {
    // Get the token from localStorage if we're in the browser
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('kogase-token');
      if (token) {
        config.headers['Authorization'] = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add a response interceptor to handle refresh token or logout
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;
    
    // If the error is a 401 (Unauthorized) and we have a refresh token
    // we can try to refresh the token and retry the request
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Redirect to login page
        if (typeof window !== 'undefined') {
          localStorage.removeItem('kogase-token');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      } catch (refreshError) {
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

export default apiClient; 