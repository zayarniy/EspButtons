namespace GuessMelody.AnswerServer
{
    public static class AnswerPageRenderer
    {
        public static string Render(AnswerHttpServer srv)
        {
            var title = Escape(srv.Title);
            var state = srv.StateHtml ?? "";
            var placeholder = Escape(srv.PlaceholderText ?? "");

            return $@"<!DOCTYPE html>
<html lang=""ru"">
<head>
<meta charset=""utf-8"">
<title>{title}</title>
<style>
  body {{ font-family: Segoe UI, Arial, sans-serif; background:#1e1e2e; color:#eee; margin:0; padding:24px; }}
  h1 {{ margin:0 0 12px; }}
  .card {{ background:#2a2a3d; padding:20px; border-radius:8px; margin-bottom:16px; }}
  .muted {{ color:#8a8aa0; }}
</style>
</head>
<body>
  <h1>{title}</h1>
  <div class=""card"">
    <p>Сервер работает. Порт: <b>{srv.Port}</b></p>
    <p class=""muted"">{placeholder}</p>
  </div>
  <div class=""card"" id=""state"">
    {state}
  </div>
</body>
</html>";
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;");
        }
    }
}