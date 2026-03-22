namespace ExpenseTracker.Infrastructure.Services.Email;

public static class EmailTemplates
{
    private static string Base(string content) => $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'><style>
  body {{ font-family: Arial, sans-serif; background: #f4f6f9; margin: 0; padding: 20px; }}
  .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
  .header {{ background: linear-gradient(135deg, #6366F1, #4F46E5); padding: 30px; text-align: center; }}
  .header h1 {{ color: white; margin: 0; font-size: 24px; }}
  .body {{ padding: 30px; }}
  .btn {{ display: inline-block; background: #6366F1; color: white; padding: 14px 28px; border-radius: 8px; text-decoration: none; font-weight: bold; margin: 20px 0; }}
  .footer {{ background: #f8fafc; padding: 20px; text-align: center; color: #94a3b8; font-size: 12px; }}
</style></head>
<body><div class='container'>
  <div class='header'><h1>💰 ExpenseTracker</h1></div>
  <div class='body'>{content}</div>
  <div class='footer'><p>© {DateTime.UtcNow.Year} ExpenseTracker. All rights reserved.</p></div>
</div></body></html>";

    public static string Welcome(string name, string verifyUrl) => Base($@"
<h2>Welcome, {name}! 🎉</h2>
<p>Thank you for joining ExpenseTracker. Please verify your email to get started.</p>
<a href='{verifyUrl}' class='btn'>Verify Email</a>
<p>Link expires in 24 hours. If you did not create an account, ignore this email.</p>");

    public static string PasswordReset(string name, string resetUrl) => Base($@"
<h2>Password Reset Request</h2>
<p>Hi {name}, we received a request to reset your password.</p>
<a href='{resetUrl}' class='btn'>Reset Password</a>
<p>Link expires in 2 hours. If you did not request this, ignore this email.</p>");

    public static string EmailVerification(string name, string verifyUrl) => Base($@"
<h2>Verify Your Email</h2>
<p>Hi {name}, please verify your email address.</p>
<a href='{verifyUrl}' class='btn'>Verify Email</a>
<p>Link expires in 24 hours.</p>");

    public static string PasswordChanged(string name) => Base($@"
<h2>Password Changed</h2>
<p>Hi {name}, your ExpenseTracker password was recently changed.</p>
<p>If you did not make this change, please contact support immediately.</p>");
}
