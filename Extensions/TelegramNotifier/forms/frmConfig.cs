using ServiceManager.Extensions.TelegramNotifier.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace ServiceManager.Extensions.TelegramNotifier.forms
{
    public partial class frmConfig : Form
    {

        Telegram.Bot.TelegramBotClient TelegramBotClient { get; set; }

        CancellationTokenSource __cancellationTokenSource;

        public frmConfig()
        {
            InitializeComponent();
        }

        private void bnSearch_Click(object sender, EventArgs e)
        {
            if (txtAPIKey.Text == "")
                return;

            MessageBox.Show("This will start the Bot for 10 seconds and saves the chat id from the first message arrived.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            tmSearch.Enabled = true;
            bnSearch.Enabled = false;

            TelegramBotClient = new TelegramBotClient(txtAPIKey.Text);

            __cancellationTokenSource = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            TelegramBotClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, __cancellationTokenSource.Token);


        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message ?? update.EditedMessage ?? update.ChannelPost;

            if (message != null)
            {
                BeginInvoke(new Action(() =>
                {
                    Thread.Sleep(500);
                    tmSearch_Tick(null, null);

                    txtChatId.Text = message.Chat.Id.ToString();

                    MessageBox.Show($"Chat Id {message.Chat.Id} found !");

                }));
            }

            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void tmSearch_Tick(object sender, EventArgs e)
        {
            tmSearch.Enabled = false;
            bnSearch.Enabled = true;

            if (TelegramBotClient != null)
            {
                __cancellationTokenSource.Cancel();
                TelegramBotClient = null;
            }



        }

        private void frmConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TelegramBotClient != null)
            {
                __cancellationTokenSource.Cancel();
                TelegramBotClient = null;
            }


        }

        private void frmConfig_Load(object sender, EventArgs e)
        {

            var __config = Config.Config.load(Config.Config.FilePath);

            txtAPIKey.Text = __config.APIKey ?? "";
            txtChatId.Text = __config.ChatId.ToString();


        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bnSave_Click(object sender, EventArgs e)
        {
            var __config = Config.Config.load(Config.Config.FilePath);

            __config.APIKey = txtAPIKey.Text;

            long chatId = 0;
            if (long.TryParse(txtChatId.Text, out chatId))
            {
                __config.ChatId = chatId;
            }

            __config.save(Config.Config.FilePath);

            this.Close();
        }
    }
}
