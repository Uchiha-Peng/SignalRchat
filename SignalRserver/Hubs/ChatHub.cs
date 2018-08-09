using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRAuthenticationSample.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        //�����Ա
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceiveSystemMessage", $"[ϵͳ��Ϣ]{Context.UserIdentifier} ����.");
            await base.OnConnectedAsync();
        }

        //�˳�����
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("ReceiveSystemMessage", $"[ϵͳ��Ϣ]{Context.UserIdentifier} ����.");
            await base.OnDisconnectedAsync(exception);
        }

        //����˽����Ϣ
        public async Task SendToUser(string user, string message)
        {
            await Clients.User(user).SendAsync("ReceiveDirectMessage", $"[˽��]{Context.UserIdentifier}: {message}");
        }

        //����Ⱥ��������Ϣ
        public async Task Send(string message)
        {
            await Clients.All.SendAsync("ReceiveChatMessage", $"[Ⱥ��]{Context.UserIdentifier}: {message}");
        }
    }
}
