🎓 Smart Attendance System (Full-Stack Master's Project)

📌 Overview

The Smart Attendance System is an enterprise-grade, full-stack application designed to eliminate attendance fraud in academic institutions. Built as a Master's degree project (2026), this system leverages modern web and mobile technologies to provide a secure, real-time, and strictly verified attendance tracking mechanism.

It moves beyond traditional roll-calls and simple QR codes by introducing a rigorous Tri-Factor Authentication process.

✨ Core Innovations & Features

🛡️ Tri-Factor Attendance Verification

To mark attendance, a student must simultaneously pass three security checks:

Dynamic QR Code: Scanned from the instructor's live dashboard (regenerated dynamically).

GPS Geofencing: The system uses the Haversine formula (NetTopologySuite) to calculate the exact distance between the student's mobile GPS and the classroom's pre-defined geofence center.

Biometric/Visual Proof: The student must capture a live selfie (or facial encoding) at the exact moment of check-in.

👥 Role-Based Dashboards (RBAC)

Admin: Manages campus infrastructure (Faculties, Departments, Rooms with GPS coordinates), oversees users, monitors system-wide security alerts (fraud attempts), and generates comprehensive tracking reports.

Instructor: Generates live QR sessions, monitors real-time student check-ins, approves/rejects medical excuses, and exports attendance analytics as PDF reports.

Student: Scans QR codes for attendance, views academic history/percentages, and submits medical excuses with image attachments.

🛠️ Technology Stack

Backend (RESTful API):

Framework: C# / ASP.NET Core Web API (.NET)

Database: SQL Server with Entity Framework Core

Spatial Data: NetTopologySuite for geographic coordinates and geofence radius calculations.

Security: JWT (JSON Web Tokens) with strict Audience/Issuer validation.

Frontend (Cross-Platform App):

Framework: Flutter (Dart)

State Management: Provider architecture.

Hardware Integration: mobile_scanner (QR), geolocator (GPS), image_picker (Camera).

Reporting: pdf and printing packages with full Arabic RTL (Cairo font) support.

UI/UX: Fully responsive design adapting to Mobile, Tablet, and Desktop (Web) views.

🚀 Installation & Local Setup

1. Backend Setup (.NET)

Navigate to the Backend directory.

Update appsettings.json with your SQL Server connection string.

Apply Entity Framework migrations to build the database:

dotnet ef database update


Run the API:

dotnet run


2. Frontend Setup (Flutter)

Navigate to the Frontend directory.

Install dependencies:

flutter pub get


Update the ApiConstants.dart file to point to your local or hosted API URL. (Note: A custom HttpOverrides class is included to bypass localhost SSL certificate issues during development).

Run the app:

flutter run


📊 Security & Analytics

The system includes a dedicated SecurityAlerts module. Any attempt by a student to check in from outside the permitted classroom radius (e.g., trying to scan a photo of the QR code sent by a friend while at home) is blocked and logged directly to the Instructor's and Admin's fraud detection dashboards.
