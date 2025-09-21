# AI Chatbot Role-Based Access Control Implementation

## Overview
The AI chatbot has been successfully implemented with comprehensive role-based access control that restricts functionality and information based on the user's login status and role.

## Role-Based Access Control

### 1. Guest Users (Not Logged In)
- **Access Level**: General information only
- **Endpoint**: `/api/aiassistant/guest`
- **Capabilities**:
  - General car rental service information
  - Available car types and brands overview
  - Basic booking process explanation
  - How to get started guide
- **Restrictions**:
  - Cannot access specific account information
  - Cannot see admin or staff functionalities
  - Cannot view customer-specific data

### 2. Customer Users (Logged In Customers)
- **Access Level**: Customer-specific features only
- **Endpoint**: `/api/aiassistant/customer`
- **Session Validation**: Requires valid `CustomerId` in session
- **Capabilities**:
  - View and manage their own bookings
  - Browse available cars for rental
  - Make new reservations
  - Process payments and view payment history
  - Update profile information
  - Get help with rental-related questions
- **Restrictions**:
  - Cannot access admin or staff functions
  - Cannot see other customers' data
  - Cannot perform administrative tasks

### 3. Staff Users (Operational Access)
- **Access Level**: Operational functions only
- **Endpoint**: `/api/aiassistant/staff`
- **Session Validation**: Requires valid `StaffId` and `UserRole` = "Staff"
- **Capabilities**:
  - Process booking requests (approve/reject)
  - Handle rental confirmations
  - Manage vehicle returns and settlements
  - Provide customer service support
  - View booking and rental history
  - Access operational metrics
- **Restrictions**:
  - **CANNOT** access admin-only functions:
    - Staff management (add/edit/remove staff)
    - System configuration
    - Business analytics and reports
    - Car inventory management (add/remove vehicles)
    - Financial management
    - Strategic admin decisions

### 4. Admin Users (Full Access)
- **Access Level**: Complete administrative access
- **Endpoint**: `/api/aiassistant/admin`
- **Session Validation**: Requires `UserRole` = "Admin"
- **Capabilities**:
  - **Full booking management** (approve/reject/monitor)
  - **Complete car inventory management** (add/edit/remove vehicles)
  - **Comprehensive customer management**
  - **Staff management** (add/edit/remove staff members)
  - **Payment processing oversight**
  - **System reports and analytics**
  - **Business intelligence and insights**
  - **System configuration and settings**
  - **Financial reports and management**
  - **Strategic decision support**

## Implementation Details

### Backend Implementation

#### Controllers (`AIAssistantController.cs`)
- **Separate endpoints** for each role ensure proper access control
- **Session validation** prevents unauthorized access
- **Role-specific routing** directs users to appropriate functionality

#### Service Layer (`AIAssistantService.cs`)
- **Role-specific system prompts** guide AI behavior
- **Context filtering** ensures appropriate data access
- **Message filtering** prevents role-inappropriate requests
- **Response filtering** removes sensitive information

#### Key Security Features:
1. **Pre-request validation**: Checks user permissions before processing
2. **Context filtering**: Provides only role-appropriate data
3. **Message screening**: Blocks inappropriate queries
4. **Response sanitization**: Removes unauthorized information

### Frontend Implementation

#### JavaScript (`ai-assistant.js`)
- **Automatic role detection** from session metadata
- **Dynamic endpoint selection** based on user role
- **Role-specific UI elements** and quick actions

#### User Interface (`_AIAssistantPartial.cshtml`)
- **Role-specific welcome messages**
- **Customized quick action buttons** for each role
- **Contextual help options** based on user permissions

## Message Flow and Security

### 1. Guest User Flow
```
Guest → `/api/aiassistant/guest` → General car rental info only
```

### 2. Customer User Flow
```
Customer → Session validation → `/api/aiassistant/customer` → Customer-specific features
```

### 3. Staff User Flow
```
Staff → Session validation → `/api/aiassistant/staff` → Operational functions only
```

### 4. Admin User Flow
```
Admin → Session validation → `/api/aiassistant/admin` → Full administrative access
```

## Security Measures

### Session-Based Authentication
- All endpoints validate user sessions
- Role verification prevents privilege escalation
- Session timeout ensures security

### Multi-Layer Filtering
1. **Controller Level**: Endpoint access control
2. **Service Level**: Data and context filtering
3. **AI Prompt Level**: Behavior guidance
4. **Response Level**: Information sanitization

### Information Isolation
- Separate context building for each role
- Role-specific data access patterns
- Filtered response mechanisms

## Quick Actions by Role

### Guest Users
- "Tell me about your services"
- "What cars do you have available?"
- "How does the booking process work?"

### Customer Users
- "How do I book a car?"
- "Show my current bookings"
- "How do I make a payment?"

### Staff Users
- "Show pending bookings"
- "How do I approve a booking?"
- "How do I process a return?"
- "Show customer service tips"

### Admin Users
- "Show pending bookings"
- "Show staff management options"
- "Show system analytics"
- "How do I manage car inventory?"

## Error Handling
- **Unauthorized access** returns appropriate error messages
- **Invalid sessions** prompt re-authentication
- **Role violations** redirect to appropriate services
- **Graceful degradation** when services are unavailable

## Benefits of This Implementation

1. **Security**: Strict role-based access prevents unauthorized information access
2. **User Experience**: Contextual assistance relevant to user's role and permissions
3. **Scalability**: Easy to add new roles or modify permissions
4. **Maintainability**: Clear separation of concerns and role-specific logic
5. **Compliance**: Ensures data access follows business rules and regulations

## Testing the Implementation

To test the role-based access control:

1. **As a Guest**: Access the chatbot without logging in - should only get general information
2. **As a Customer**: Log in as a customer - should only see customer-related features
3. **As a Staff**: Log in as staff - should see operational features but not admin functions
4. **As an Admin**: Log in as admin - should have access to all administrative features

The system will automatically detect the user's role and provide appropriate functionality while blocking unauthorized access attempts.