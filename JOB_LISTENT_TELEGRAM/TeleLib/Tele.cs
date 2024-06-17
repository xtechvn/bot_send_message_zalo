using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace JOB_LISTENT_TELEGRAM.TeleLib
{
    public class Tele
    {
        private readonly string telegram_token;
        private readonly string telegram_group;
        public Tele(string _telegram_token,string _telegram_group)
        {
            telegram_token = _telegram_token;
            telegram_group = _telegram_group;
        }

        public void TelegramBotReply()
        {
            TelegramBotClient bot = new TelegramBotClient(telegram_group);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[]
                {
                        Telegram.Bot.Types.Enums.UpdateType.Message,
                        Telegram.Bot.Types.Enums.UpdateType.EditedMessage
                 }
            };
            bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);
        }

        private async Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            return;
        }

        private async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken arg3)
        {
            if (update.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                var text = update.Message.Text;
                var username = update.Message.From.Id;
                var id = update.Message.From.Username;
                await bot.SendTextMessageAsync(telegram_group, "ID: " + id + " . Username: " + username + " đã gửi message: " + text);
            }
        }
    }
}
