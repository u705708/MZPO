using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("[controller]")]
    public class UberController : ControllerBase
    {
        private readonly List<Task> _tasks;
        private readonly TimeSpan _timeOut;
        private readonly Uber _uber;
        private States currentState;

        private Task<WebSocketReceiveResult> incomingMessageTask;
        private UberLead lead;
        private DateTime validity;
        private int visitor_id;

        private Task waitAfterDistributionTask = Task.CompletedTask;
        private Task waitForTimeOutTask = Task.CompletedTask;
        private Task waitForUpdateTask = Task.CompletedTask;
        private Task waitToAcceptTask = Task.CompletedTask;

        public UberController(Uber uber)
        {
            _uber = uber;
            _tasks = new();
            _timeOut = TimeSpan.FromSeconds(90);
            validity = DateTime.Now.Add(_timeOut);
            lead = new();
        }

        private enum Results
        {
            Accepted,
            Declined,
            DND,
            Ignored,
            Active
        }

        private enum States
        {
            WaitingForDistribution,
            Distributed,
            WaitingAfterDistribution,
            Idle
        }

        [Route("wss")]
        [HttpGet("{user_id}")]
        public async Task Get(int user_id)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                visitor_id = user_id;
                using WebSocket webSocket = await
                                   HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task<bool> AcceptLead(UberLead lead, int visitor_id)
        {
            return await _uber.AcceptLead(lead, visitor_id);
        }

        private async Task<bool> CheckForTimeout(WebSocket webSocket)
        {
            if (DateTime.Now > validity)
            {
                if (currentState == States.Distributed) DeclineLead();
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                return true;
            }
            return false;
        }

        private async Task<bool> CheckResultForClosure(WebSocketReceiveResult result, WebSocket webSocket)
        {
            if (result.CloseStatus.HasValue)
            {
                if (currentState == States.Distributed) DeclineLead();
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                return true;
            }
            return false;
        }

        private void DeclineLead()
        {
            _uber.DeclineLead(lead, visitor_id);
            currentState = States.Idle;
            lead = new();
        }

        private void Ping(int visitor_id, bool updateController = false)
        {
            validity = DateTime.Now.Add(_timeOut);
            if (updateController)
            {
                _uber.RefreshUser(visitor_id, ReceiveLead);
                return;
            }
            _uber.RefreshUser(visitor_id);
        }

        private Results ProcessResult(string message)
        {
            return message switch
            {
                "accept" => Results.Accepted,
                "decline" => Results.Declined,
                "dnd" => Results.DND,
                "active" => Results.Active,
                _ => Results.Ignored,
            };
        }

        private void ReceiveLead(UberLead lead)
        {
            this.lead = lead;
            currentState = States.Distributed;
        }

        private void RequestLead(int visitor_id)
        {
            currentState = States.WaitingForDistribution;
            _uber.RequestLead(visitor_id, ReceiveLead);
        }
        private Task WaitForSeconds(int seconds)
        {
            return Task.Delay(TimeSpan.FromSeconds(seconds));
        }

        private async Task Echo(HttpContext httpContext, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            currentState = States.Idle;

            _tasks.Add(incomingMessageTask = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None));
            _tasks.Add(waitForTimeOutTask = WaitForSeconds(60));
            _tasks.Add(waitForUpdateTask = WaitForSeconds(1));
            RequestLead(visitor_id);

            while (webSocket.State == WebSocketState.Open)
            {
                await Task.WhenAny(_tasks);

                //Прошло 120 секунд, проверям на таймаут
                if (waitForTimeOutTask.IsCompleted)
                {
                    if (await CheckForTimeout(webSocket)) break;

                    _tasks.Remove(waitForTimeOutTask);
                    _tasks.Add(waitForTimeOutTask = WaitForSeconds(60));
                }

                //Пришло сообщение
                if (incomingMessageTask.IsCompleted)
                {
                    var message = incomingMessageTask.Result;
                    if (await CheckResultForClosure(message, webSocket)) break;

                    Results result = ProcessResult(Encoding.UTF8.GetString(buffer, 0, message.Count));

                    if (currentState == States.Distributed)
                    {
                        switch (result)
                        {
                            case Results.Accepted:
                                if (!await AcceptLead(lead, visitor_id)) break;
                                var serverMsg = Encoding.UTF8.GetBytes(lead.leadUri);
                                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                                currentState = States.WaitingAfterDistribution;
                                lead = new();
                                _tasks.Add(waitAfterDistributionTask = WaitForSeconds(45));
                                break;

                            case Results.Declined:
                                DeclineLead();
                                _tasks.Add(waitAfterDistributionTask = WaitForSeconds(15));
                                break;

                            case Results.DND:
                                DeclineLead();
                                _tasks.Add(waitAfterDistributionTask = WaitForSeconds(900));
                                break;

                            case Results.Ignored:
                                DeclineLead();
                                break;
                        }

                        if (_tasks.Contains(waitToAcceptTask)) _tasks.Remove(waitToAcceptTask);
                    }

                    Ping(visitor_id, true);
                    _tasks.Remove(incomingMessageTask);
                    _tasks.Add(incomingMessageTask = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None));
                }

                //Распределилась сделка
                if (currentState == States.Distributed &&
                    !_tasks.Contains(waitToAcceptTask))
                {
                    var serverMsg = Encoding.UTF8.GetBytes($"distribution_start {lead.leadName}");
                    await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);

                    _tasks.Add(waitToAcceptTask = WaitForSeconds(15));
                }

                //Не приняли сделку за 15 сек
                if (_tasks.Contains(waitToAcceptTask) &&
                    waitToAcceptTask.IsCompleted)
                {
                    DeclineLead();
                    _tasks.Remove(waitToAcceptTask);
                    RequestLead(visitor_id);
                }

                //Прошло ожидание после распределения
                if (_tasks.Contains(waitAfterDistributionTask) &&
                    waitAfterDistributionTask.IsCompleted)
                {
                    currentState = States.Idle;
                    _tasks.Remove(waitAfterDistributionTask);
                    RequestLead(visitor_id);
                }

                //Прошла 1 сек на апдейт
                if (waitForUpdateTask.IsCompleted)
                {
                    _tasks.Remove(waitForUpdateTask);
                    _tasks.Add(waitForUpdateTask = WaitForSeconds(1));
                }
            }
        }
    }
}