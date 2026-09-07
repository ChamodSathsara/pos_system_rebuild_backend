using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PosApi.Configuration;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

[ApiController]
[Route("api/qz")]
[Authorize]
[EnableCors("QzCorsPolicy")]
public class QzController : ControllerBase
{
    private readonly IQzSigningService _signingService;
    private readonly QzSigningSettings _settings;
    private readonly ILogger<QzController> _logger;

    public QzController(
        IQzSigningService signingService,
        IOptions<QzSigningSettings> settings,
        ILogger<QzController> logger)
    {
        _signingService = signingService;
        _settings = settings.Value;
        _logger = logger;
    }

    [HttpGet("certificate")]
    [Produces("text/plain")]
    public async Task<IActionResult> Certificate(CancellationToken cancellationToken)
    {
        try
        {
            var certificate = await _signingService.GetCertificateAsync(cancellationToken);
            Response.Headers.CacheControl = "no-store";
            return Content(certificate, "text/plain", Encoding.UTF8);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            _logger.LogError(exception, "Unable to read the configured QZ certificate.");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "QZ certificate is unavailable.",
                detail: "The server could not load the configured QZ certificate.");
        }
    }

    [HttpPost("sign")]
    [Consumes("text/plain")]
    [Produces("text/plain")]
    [RequestSizeLimit(100 * 1024)]
    public async Task<IActionResult> Sign(CancellationToken cancellationToken)
    {
        if (Request.ContentType is null
            || !Request.ContentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Content-Type must be text/plain.");
        }

        if (Request.ContentLength > _settings.MaxPayloadBytes)
        {
            return BadRequest($"Signing payload cannot exceed {_settings.MaxPayloadBytes} bytes.");
        }

        byte[] payload;
        await using (var buffer = new MemoryStream())
        {
            var chunk = new byte[8192];
            int bytesRead;
            while ((bytesRead = await Request.Body.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + bytesRead > _settings.MaxPayloadBytes)
                {
                    return BadRequest($"Signing payload cannot exceed {_settings.MaxPayloadBytes} bytes.");
                }

                await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
            }

            payload = buffer.ToArray();
        }

        if (payload.Length == 0)
        {
            return BadRequest("Signing payload is required.");
        }

        try
        {
            _ = new UTF8Encoding(false, true).GetString(payload);
        }
        catch (DecoderFallbackException)
        {
            return BadRequest("Signing payload must be valid UTF-8 text.");
        }

        try
        {
            var signature = await _signingService.SignAsync(payload, cancellationToken);
            Response.Headers.CacheControl = "no-store";
            return Content(signature, "text/plain", Encoding.UTF8);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or CryptographicException)
        {
            _logger.LogError(exception, "Unable to sign a QZ payload using the configured private key.");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "QZ signing is unavailable.",
                detail: "The server could not sign the request payload.");
        }
    }
}
