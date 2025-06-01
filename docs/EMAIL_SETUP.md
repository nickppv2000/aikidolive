# 📧 Email Functionality - Setup Guide

This document describes the SendGrid email integration added to Aikido Live for user authentication features.

## Features

### ✅ Email Confirmation
- New users receive confirmation emails upon registration
- Users must verify their email before they can log in
- Confirmation tokens expire after 7 days
- Clear error messages guide users through the process

### 🔐 Password Reset
- "Forgot Password" link on login page
- Secure token-based password reset via email
- Reset tokens expire after 24 hours
- Users can safely reset passwords without exposing account information

## Configuration

### SendGrid Setup

1. **Create SendGrid Account**: Sign up at [sendgrid.com](https://sendgrid.com)

2. **Generate API Key**: 
   - Go to Settings > API Keys
   - Create a new API key with "Full Access" or "Mail Send" permissions
   - Copy the API key (starts with `SG.`)

3. **Configure Application Settings**:

```json
{
  "SendGrid": {
    "ApiKey": "SG.your_actual_api_key_here",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Your App Name"
  }
}
```

### Environment-Specific Configuration

#### Development (`appsettings.Development.json`)
```json
{
  "SendGrid": {
    "ApiKey": "SG.your_development_api_key",
    "FromEmail": "dev@yourdomain.com",
    "FromName": "Your App (Development)"
  }
}
```

#### Production (Azure App Service Configuration)
Set these as Application Settings in Azure:
- `SendGrid__ApiKey`: Your production SendGrid API key
- `SendGrid__FromEmail`: Your verified sender email
- `SendGrid__FromName`: Your application name

## Email Templates

### Confirmation Email
- Subject: "Confirm Your Aikido Live Account"
- Contains activation link with secure token
- Professional HTML template with fallback plain text

### Password Reset Email
- Subject: "Reset Your Aikido Live Password"
- Contains reset link with secure token
- Includes security notice about 24-hour expiration

## User Flow

### Registration Flow
1. User fills out registration form
2. User account created with `IsEmailConfirmed = false`
3. Confirmation email sent automatically
4. User redirected to "Email Sent" page
5. User clicks link in email
6. Email confirmed, user can now log in

### Password Reset Flow
1. User clicks "Forgot Password" on login page
2. User enters email address
3. Reset email sent (if account exists)
4. User clicks link in email
5. User enters new password
6. Password updated, user can log in

## Security Features

- **Token-Based**: All email links use cryptographically secure tokens
- **Expiration**: Tokens automatically expire (7 days for confirmation, 24 hours for reset)
- **One-Time Use**: Tokens are invalidated after use
- **No Information Disclosure**: Password reset doesn't reveal if email exists
- **HTTPS Required**: All email links use HTTPS scheme

## Error Handling

### Login Errors
- "Please confirm your email address before logging in" - for unconfirmed accounts
- "Invalid email or password" - for authentication failures

### Email Errors
- Graceful fallback if SendGrid is unavailable
- Logged errors for troubleshooting
- User sees generic success message for security

## Testing

### Development Testing
1. Use SendGrid sandbox mode for development
2. Check logs for email send confirmations
3. Copy confirmation/reset URLs from logs for testing

### Email Testing Services
- Use services like [MailHog](https://github.com/mailhog/MailHog) for local testing
- [Ethereal Email](https://ethereal.email/) for development email capture

## Troubleshooting

### Common Issues

1. **Emails Not Sending**
   - Check API key is correct and has proper permissions
   - Verify FromEmail is verified in SendGrid
   - Check application logs for SendGrid errors

2. **Links Not Working**
   - Ensure application URL is correct in configuration
   - Check HTTPS is enabled
   - Verify tokens haven't expired

3. **Confirmation Loops**
   - Check database for `IsEmailConfirmed` status
   - Verify email confirmation logic is working
   - Look for duplicate user accounts

### Logs
Monitor application logs for:
- `Email confirmed successfully for token: {token}`
- `Password reset successfully for token: {token}`
- `Failed to send confirmation email to {email}`

## Database Changes

The User model now includes:
```csharp
public bool IsEmailConfirmed { get; set; }
public string? EmailConfirmationToken { get; set; }
public DateTime? EmailConfirmationTokenExpiry { get; set; }
public string? PasswordResetToken { get; set; }
public DateTime? PasswordResetTokenExpiry { get; set; }
```

Existing users will need their `IsEmailConfirmed` set to `true` to maintain access.

## API Reference

### IEmailService Methods
- `SendConfirmationEmailAsync(email, firstName, confirmationLink)` - Send account confirmation
- `SendPasswordResetEmailAsync(email, firstName, resetLink)` - Send password reset

### IAuthService New Methods
- `ConfirmEmailAsync(token)` - Confirm email with token
- `SendPasswordResetEmailAsync(email)` - Initiate password reset
- `ResetPasswordAsync(token, newPassword)` - Complete password reset
- `GetUserByEmailAsync(email)` - Get user by email address

## Pages Added
- `/Account/EmailSent` - Confirmation that email was sent
- `/Account/ConfirmEmail?token={token}` - Email confirmation page
- `/Account/ForgotPassword` - Password reset request form
- `/Account/ResetPassword?token={token}` - Password reset form