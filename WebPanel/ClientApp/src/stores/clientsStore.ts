import { create } from 'zustand';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from './authStore';

interface ClientInfo {
  clientId: string;
  hwid: string;
  username: string;
  os: string;
  ipAddress: string;
  country: string;
  ping: number;
  isConnected: boolean;
  connectedAt: string;
  activeWindow?: string;
  antiVirus?: string;
  version?: string;
}

interface ClientsState {
  clients: ClientInfo[];
  connection: signalR.HubConnection | null;
  isConnected: boolean;
  connect: () => Promise<void>;
  disconnect: () => Promise<void>;
  addClient: (client: ClientInfo) => void;
  removeClient: (clientId: string) => void;
  updateClient: (client: ClientInfo) => void;
}

export const useClientsStore = create<ClientsState>((set, get) => ({
  clients: [],
  connection: null,
  isConnected: false,

  connect: async () => {
    const { token } = useAuthStore.getState();
    
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/clients', {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('ClientConnected', (client: ClientInfo) => {
      get().addClient(client);
    });

    connection.on('ClientDisconnected', (data: { clientId: string }) => {
      get().removeClient(data.clientId);
    });

    connection.on('ClientUpdated', (client: ClientInfo) => {
      get().updateClient(client);
    });

    connection.on('CommandResponse', (data: any) => {
      console.log('Command response:', data);
    });

    connection.on('FileTransferProgress', (data: any) => {
      console.log('File transfer progress:', data);
    });

    try {
      await connection.start();
      set({ connection, isConnected: true });
      console.log('SignalR connected');
    } catch (error) {
      console.error('SignalR connection failed:', error);
    }
  },

  disconnect: async () => {
    const { connection } = get();
    if (connection) {
      await connection.stop();
      set({ connection: null, isConnected: false });
    }
  },

  addClient: (client: ClientInfo) => {
    set((state) => ({
      clients: [...state.clients.filter((c) => c.clientId !== client.clientId), client],
    }));
  },

  removeClient: (clientId: string) => {
    set((state) => ({
      clients: state.clients.map((c) =>
        c.clientId === clientId ? { ...c, isConnected: false } : c
      ),
    }));
  },

  updateClient: (client: ClientInfo) => {
    set((state) => ({
      clients: state.clients.map((c) => (c.clientId === client.clientId ? client : c)),
    }));
  },
}));
