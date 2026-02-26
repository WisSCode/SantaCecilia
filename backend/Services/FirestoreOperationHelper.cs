using Grpc.Core;
using System.Net.Http;
using System.Net.Sockets;

namespace backend.Services;

public sealed class FirestoreUnavailableException : Exception
{
    public FirestoreUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public static class FirestoreOperationHelper
{
    private static readonly TimeSpan[] RetryDelays =
    {
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(750)
    };

    public static Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, cancellationToken);
    }

    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= RetryDelays.Length + 1; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsTransient(ex) && attempt <= RetryDelays.Length)
            {
                lastException = ex;
                await Task.Delay(RetryDelays[attempt - 1], cancellationToken);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                throw new FirestoreUnavailableException("Firestore is temporarily unavailable.", ex);
            }
        }

        throw new FirestoreUnavailableException("Firestore is temporarily unavailable.", lastException);
    }

    private static bool IsTransient(Exception exception)
    {
        return exception switch
        {
            FirestoreUnavailableException => false,
            RpcException rpcException => rpcException.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded,
            HttpRequestException => true,
            IOException => true,
            SocketException => true,
            _ => exception.InnerException is not null && IsTransient(exception.InnerException)
        };
    }
}