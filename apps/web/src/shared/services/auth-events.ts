type AuthEventListener = () => void;

class AuthEventEmitter {
  private listeners: AuthEventListener[] = [];

  subscribe(listener: AuthEventListener): () => void {
    this.listeners.push(listener);
    // Retorna função de unsubscribe
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  emit(): void {
    for (const listener of this.listeners) {
      listener();
    }
  }
}

export const authEvents = new AuthEventEmitter();
