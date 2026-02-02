namespace SutureHealth.AspNetCore.WebSockets.Models;

public class AccountAuditMessage
{
    public string UserName { get; set; }
    public string AuditEvent { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}