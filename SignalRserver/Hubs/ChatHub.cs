using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRAuthenticationSample.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        //监控人员
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceiveSystemMessage", $"[系统消息]{Context.UserIdentifier} 上线.");
            await base.OnConnectedAsync();
        }

        //退出聊天
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("ReceiveSystemMessage", $"[系统消息]{Context.UserIdentifier} 下线.");
            await base.OnDisconnectedAsync(exception);
        }

        //接收私聊消息
        public async Task SendToUser(string user, string message)
        {
            await Clients.User(user).SendAsync("ReceiveDirectMessage", $"[私聊]{Context.UserIdentifier}: {message}");
        }

        //接收群聊聊天消息
        public async Task Send(string message)
        {
            await Clients.All.SendAsync("ReceiveChatMessage", $"[群聊]{Context.UserIdentifier}: {message}");
        }
    }
}
