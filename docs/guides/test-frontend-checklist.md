# Frontend Testing Checklist

## 🌐 Access the Application
1. Open browser: http://localhost:4200
2. Check if Angular app loads without errors
3. Open browser console (F12) to check for JavaScript errors

## 📋 Navigation Testing
- [ ] Click "Dashboard" - should load dashboard
- [ ] Click "User Management" - should load users page
- [ ] Click "Role Management" - should load roles page  
- [ ] Click "Projects" - should load projects page
- [ ] Click "Locations" - should load locations page

## 👥 User Management Testing
### List Users
- [ ] Users table loads with data
- [ ] Role names display correctly (not "Unknown")
- [ ] User status shows Active/Inactive
- [ ] Action buttons are visible

### Create User
- [ ] Click "Add User" - modal opens
- [ ] Fill form fields:
  - First Name: Test
  - Last Name: User
  - Username: auto-generates
  - Email: test@company.com
  - Role: Select any role
  - Password: TestPass123!
- [ ] Click "Create User" - user appears in list
- [ ] Success message shows

### Edit User
- [ ] Click edit button on any user
- [ ] Modal opens with user data
- [ ] Change first name
- [ ] Click "Update User"
- [ ] Changes reflect in list

### Deactivate/Activate User
- [ ] Click deactivate button
- [ ] User status changes to "Inactive"
- [ ] Click activate button
- [ ] User status changes to "Active"

## 🔐 Role Management Testing
- [ ] Roles list loads
- [ ] Can create new role
- [ ] Can assign permissions to role
- [ ] Permission matrix works

## 📁 Project Management Testing
- [ ] Projects list loads
- [ ] Can create new project
- [ ] Auto-generated project codes work
- [ ] Can assign locations to projects

## 📍 Location Management Testing
- [ ] Locations list loads
- [ ] Can create new location
- [ ] Dropdown selections work (region, state, etc.)
- [ ] Location names auto-generate

## 🔍 Console Errors Check
Open browser console and check for:
- [ ] No red error messages
- [ ] API calls return 200 status
- [ ] No CORS errors
- [ ] No TypeScript compilation errors