using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Net;

namespace Helper.Helpers;

public static class Utils
{
    public static Expression<Func<T, bool>> CombineExpressions<T>(IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        if (!expressions.Any()) return x => true;

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = expressions
            .Select(expr => ReplaceParameter(expr.Body, expr.Parameters[0], parameter))
            .Aggregate(Expression.AndAlso);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression ReplaceParameter(Expression body, ParameterExpression toReplace, ParameterExpression replacement)
        => new ParameterReplacer(toReplace, replacement).Visit(body);

    private class ParameterReplacer(ParameterExpression toReplace, ParameterExpression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == toReplace ? replacement : base.VisitParameter(node);
    }
    
    public static string GetIP(HttpContext context)
    {
        List<string?> ips = [context.Connection.RemoteIpAddress?.ToString()];

        string[] headersToCheck =
        [
            "X-Forwarded-For",
            "X-Real-IP",
            "CF-Connecting-IP", // Cloudflare
            "HTTP_X_FORWARDED_FOR"
        ];

        foreach (var header in headersToCheck)
        {
            if (context.Request.Headers.TryGetValue(header, out var value))
                ips.Add(value.ToString());
        }

        var realIpAddress = GetPublicIPAddress(ips);

        return realIpAddress ?? "Unknown";
    }

    public static string? GetPublicIPAddress(List<string?> ipAddresses)
    {
        foreach (var ipAddress in ipAddresses)
        {
            if (!string.IsNullOrEmpty(ipAddress) && IPAddress.TryParse(ipAddress, out var address))
            {
                if (IsPublicIPAddress(address))
                {
                    return ipAddress;
                }
            }
        }
        return null;
    }
    private static bool IsPublicIPAddress(IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            return !(bytes[0] == 10 || bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31 || bytes[0] == 192 && bytes[1] == 168);
        }
        else if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return !IPAddress.IsLoopback(ipAddress) && !ipAddress.IsIPv6LinkLocal && !ipAddress.IsIPv6SiteLocal;
        }
        return false;
    }
}
