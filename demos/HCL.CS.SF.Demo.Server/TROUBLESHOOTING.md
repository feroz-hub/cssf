# HCL.CS.SF Demo Server - Troubleshooting Guide

## Issue: Page Not Loading / Server Won't Start

### Problem: "Address already in use" Error

If you see an error like:

```
Failed to bind to address https://127.0.0.1:5001: address already in use.
System.Net.Sockets.SocketException (48): Address already in use
```

This means another process is already using port 5001.

### Solutions

#### Solution 1: Kill the Existing Process

1. Find the process using port 5001:
   ```bash
   lsof -i :5001
   ```

2. Kill the process (replace `<PID>` with the actual process ID):
   ```bash
   kill -9 <PID>
   ```

3. Restart the server:
   ```bash
   dotnet run --project HCL.CS.SF.DemoServerApp.csproj
   ```

#### Solution 2: Change the Port

You can configure a different port in `appsettings.Development.json`:

```json
{
  "Urls": "https://localhost:5002",
  "Logging": {
    // ...existing configuration...
  }
}
```

Or run with a specific port:

```bash
dotnet run --project HCL.CS.SF.DemoServerApp.csproj --urls "https://localhost:5002"
```

### Verifying the Server is Running

1. Check if the process is listening:
   ```bash
   lsof -i :5001
   ```

2. Test the endpoint:
   ```bash
   curl -k -I https://localhost:5001
   ```

   You should see `HTTP/2 200` in the response.

3. Open in browser:
    - Navigate to: https://localhost:5001

### Common Issues

1. **Certificate Warnings in Browser**: The server uses self-signed certificates. Click "Advanced" and "Proceed" to
   continue.

2. **Database Connection Issues**: Ensure PostgreSQL is running and the connection string in
   `Configurations/SystemSettings.json` is correct.

3. **Missing Certificates**: The server will auto-generate certificates in the `./Certificates/` folder on first run.

### Running the Server

```bash
# From the HCL.CS.SF.Demo.Server directory
dotnet run --project HCL.CS.SF.DemoServerApp.csproj

# Or just
dotnet run
```

The server will be available at:

- HTTPS: https://localhost:5001
- HTTP: Not configured (HTTPS only for security)

### Default Credentials

Check the seeded data in your database or the documentation for default admin credentials.

