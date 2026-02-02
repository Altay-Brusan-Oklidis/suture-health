using Microsoft.AspNetCore.SignalR;
using SutureHealth.AspNetCore.WebSockets.Clients;
using SutureHealth.AspNetCore.WebSockets.Models;

namespace SutureHealth.AspNetCore.WebSockets.Hubs;

public class AccountAuditHub : Hub<IAccountAuditClient>
{
    public async Task SendAccountAuditMessage(AccountAuditMessage message)
    {
        await Clients.All.ReceiveMessage(message);
    }
}