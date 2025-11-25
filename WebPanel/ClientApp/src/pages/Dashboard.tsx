import { useEffect, useState } from 'react';
import { useClientsStore } from '../stores/clientsStore';
import axios from 'axios';
import './Dashboard.css';

interface DashboardStats {
  totalClients: number;
  onlineClients: number;
  offlineClients: number;
  commandsToday: number;
  fileTransfersToday: number;
  clientsByCountry: Record<string, number>;
  clientsByOS: Record<string, number>;
}

function Dashboard() {
  const { clients } = useClientsStore();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchStats();
    const interval = setInterval(fetchStats, 10000); // Refresh every 10s
    return () => clearInterval(interval);
  }, []);

  const fetchStats = async () => {
    try {
      const response = await axios.get('/api/dashboard/stats');
      setStats(response.data);
    } catch (error) {
      console.error('Failed to fetch stats:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="loading">Loading dashboard...</div>;
  }

  const onlineClients = clients.filter((c) => c.isConnected);

  return (
    <div className="dashboard">
      <h1 className="dashboard-title">Dashboard</h1>

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon online">●</div>
          <div className="stat-content">
            <div className="stat-value">{stats?.onlineClients || 0}</div>
            <div className="stat-label">Online Clients</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon offline">●</div>
          <div className="stat-content">
            <div className="stat-value">{stats?.offlineClients || 0}</div>
            <div className="stat-label">Offline Clients</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon total">●</div>
          <div className="stat-content">
            <div className="stat-value">{stats?.totalClients || 0}</div>
            <div className="stat-label">Total Clients</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon commands">●</div>
          <div className="stat-content">
            <div className="stat-value">{stats?.commandsToday || 0}</div>
            <div className="stat-label">Commands Today</div>
          </div>
        </div>
      </div>

      <div className="dashboard-grid">
        <div className="dashboard-card">
          <h2 className="card-title">Recently Connected</h2>
          <div className="client-list">
            {onlineClients.slice(0, 5).map((client) => (
              <div key={client.clientId} className="client-item">
                <div className="client-info">
                  <div className="client-username">{client.username}</div>
                  <div className="client-os">{client.os}</div>
                </div>
                <div className="client-meta">
                  <span className="client-country">{client.country}</span>
                  <span className="client-ping">{client.ping}ms</span>
                </div>
              </div>
            ))}
            {onlineClients.length === 0 && (
              <div className="empty-state">No clients connected</div>
            )}
          </div>
        </div>

        <div className="dashboard-card">
          <h2 className="card-title">Clients by Country</h2>
          <div className="stats-list">
            {Object.entries(stats?.clientsByCountry || {}).map(([country, count]) => (
              <div key={country} className="stats-item">
                <span className="stats-label">{country}</span>
                <span className="stats-value">{count}</span>
              </div>
            ))}
            {Object.keys(stats?.clientsByCountry || {}).length === 0 && (
              <div className="empty-state">No data available</div>
            )}
          </div>
        </div>

        <div className="dashboard-card">
          <h2 className="card-title">Clients by OS</h2>
          <div className="stats-list">
            {Object.entries(stats?.clientsByOS || {}).map(([os, count]) => (
              <div key={os} className="stats-item">
                <span className="stats-label">{os}</span>
                <span className="stats-value">{count}</span>
              </div>
            ))}
            {Object.keys(stats?.clientsByOS || {}).length === 0 && (
              <div className="empty-state">No data available</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
