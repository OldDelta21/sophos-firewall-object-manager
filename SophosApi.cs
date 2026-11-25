using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;

namespace SOPHOS_Host___Addresses_Manager;

internal sealed class FirewallTlsException(string message, Exception inner) : InvalidOperationException(message, inner);

internal sealed record PresentedCertificate(string Subject, string Issuer, DateTime Expires, string Fingerprint, string Errors);

internal sealed class SophosApi(Uri endpoint, string username, string password, string apiVersion,
    HttpClient? client = null, string? certificateFingerprint = null) : IDisposable
{
    private static readonly HttpClient Client = new(new HttpClientHandler { AllowAutoRedirect = false });
    private readonly HttpClient? pinnedClient = certificateFingerprint is { Length: > 0 }
        ? CreatePinnedClient(endpoint, NormalizeFingerprint(certificateFingerprint)) : null;
    private const int PayloadLimit = 16 * 1024 * 1024;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(2);

    public void Dispose() => pinnedClient?.Dispose();

    public static string NormalizeFingerprint(string? value)
    {
        var normalized = string.Concat((value ?? "").Where(c => c != ':' && !char.IsWhiteSpace(c)));
        if (!string.IsNullOrWhiteSpace(value) && (normalized.Length != 64 || !normalized.All(Uri.IsHexDigit)))
            throw new FormatException("Enter a verified SHA-256 certificate fingerprint (64 hex digits), or leave it blank.");
        return normalized.ToUpperInvariant();
    }

    private static HttpClient CreatePinnedClient(Uri endpoint, string fingerprint)
    {
        var expected = Convert.FromHexString(fingerprint);
        return new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) =>
                request.RequestUri == endpoint && MatchesTrustedFingerprint(expected, certificate, chain, errors)
        });
    }

    internal static bool MatchesTrustedFingerprint(byte[] expected, X509Certificate2? certificate,
        X509Chain? chain, SslPolicyErrors errors)
    {
        if (certificate == null || errors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable) ||
            !CryptographicOperations.FixedTimeEquals(expected, certificate.GetCertHash(HashAlgorithmName.SHA256)))
            return false;
        var now = DateTime.UtcNow;
        if (now < certificate.NotBefore.ToUniversalTime() || now > certificate.NotAfter.ToUniversalTime()) return false;
        const X509ChainStatusFlags trustErrors = X509ChainStatusFlags.UntrustedRoot | X509ChainStatusFlags.PartialChain;
        return !errors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors) ||
               chain != null && chain.ChainStatus.All(status => (status.Status & ~trustErrors) == 0);
    }

    public static async Task<PresentedCertificate?> InspectCertificateAsync(Uri endpoint, CancellationToken cancellation)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        PresentedCertificate? presented = null;
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        handler.ServerCertificateCustomValidationCallback = (_, certificate, chain, errors) =>
        {
            if (certificate is X509Certificate2 leaf)
            {
                var hash = leaf.GetCertHash(HashAlgorithmName.SHA256);
                if (MatchesTrustedFingerprint(hash, leaf, chain, errors))
                    presented = new PresentedCertificate(leaf.Subject, leaf.Issuer, leaf.NotAfter,
                        Convert.ToHexString(hash), errors.ToString());
            }
            return false;
        };
        using var client = new HttpClient(handler);
        try
        {
            using var response = await client.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        }
        catch (HttpRequestException ex) when (ex.HttpRequestError == HttpRequestError.SecureConnectionError) { }
        return presented;
    }

    public static Uri Endpoint(string value)
    {
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
            uri.UserInfo.Length != 0 || uri.AbsolutePath != "/" || uri.Query.Length != 0 || uri.Fragment.Length != 0)
            throw new FormatException("Enter the HTTPS admin origin, for example https://firewall.example:4444");
        return new Uri(uri, "/webconsole/APIController");
    }

    public async Task<XElement> SendAsync(XElement? operation, CancellationToken cancellation)
    {
        var requestXml = new XElement("Request",
            apiVersion.Length == 0 ? null : new XAttribute("APIVersion", apiVersion), new XElement("Login",
            new XElement("Username", username), new XElement("Password", password)), operation);
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(requestXml.ToString(SaveOptions.DisableFormatting)), "reqxml");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
        var root = Parse(await DownloadTextAsync(client ?? pinnedClient ?? Client, request, cancellation, pinnedClient != null));
        if (root.Name != "Response") throw new FormatException("The firewall did not return an XML API Response.");
        var login = root.Element("Login")?.Element("status") ?? root.Element("Login")?.Element("Status");
        var loginStatus = login?.Value.Trim().TrimEnd('.');
        if (loginStatus != null && !loginStatus.Equals("Authentication Successful", StringComparison.OrdinalIgnoreCase) &&
            !loginStatus.Equals("Authetication Successful", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Firewall authentication failed. Check credentials and API access.");
        foreach (var status in root.Elements("Status").Concat(root.Elements().Elements("Status")))
        {
            if ((string?)status.Attribute("code") is { } code && code is not ("200" or "216") &&
                !(code == "526" && operation?.Name == "Get"))
            {
                var details = code switch
                {
                    "522" or "523" or "524" or "525" => "The appliance reached an entity capacity limit.",
                    "502" => "An entity with this name already exists.",
                    "521" or "530" => "The operation is incompatible with the specified XML API version.",
                    _ => "See the release's API response-code reference."
                };
                var applianceMessage = string.Join(" ", status.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
                if (applianceMessage.Length > 300) applianceMessage = applianceMessage[..300] + "…";
                var explanation = applianceMessage.Length == 0 ? details : $"{details} Firewall response: {applianceMessage}";
                throw new InvalidOperationException($"Sophos API status {code}. {explanation}");
            }
            if (status.Parent == root && status.Attribute("code") == null)
                throw new InvalidOperationException("The firewall returned an unrecognized API status.");
        }
        if (operation == null && login == null)
            throw new InvalidOperationException("The firewall did not confirm authentication.");
        return root;
    }

    public async Task SetAsync(string operation, XElement entity, CancellationToken cancellation)
    {
        var response = await SendAsync(new XElement("Set", new XAttribute("operation", operation), entity), cancellation);
        if (!response.Elements(entity.Name).Elements("Status").Concat(response.Elements("Status"))
                .Any(x => (string?)x.Attribute("code") is "200" or "216"))
            throw new InvalidOperationException("The firewall did not confirm the write. Check the appliance before retrying.");
    }

    public static XElement Parse(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = PayloadLimit
        });
        return XElement.Load(reader);
    }

    public static async Task<string> LoadAsync(string path, bool remote, CancellationToken cancellation)
    {
        if (!remote)
        {
            await using var file = File.OpenRead(path);
            return await ReadTextAsync(file, cancellation);
        }
        if (!Uri.TryCreate(path, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.UserInfo.Length != 0)
            throw new FormatException("Enter a direct HTTPS URL without embedded credentials.");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        return await DownloadTextAsync(Client, request, cancellation);
    }

    private static async Task<string> DownloadTextAsync(HttpClient http, HttpRequestMessage request,
        CancellationToken cancellation, bool fingerprintRequired = false)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(RequestTimeout);
        try
        {
            using var response = await SendHttpAsync(http, request, deadline.Token, fingerprintRequired);
            response.EnsureSuccessStatusCode();
            return await ReadTextAsync(response.Content, deadline.Token);
        }
        catch (OperationCanceledException ex) when (!cancellation.IsCancellationRequested)
        {
            throw new TimeoutException($"The request to {request.RequestUri!.Authority} timed out. " +
                "Check any unconfirmed firewall write before retrying.", ex);
        }
    }

    private static async Task<HttpResponseMessage> SendHttpAsync(HttpClient http, HttpRequestMessage request,
        CancellationToken cancellation, bool fingerprintRequired = false)
    {
        try
        {
            return await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation);
        }
        catch (HttpRequestException ex) when (ex.HttpRequestError == HttpRequestError.SecureConnectionError ||
                                             ex.InnerException is AuthenticationException)
        {
            var details = ex.GetBaseException().Message;
            var guidance = fingerprintRequired
                ? "Verify the configured SHA-256 fingerprint against the current admin certificate and check its validity."
                : details.Contains("RemoteCertificateNameMismatch", StringComparison.Ordinal)
                ? "Use the HTTPS hostname on the server certificate and make sure Windows trusts its issuing CA."
                : details.Contains("RemoteCertificateChainErrors", StringComparison.Ordinal)
                    ? "Check the server certificate's issuer, full chain, and expiration. Windows must trust the issuing CA."
                    : "Check the server's TLS configuration and certificate.";
            throw new FirewallTlsException(
                $"TLS failed for {request.RequestUri!.Authority}. {guidance}{Environment.NewLine}{details}", ex);
        }
    }

    private static async Task<string> ReadTextAsync(HttpContent content, CancellationToken cancellation)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellation);
        return await ReadTextAsync(stream, cancellation);
    }

    private static async Task<string> ReadTextAsync(Stream stream, CancellationToken cancellation)
    {
        using var reader = new StreamReader(stream);
        var result = new System.Text.StringBuilder();
        var buffer = new char[8192];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory(), cancellation)) != 0)
        {
            if (result.Length + count > PayloadLimit)
                throw new InvalidOperationException("Input exceeds the 16-million-character limit.");
            result.Append(buffer, 0, count);
        }
        return result.ToString();
    }
}

