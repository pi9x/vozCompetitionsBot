using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using vozCompetitionsLibrary;
using vozCompetitionsLibrary.DataAccess;
using vozCompetitionsLibrary.Model;

namespace vozCompetitions
{
    class Program
    {
        static ITelegramBotClient vozCompetitionsBot;

        static void Main()
        {
            vozCompetitionsBot = new TelegramBotClient(ConfigGetter.API());

            vozCompetitionsBot.OnMessage += Bot_OnMessage;
            vozCompetitionsBot.OnMessageEdited += Bot_OnMessage;
            vozCompetitionsBot.OnCallbackQuery += Bot_OnCallbackQuery;

            vozCompetitionsBot.StartReceiving();

            var me = vozCompetitionsBot.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            Thread.Sleep(int.MaxValue);
            vozCompetitionsBot.StopReceiving();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs messageEvent)
        {
            var message = messageEvent.Message;
            if (message == null) return;

            // ADD NEW COMPETITION - My Telegram ID: 650818972
            // Command format: /newcompetition #hashtag Competition name
            if (message.Text != null)
                if (message.Text.Split(' ', 3)[0].Trim() == "/newcompetition" && message.From.Id == 650818972)
                    try
                    {
                        Competition competition = new Competition()
                        {
                            Hashtag = message.Text.Split(' ', 3)[1].Trim(),
                            Name = message.Text.Split(' ', 3)[2].Trim(),
                            Status = "Opening"
                        };

                        AccessCompetition.Create(competition);

                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            $"Cuộc thi: {competition.Name}\nHashtag: {competition.Hashtag}\n\nBẮT ĐẦU!!!",
                            replyToMessageId: message.MessageId
                        );
                    }
                    catch (NullReferenceException)
                    {
                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            "Lỗi rồi bạn ey!",
                            replyToMessageId: message.MessageId
                        );
                    }


            // LIST ALL COMPETITIONS
            // Command: /listcompetition
            if (message.Text != null)
                if (message.Text == "/listcompetition")
                {
                    string listCompetitions = "DANH SÁCH CÁC CUỘC THI\n\n";
                    int count = 0;
                    foreach (Competition competition in AccessCompetition.Get())
                    {
                        count++;
                        listCompetitions += $"[{count}] {competition.Hashtag} - {competition.Name} - {competition.Status}\n\n";
                    }
                    if (count == 0)
                        listCompetitions += "Chưa có cuộc thi nào.";

                    await vozCompetitionsBot.SendTextMessageAsync(
                        message.Chat,
                        listCompetitions,
                        replyToMessageId: message.MessageId
                    );
                }


            // CLOSE COMPETITION - - My Telegram ID: 650818972
            // Command format: /close #hashtag
            if (message.Text != null)
                if (message.Text.Split(' ')[0].Trim() == "/close" && message.From.Id == 650818972)
                    try
                    {
                        bool found = AccessCompetition.Close(message.Text.Split(' ')[1].Trim());
                        if (found)
                        {
                            await vozCompetitionsBot.SendTextMessageAsync(
                                message.Chat,
                                "Kết thúc cuộc thi!",
                                replyToMessageId: message.MessageId
                            );
                        }
                        else
                        {
                            await vozCompetitionsBot.SendTextMessageAsync(
                                message.Chat,
                                "Cuộc thi này không tồn tại!",
                                replyToMessageId: message.MessageId
                            );
                        }
                    }
                    catch (IndexOutOfRangeException) { }


            // ADD NEW SUBMISSION
            // Photo with #hashtag
            if (message.Caption != null)
            {
                string hashtag = message.Caption.Split(' ')[0];
                if (AccessCompetition.Get().Exists(x => x.Hashtag == hashtag))
                    if (AccessCompetition.GetStatus(hashtag) == "Opening")
                    {
                        Submission submission = new Submission()
                        {
                            CompetitionHashtag = hashtag,
                            UserId = message.From.Id,
                            UserInfo = $"{message.From.FirstName} {message.From.LastName} @{message.From.Username}",
                            MessageId = message.MessageId,
                            Point = 0
                        };

                        AccessSubmission.Add(submission);

                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            $"Bình chọn cho {message.From.FirstName} {message.From.LastName} @{message.From.Username}",
                            replyToMessageId: message.MessageId,
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Bình chọn - 0", hashtag))
                        );
                    }
                    else
                    {
                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            "Cuộc thi này đã kết thúc, chậm chân ròi comrade ey.",
                            replyToMessageId: message.MessageId
                        );
                    }
            }


            // SHOW RESULT
            // Command format: /result #hashtag
            if (message.Text != null)
                if (message.Text.Split(' ')[0] == "/result")
                    try
                    {
                        string hashtag = message.Text.Split(' ')[1].Trim();
                        string listSubmissions = $"KẾT QUẢ HIỆN TẠI - {hashtag}\n\n";
                        foreach (Submission submission in AccessSubmission.Get())
                            if (submission.CompetitionHashtag == hashtag)
                                listSubmissions += $"*Điểm: {submission.Point}* - {submission.UserInfo} - [Xem hình](https://t.me/{message.Chat.Username}/{submission.MessageId})\n";

                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            listSubmissions,
                            ParseMode.Markdown,
                            disableWebPagePreview: true,
                            replyToMessageId: message.MessageId
                        );
                    }
                    catch (IndexOutOfRangeException) { }
        }

        static async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs callbackQueryEvent)
        {
            // VOTE
            var callback = callbackQueryEvent.CallbackQuery;
            Vote vote = new Vote()
            {
                UserId = callback.From.Id,
                MessageId = callback.Message.ReplyToMessage.MessageId
            };

            if (AccessCompetition.GetStatus(callback.Data) == "Opening")
                if (AccessVote.Exists(vote))
                {
                    await vozCompetitionsBot.AnswerCallbackQueryAsync(callback.Id, "Bạn đã bình chọn rồi!");
                }
                else
                {
                    AccessVote.Add(vote);
                    AccessSubmission.ChangeVote(vote.MessageId, true);

                    int point = 0;
                    foreach (Submission submission in AccessSubmission.Get())
                        if (submission.MessageId == vote.MessageId)
                        {
                            point = submission.Point;
                            break;
                        }

                    await vozCompetitionsBot.AnswerCallbackQueryAsync(callback.Id, "Bình chọn thành công!");

                    await vozCompetitionsBot.EditMessageTextAsync(
                        callback.Message.Chat,
                        callback.Message.MessageId,
                        $"Bình chọn cho {callback.Message.ReplyToMessage.From.FirstName} {callback.Message.ReplyToMessage.From.LastName} @{callback.Message.ReplyToMessage.From.Username}",
                        replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData($"Bình chọn - {point}", callback.Data))
                    );
                }
            else
            {
                await vozCompetitionsBot.AnswerCallbackQueryAsync(callback.Id, "Cuộc thi này đã kết thúc!");
            }
        }
    }
}
