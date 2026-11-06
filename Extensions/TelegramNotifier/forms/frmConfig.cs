using ServiceManager.Extensions.TelegramNotifier.Config;
using ServiceManager.Extensions.TelegramNotifier.Polling;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace ServiceManager.Extensions.TelegramNotifier.forms
{
    public partial class frmConfig : Form
    {

        private static readonly TimeSpan SearchTimeout = TimeSpan.FromSeconds(10);

        TelegramBotClient __client;

        string __clientToken;

        CancellationTokenSource __cancellationTokenSource;

        bool __closing;

        public frmConfig()
        {
            InitializeComponent();
        }

        /// <summary>
        /// A client owns its HttpClient, so keep one per API key instead of creating one per click.
        /// </summary>
        private TelegramBotClient GetClient(string token)
        {
            if (__client == null || __clientToken != token)
            {
                __client = new TelegramBotClient(token);
                __clientToken = token;
            }

            return __client;
        }

        private async void bnSearch_Click(object sender, EventArgs e)
        {
            var token = txtAPIKey.Text.Trim();

            if (token == "")
                return;

            MessageBox.Show("This will start the Bot for 10 seconds and saves the chat id of the first message sent to it.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var collector = new ChatIdCollector();

            __cancellationTokenSource = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.EditedMessage, UpdateType.ChannelPost },
                ThrowPendingUpdates = true
            };

            bnSearch.Enabled = false;
            bnSearch.Text = "Searching...";

            GetClient(token).StartReceiving(collector, receiverOptions, __cancellationTokenSource.Token);

            var timeout = Task.Delay(SearchTimeout, __cancellationTokenSource.Token);

            var finished = await Task.WhenAny(collector.Result, timeout);

            StopSearch();

            if (__closing || IsDisposed)
                return;

            if (finished == collector.Result)
            {
                try
                {
                    txtChatId.Text = (await collector.Result).ToString();

                    MessageBox.Show($"Chat Id {txtChatId.Text} found !");
                }
                catch (ApiRequestException ex)
                {
                    MessageBox.Show($"Telegram rejected the API key: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (!timeout.IsCanceled)
            {
                MessageBox.Show("No message arrived within 10 seconds.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void StopSearch()
        {
            if (__cancellationTokenSource != null)
            {
                __cancellationTokenSource.Cancel();
                __cancellationTokenSource = null;
            }

            if (!IsDisposed)
            {
                bnSearch.Text = "Start searching";
                bnSearch.Enabled = true;
            }
        }

        private void frmConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            __closing = true;

            StopSearch();
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

        private async void bnTestMessage_Click(object sender, EventArgs e)
        {
            long chatId = 0;
            if (!long.TryParse(txtChatId.Text.Trim(), out chatId))
            {
                return;
            }

            bnTestMessage.Enabled = false;

            try
            {
                await GetClient(txtAPIKey.Text.Trim()).SendTextMessageAsync(chatId, "Testmessage from ServiceManager");

                MessageBox.Show($"Testmessage sent!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during sending. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed)
                    bnTestMessage.Enabled = true;
            }
        }
    }
}
