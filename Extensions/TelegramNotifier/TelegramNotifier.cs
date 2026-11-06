using ServiceManager.Extensions.TelegramNotifier.Config;
using ServiceManager.Extensions.TelegramNotifier.forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ServiceManager.Extensions.TelegramNotifier
{
    public class TelegramNotifier : Interfaces.INotificationExtension
    {


        public Guid Id => Guid.Parse("45f57793-6312-4cc2-8e9f-3eb19383f5d6");

        public string Name => "Telegram Bot Notifier";

        public string Description => "Sends you notifications about your services via a Telegram Bot.";


        private Config.Config __config;

        private static TelegramBotClient __client;

        private static string __clientToken;

        /// <summary>
        /// The host creates a new instance of this extension for every notification, so the client
        /// (and with it its HttpClient and its connection pool) has to live beyond one instance.
        /// </summary>
        private static TelegramBotClient GetClient(string token)
        {
            if (__client == null || __clientToken != token)
            {
                __client = new TelegramBotClient(token);
                __clientToken = token;
            }

            return __client;
        }


        public TelegramNotifier()
        {
            __config = Config.Config.load(Config.Config.FilePath);
            if (__config == null || __config.APIKey == null)
            {
            }
            __config.save(Config.Config.FilePath);
        }


        public async Task Send(string message)
        {
            if (__config.APIKey == null || __config.APIKey.Trim() == "")
                return;

            if (__config.ChatId == 0)
                return;

            try
            {
                await GetClient(__config.APIKey).SendTextMessageAsync(__config.ChatId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                return;

            }
            catch
            {

            }

            return;
        }

        public void Configure()
        {
            frmConfig frm = new frmConfig();

            frm.ShowDialog();
        }
    }
}
