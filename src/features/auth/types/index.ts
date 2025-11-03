export interface User {
  id: string;
  name: string;
  email: string;
}

export interface Credentials {
  email: string;
  password: string;
}

export interface AuthState {
  user: User | null;
  token: string | null;
}
