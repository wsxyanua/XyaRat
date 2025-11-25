import { Link } from 'react-router-dom';
import { useClientsStore } from '../stores/clientsStore';
import './Clients.css';

function Clients() {
  const { clients } = useClientsStore();

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString();
  };

  return (
    <div className="clients-page">
      <div className="clients-header">
        <h1 className="clients-title">Connected Clients</h1>
        <div className="clients-count">
          {clients.filter((c) => c.isConnected).length} online / {clients.length} total
        </div>
      </div>

      <div className="clients-table-container">
        <table className="clients-table">
          <thead>
            <tr>
              <th>Status</th>
              <th>Username</th>
              <th>HWID</th>
              <th>OS</th>
              <th>IP Address</th>
              <th>Country</th>
              <th>Ping</th>
              <th>Connected At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {clients.map((client) => (
              <tr key={client.clientId} className={client.isConnected ? 'online' : 'offline'}>
                <td>
                  <span className={`status-indicator ${client.isConnected ? 'online' : 'offline'}`}>
                    ●
                  </span>
                </td>
                <td className="client-username">{client.username}</td>
                <td className="client-hwid">{client.hwid}</td>
                <td>{client.os}</td>
                <td>{client.ipAddress}</td>
                <td>{client.country}</td>
                <td className="client-ping">{client.ping}ms</td>
                <td>{formatDate(client.connectedAt)}</td>
                <td>
                  <Link to={`/clients/${client.clientId}`} className="action-button view">
                    View
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {clients.length === 0 && (
          <div className="empty-state">
            <p>No clients connected</p>
            <p className="empty-state-hint">Waiting for clients to connect...</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default Clients;
