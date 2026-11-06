using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ServiceManager.Extensions.TelegramNotifier.Polling
{
    /// <summary>
    /// Update handler that completes as soon as the first message arrived. It is handed to
    /// Telegram.Bot's receiver, which calls it on a background thread, so it never touches the form.
    /// </summary>
    public class ChatIdCollector : IUpdateHandler
    {
        private readonly TaskCompletionSource<long> __result = new TaskCompletionSource<long>();

        public Task<long> Result => __result.Task;

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message ?? update.EditedMessage ?? update.ChannelPost;

            if (message != null)
                __result.TrySetResult(message.Chat.Id);

            return Task.CompletedTask;
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // A token that Telegram rejects will not heal itself, so give up on it. Everything else
            // (network hiccups, 5xx, timeouts) Telegram.Bot retries by itself.
            var apiException = exception as ApiRequestException;

            if (apiException != null && (apiException.ErrorCode == 401 || apiException.ErrorCode == 404))
                __result.TrySetException(exception);

            return Task.CompletedTask;
        }
    }
}
