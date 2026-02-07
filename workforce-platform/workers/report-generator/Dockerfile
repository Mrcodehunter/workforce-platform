# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies (production only)
RUN npm ci --omit=dev

# Runtime stage
FROM node:20-alpine

WORKDIR /app

# Copy dependencies from build stage
COPY --from=build /app/node_modules ./node_modules

# Copy application code
COPY . .

# Run as non-root user
USER node

CMD ["node", "src/index.js"]
