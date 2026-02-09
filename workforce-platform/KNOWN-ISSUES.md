# Known Issues and Future Improvements

This document outlines incomplete features and limitations that would be addressed given more time.

## 🔐 Authentication & Authorization
**Status**: Not Implemented  
**Impact**: All endpoints publicly accessible, no user tracking, audit logs show "System" as actor  
**Plan**: JWT authentication, RBAC, user context integration, protected routes

## 🔍 Search Functionality
**Status**: Not Implemented  
**Impact**: Users must manually browse lists, poor UX for large datasets  
**Plan**: PostgreSQL/MongoDB full-text search, search API endpoints, frontend search UI

## 🔔 Real-Time Notifications
**Status**: Not Implemented  
**Impact**: Users must refresh to see updates, no alerts for important events  
**Plan**: SignalR WebSocket integration, notification service, toast notifications

## 🧪 Testing
**Status**: Not Implemented  
**Missing**: Unit tests, integration tests, E2E tests  
**Plan**: xUnit (backend), Jest + React Testing Library (frontend), Playwright/Cypress (E2E)

## ☁️ Cloud Deployment
**Status**: Not Implemented  
**Missing**: Kubernetes manifests, CI/CD pipelines, cloud services, monitoring  
**Plan**: Container orchestration, automated deployments, managed databases, observability


## 🐛 Known Bugs
1. **Audit Log Actor**: Always shows "System" - requires authentication implementation
2. **Leave Request Approval**: Uses placeholder user - needs auth context
3. **CORS Configuration**: Allows all methods/headers - security risk in production
4. **Default Passwords**: Used in `.env` - security risk if deployed unchanged

## 🚀 Performance Optimizations
**Areas**: Database query optimization, API response caching, frontend bundle optimization, background job batching

## 📚 Documentation
**Status**: Partially Complete  
**Missing**: API examples, ADRs, developer onboarding guide  
**Note**: Third-party library documentation completed in `THIRD_PARTY_LIBRARIES.md`

---

**Last Updated**: February 9, 2026
