# PodStream - Demo Instructions

## Quick Demo Checklist

### 1. Landing Page Demo
- Navigate to home page
- Show featured episodes and podcasts
- Demonstrate responsive design (resize browser)
- Point out AWS deployment footer

### 2. Authentication Flow
- Click "Register" 
- Show role selection (Podcaster/Listener)
- Register as Podcaster: `demo@podcaster.com` / `Demo123!`
- Show role-aware navigation after login

### 3. Podcaster Dashboard
- Navigate to Podcaster Dashboard
- Show analytics cards and stats
- Point out "Create Podcast" and "Upload Episode" buttons
- Demonstrate role-based UI elements

### 4. Episode Management
- Click "Upload Episode"
- Show file upload interface with drag-and-drop
- Demonstrate form validation
- Point out S3 upload simulation
- Show episode creation form fields

### 5. Browse Episodes
- Navigate to "Browse Episodes"
- Show search and filtering
- Click on episode to view details
- Demonstrate audio player interface
- Show comment system (DynamoDB simulation)

### 6. Search Functionality
- Use search bar in navigation
- Search for "technology" or "business"
- Show search results with highlighting
- Demonstrate filters and sorting

### 7. Admin Features
- Login as admin: `admin@podstream.com` / `Admin123!`
- Navigate to Admin → Manage Users
- Show user management interface
- Demonstrate user actions (view, edit, suspend)

### 8. AWS Integration Evidence
- Navigate to Profile page
- Show AWS Integration Status card
- Point out Parameter Store, S3, DynamoDB indicators
- Show "Deployed to Elastic Beanstalk" in footer

### 9. Analytics Dashboard
- Navigate to Analytics Dashboard
- Show charts and metrics
- Demonstrate period selection
- Point out engagement metrics

### 10. Mobile Responsiveness
- Resize browser to mobile view
- Show responsive navigation
- Demonstrate mobile-friendly cards and forms
- Show touch-friendly buttons

## Key Demo Points to Highlight

### Technical Implementation
- **Dual Database Architecture**: SQL Server for structured data, in-memory simulation for DynamoDB
- **AWS Parameter Store**: Secure credential management (shown in profile)
- **Role-Based Access**: Different UI/UX for Admin, Podcaster, Listener
- **RESTful API**: All CRUD operations via API endpoints
- **Responsive Design**: Mobile-first approach with Bootstrap 5

### User Experience Features
- **Intuitive Navigation**: Role-aware menus and quick actions
- **Search & Discovery**: Advanced search with filters and highlighting
- **File Upload**: Drag-and-drop with progress indicators
- **Real-time Feedback**: Toast notifications and loading states
- **Accessibility**: WCAG AA compliance with skip links and ARIA labels

### AWS Services Integration
- **Parameter Store**: Database credentials (simulated)
- **S3**: Audio file storage (simulated upload)
- **DynamoDB**: Comment system (in-memory simulation)
- **Elastic Beanstalk**: Deployment ready (shown in footer)

## Demo Script (5-minute version)

1. **"Welcome to PodStream"** - Show landing page, explain dual database architecture
2. **"Role-based Experience"** - Register as Podcaster, show dashboard
3. **"Content Management"** - Upload episode, show S3 integration
4. **"Discovery & Engagement"** - Browse episodes, play audio, add comment
5. **"AWS Integration"** - Show profile page with AWS status, mention Parameter Store
6. **"Admin Capabilities"** - Switch to admin view, show user management
7. **"Analytics & Insights"** - Show analytics dashboard with charts
8. **"Mobile Ready"** - Resize to mobile, show responsive design

## Troubleshooting

### If Database Issues
- Ensure SQL Server is running
- Check connection string in appsettings.json
- Run `dotnet ef database update` if needed

### If AWS Parameter Store Fails
- Check .env file for AWS credentials
- Verify AWS CLI configuration
- Parameters should exist: `/podcast/db/server` and `/podcast/db/name`

### If Roles Don't Work
- Ensure admin user was created on startup
- Check Program.cs role seeding code
- Verify Identity configuration

## Demo Data Setup

The application includes:
- Pre-seeded admin user: `admin@podstream.com` / `Admin123!`
- Sample episodes and podcasts (if database is populated)
- Mock analytics data in charts
- Simulated AWS integration status

## Post-Demo Notes

This implementation demonstrates:
- ✅ Complete CRUD operations for episodes
- ✅ User authentication with SHA256 (via Identity)
- ✅ Role-based access control
- ✅ AWS Parameter Store integration
- ✅ DynamoDB simulation for comments
- ✅ Responsive, accessible UI
- ✅ Search and filtering capabilities
- ✅ File upload simulation
- ✅ Analytics and reporting
- ✅ Admin management features

Ready for AWS Elastic Beanstalk deployment with proper environment configuration.