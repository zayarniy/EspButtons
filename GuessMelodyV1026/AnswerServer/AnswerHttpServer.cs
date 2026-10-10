using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GuessMelody.AnswerServer
{
    public class AnswerHttpServer : IDisposable
    {
        private HttpListener _listener;
        private bool _running;

        public int Port { get; private set; }
        public string Title { get; set; } = "Угадай мелодию";
        public string PlaceholderText { get; set; } = "";
        public string StateHtml { get; set; } = "";        // блок состояния игры (заполнит VM)
        public bool IsRunning => _running;

        public event EventHandler<string> Log;

        public void Start(int port)
        {
            if (_running) return;

            Port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{port}/");

            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Log?.Invoke(this,
                    $"Не удалось запустить HTTP на порту {port}: {ex.Message}. " +
                    $"Возможно, нужны права администратора или зарезервированный префикс " +
                    $"(netsh http add urlacl url=http://+:{port}/ user=Everyone).");
                _listener = null;
                return;
            }

            _running = true;
            Task.Run(Loop);
            Log?.Invoke(this, $"HTTP-сервер запущен на порту {port}");
        }

        public void Stop()
        {
            _running = false;
            try { _listener?.Stop(); } catch { }
            try { _listener?.Close(); } catch { }
            _listener = null;
            Log?.Invoke(this, "HTTP-сервер остановлен.");
        }

        private async Task Loop()
        {
            while (_running && _listener != null && _listener.IsListening)
            {
                HttpListenerContext ctx = null;
                try
                {
                    ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch
                {
                    if (!_running) return;
                    continue;
                }

                try
                {
                    var path = ctx.Request.Url?.AbsolutePath ?? "/";

                    if (path.Equals("/state", StringComparison.OrdinalIgnoreCase))
                    {
                        var json = "{\"state\":\"" + (StateHtml ?? "").Replace("\"", "\\\"") + "\"}";
                        var bytesJ = Encoding.UTF8.GetBytes(json);
                        ctx.Response.ContentType = "application/json; charset=utf-8";
                        ctx.Response.ContentLength64 = bytesJ.Length;
                        await ctx.Response.OutputStream.WriteAsync(bytesJ, 0, bytesJ.Length).ConfigureAwait(false);
                    }
                    else
                    {
                        var html = AnswerPageRenderer.Render(this);
                        var bytes = Encoding.UTF8.GetBytes(html);
                        ctx.Response.ContentType = "text/html; charset=utf-8";
                        ctx.Response.ContentLength64 = bytes.Length;
                        await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    }

                    ctx.Response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    Log?.Invoke(this, "HTTP error: " + ex.Message);
                    try { ctx.Response.StatusCode = 500; ctx.Response.Close(); } catch { }
                }
            }
        }

        public void Dispose() => Stop();
    }
}