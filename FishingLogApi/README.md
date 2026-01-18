# FishingLogApi

## Deploy to IIS

These steps describe how to publish the FishingLog API to IIS on Windows.

### Prerequisites

- Windows Server with IIS installed.
- ASP.NET Core Hosting Bundle installed on the IIS server (matching the .NET runtime version used by the API).
- The server has the required database connection details available (typically via `appsettings.json` or environment variables).

### Publish the API

Run the publish command from the solution folder (or from the project folder):

```bash
dotnet publish FishingLogApi/FishingLogApi.csproj -c Release -o ./publish
```

This produces a `publish` folder containing the files that IIS will serve.

### Configure IIS

1. **Copy the published output** to the IIS server, for example to `C:\inetpub\FishingLogApi`.
2. **Create an IIS site** in IIS Manager:
   - Site name: `FishingLogApi` (or your preferred name).
   - Physical path: the folder you copied in step 1.
   - Binding: choose HTTP/HTTPS and the port/hostname you want.
3. **Set the Application Pool**:
   - Create or select an app pool for the site.
   - Set **.NET CLR version** to **No Managed Code** (ASP.NET Core runs via the hosting bundle).
4. **Confirm `web.config` exists** in the publish output (it is generated automatically by `dotnet publish`).
5. **Set environment variables** if needed:
   - Example: `ASPNETCORE_ENVIRONMENT=Production`.
   - You can set these in the IIS site configuration or in the app pool.

### Verify

- Browse to the site URL to confirm it responds.
- Check IIS logs and the Windows Event Viewer (Application logs) if the app fails to start.

### Common troubleshooting

- **HTTP 500.31**: install the correct .NET runtime/hosting bundle on the server.
- **HTTP 502.5**: check the app starts locally with `dotnet publish` output and review logs in Event Viewer.
