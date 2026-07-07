namespace FocusMind.API.Middleware;

// HU-20: cabeceras de seguridad HTTP equivalentes a "Helmet" (Express/Node). No existe un
// paquete oficial de Microsoft con este propósito, así que se agregan a mano — cada una mitiga
// una clase de ataque distinta:
//   - X-Content-Type-Options: evita que el navegador adivine el Content-Type (MIME-sniffing).
//   - X-Frame-Options: bloquea que la API se embeba en un <iframe> (clickjacking); no aplica a
//     una API JSON pura, pero cubre Swagger UI si se expusiera en el mismo origen.
//   - Referrer-Policy: no se filtra la URL completa como Referer hacia otros orígenes.
//   - Permissions-Policy: deshabilita APIs de navegador que esta API nunca usa.
//   - Content-Security-Policy: "default-src 'none'" porque este origen solo responde JSON, no
//     sirve HTML/JS propio (se excluye /swagger para no romper la UI en Development).
// Ninguna de estas reemplaza HTTPS: Strict-Transport-Security solo se agrega cuando la request
// ya llegó por HTTPS (en producción esto lo hará API Gateway/CloudFront, ver HU-22).
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";

        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        }

        if (context.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        await next(context);
    }
}
