using SutureHealth.AspNetCore.WebSockets.Models;

namespace SutureHealth.AspNetCore.WebSockets.Clients;

public interface IAccountAuditClient
{
    Task ReceiveMessage(AccountAuditMessage message);
}
