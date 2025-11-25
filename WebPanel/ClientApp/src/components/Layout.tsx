import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useAuthStore } from '../stores/authStore';
import { useClientsStore } from '../stores/clientsStore';
import './Layout.css';

function Layout() {
  const { user, logout } = useAuthStore();
  const { connect, disconnect, clients } = useClientsStore();
  const navigate = useNavigate();

  useEffect(() => {
    connect();
    return () => {
      disconnect();
    };
  }, []);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const onlineClients = clients.filter((c) => c.isConnected).length;

  return (
    <div className="layout">
      <nav className="navbar">
        <div className="navbar-brand">
          <h1>XyaRat</h1>
          <span className="navbar-version">Web Panel</span>
        </div>
        <div className="navbar-menu">
          <Link to="/" className="nav-link">
            Dashboard
          </Link>
          <Link to="/clients" className="nav-link">
            Clients ({onlineClients})
          </Link>
        </div>
        <div className="navbar-user">
          <span className="user-name">{user?.username}</span>
          <span className="user-role">{user?.role}</span>
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>
      </nav>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}

export default Layout;
