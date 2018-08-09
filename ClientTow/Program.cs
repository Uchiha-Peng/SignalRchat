using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClientTow
{
    public class JwtToken
    {
        public string Token { get; set; }
    }
    public class ChatMessages
    {
        public static List<string> messagesList = new List<string>();
        public static HubConnection connection;
        public static string userName;
        public static string password;
        private static async Task Main(string[] args)
        {
            JwtToken token = null;
            //获取Token
            do
            {
                Console.WriteLine("欢迎来到ClientTow\n请输入用户名：");
                do
                {
                    userName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        Console.WriteLine("必须输入用户名：");
                    }

                } while (string.IsNullOrWhiteSpace(userName));

                Console.WriteLine("请输入密码：");
                do
                {
                    password = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        Console.WriteLine("必须输入用密码：");
                    }

                } while (string.IsNullOrWhiteSpace(password));

                token = await GetToken(userName, password);

            } while (token.Token == null);
            try
            {
                //初始化连接
                connection = new HubConnectionBuilder()
                   .WithUrl("http://localhost:53730/hubs/chat", options =>
                   {
                       options.AccessTokenProvider = async () => await Task.Run(() =>
                       {
                           return token.Token;
                       });
                   })
                   .Build();

                Connect();
                int i = 0;
                string msg = string.Empty;
                Console.WriteLine("请输入群聊信息：\n————群聊中————");
                do
                {
                    msg = Console.ReadLine() ?? $"无语{i}次";
                    if (msg == "0")
                    {
                        break;
                    }
                    SendToGroup(msg);
                    i++;
                } while (i <= 100);
                i = 0;

                Console.WriteLine("请输入私聊用户名");
                string toUser = string.Empty;
                do
                {
                    toUser = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(toUser))
                    {
                        Console.WriteLine("必须输入私聊对象：");
                    }

                } while (string.IsNullOrWhiteSpace(toUser));
                do
                {
                    msg = Console.ReadLine() ?? $"无语{i}次";
                    if (msg == "0")
                    {
                        break;
                    }
                    SendToUser(toUser, msg);
                    i++;
                } while (i <= 100);
                i = 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
        public static async Task<JwtToken> GetToken(string userName, string pwd)
        {
            Dictionary<string, string> paraDic = new Dictionary<string, string>();
            paraDic.Add("email", userName);
            paraDic.Add("password", pwd);
            using (HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:53730") })
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                HttpResponseMessage httpResponse = await httpClient.PostAsync("/Account/Token", new FormUrlEncodedContent(paraDic));
                string json = string.Empty;
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    json = await httpResponse.Content.ReadAsStringAsync();
                    JwtToken token = JsonConvert.DeserializeObject<JwtToken>(json);
                    if (token != null)
                    {
                        return token;
                    }
                }
                return null;
            }

        }

        public static async void Connect()
        {
            //此处要和发送消息方法中的string方法名、参数一致
            //监控群聊消息
            connection.On<string>("ReceiveChatMessage", (message) =>
            {
                var newMessage = $"{message}";
                if (!newMessage.Contains(userName))
                {
                    Console.WriteLine(newMessage);
                }
                messagesList.Add(newMessage);
            });

            //监控私聊信息
            connection.On<string>("ReceiveDirectMessage", (message) =>
            {
                var newMessage = $"{message}";
                if (!newMessage.Contains(userName))
                {
                    Console.WriteLine(newMessage);
                }
                messagesList.Add(newMessage);
            });

            //监控系统信息
            connection.On<string>("ReceiveSystemMessage", (message) =>
            {
                var newMessage = $"{message}";
                if (!newMessage.Contains(userName))
                {
                    Console.WriteLine(newMessage);
                }
                messagesList.Add(newMessage);
            });

            try
            {
                await connection.StartAsync();
                messagesList.Add("Connection started");
            }
            catch (Exception ex)
            {
                messagesList.Add(ex.Message);
            }
        }

        public static async void SendToGroup(string message)
        {
            try
            {
                //此处方法名要和ChatHub发送消息对应的方法名、参数一致
                await connection.InvokeAsync("Send", message);
                Console.WriteLine($"我：{message}");
            }
            catch (Exception ex)
            {
                messagesList.Add(ex.Message);
            }
        }

        public static async void SendToUser(string userName, string message)
        {
            try
            {
                //此处方法名要和ChatHub发送消息对应的方法名、参数一致
                await connection.InvokeAsync("SendToUser", userName, message);
                Console.WriteLine($"我：{message}");
            }
            catch (Exception ex)
            {
                messagesList.Add(ex.Message);
            }
        }

    }
}
