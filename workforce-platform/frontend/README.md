# Workforce Frontend

React + TypeScript frontend for the Workforce Management Platform.

## Prerequisites

- Node.js 18+ and npm/yarn
- Backend API running (or update `VITE_API_URL` in `.env`)

## Installation

```bash
# Install dependencies
npm install
```

## Running the Frontend

### Development Mode

```bash
npm run dev
```

This will start the Vite dev server on `http://localhost:3000`

### Production Build

```bash
# Build for production
npm run build

# Preview production build
npm run preview
```

## Configuration

### API URL

The frontend connects to the backend API. The default depends on how the backend is running:

- **Docker Compose (default):** `http://localhost:5000/api`
- **Standalone backend:** `http://localhost:63890/api`

To change the API URL:

1. Create a `.env` file in the frontend directory:
```bash
# For Docker Compose
VITE_API_URL=http://localhost:5000/api

# For standalone backend
VITE_API_URL=http://localhost:63890/api
```

2. Restart the dev server after changing `.env`:
```bash
npm run dev
```

**Note:** If you get `ERR_CONNECTION_REFUSED`, check:
- Is the backend running?
- Is the port correct?
- See `API_CONNECTION.md` for troubleshooting

### Environment Variables

Create a `.env` file with:

```env
VITE_API_URL=http://localhost:63890/api
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Development

The frontend uses:
- **React 18** with TypeScript
- **Vite** as build tool
- **Tailwind CSS** for styling
- **React Query** for data fetching
- **React Router** for routing
- **Recharts** for data visualization

## Project Structure

```
src/
├── components/     # Reusable components
├── pages/          # Page components
├── services/       # API service layer
├── hooks/          # Custom React hooks
├── types/          # TypeScript types
└── utils/          # Utility functions
```
