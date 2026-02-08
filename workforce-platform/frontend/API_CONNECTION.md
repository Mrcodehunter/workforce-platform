# API Connection Guide

## Connection Refused Error

If you're getting `ERR_CONNECTION_REFUSED`, check the following:

### 1. Is the Backend Running?

**Docker Compose:**
```bash
docker compose ps
```
Check if the `workforce-api` container is running.

**Standalone:**
```bash
cd backend/WorkforceAPI
dotnet run
```
The backend should start on `http://localhost:63890` or `https://localhost:63889`

### 2. Check the API URL Configuration

The frontend uses different API URLs depending on how the backend is running:

**Docker Compose (default):**
- Backend runs on: `http://localhost:5000`
- Frontend should use: `http://localhost:5000/api`

**Standalone Backend:**
- Backend runs on: `http://localhost:63890` (from launchSettings.json)
- Frontend should use: `http://localhost:63890/api`

### 3. Configure Frontend API URL

Create a `.env` file in the `frontend` directory:

**For Docker Compose:**
```env
VITE_API_URL=http://localhost:5000/api
```

**For Standalone Backend:**
```env
VITE_API_URL=http://localhost:63890/api
```

### 4. Verify Backend is Accessible

Test the backend directly in your browser or with curl:

```bash
# Docker Compose
curl http://localhost:5000/health

# Standalone
curl http://localhost:63890/health
```

### 5. Check CORS Configuration

The backend CORS is configured to allow `http://localhost:3000`. Make sure:
- Frontend is running on port 3000
- Backend CORS policy includes the frontend URL

### 6. Common Issues

**Issue:** Backend not running
- **Solution:** Start the backend (Docker or standalone)

**Issue:** Wrong port
- **Solution:** Update `.env` file with correct port

**Issue:** CORS error (not connection refused)
- **Solution:** Check backend CORS configuration in `Program.cs`

**Issue:** Backend running but not accessible
- **Solution:** Check if port is already in use or firewall blocking

## Quick Fix

1. **If using Docker Compose:**
   ```bash
   docker compose up api
   ```
   Frontend should use: `http://localhost:5000/api`

2. **If running backend standalone:**
   ```bash
   cd backend/WorkforceAPI
   dotnet run
   ```
   Create `.env` file in frontend:
   ```env
   VITE_API_URL=http://localhost:63890/api
   ```

3. **Restart frontend dev server** after changing `.env`:
   ```bash
   npm run dev
   ```
