using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using gymer.Models;

namespace gymer
{
    public class BotHandlers
    {
        private readonly TelegramBotClient _botClient;
        private readonly Database _database;
        private readonly Dictionary<long, List<Exercise>> _userExercises = new Dictionary<long, List<Exercise>>();
        private readonly Dictionary<long, string> _userStates = new Dictionary<long, string>();

        public BotHandlers(TelegramBotClient botClient)
        {
            _botClient = botClient;
            _database = new Database();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {
                var message = update.Message;
                var chatId = message.Chat.Id;

                // Выводим ввод пользователя в консоль для отладки
                Console.WriteLine($"User input: {message.Text}");

                if (message.Text == "Назад")
                {
                    await ShowMainMenu(chatId);
                    return;
                }

                if (message.Text == "/start")
                {
                    await ShowMainMenu(chatId);
                }
                else if (message.Text == "Добавить упражнение")
                {
                    if (!_userExercises.ContainsKey(chatId))
                    {
                        _userExercises[chatId] = new List<Exercise>();
                    }
                    _userStates[chatId] = "AddingExercise";
                    await _botClient.SendTextMessageAsync(chatId, "Введите упражнение в формате: название, вес (кг), количество повторений", replyMarkup: GetBackButton());
                }
                else if (message.Text == "Прогресс")
                {
                    _userStates[chatId] = "ViewingProgress";
                    var exercises = _database.GetExercises(chatId);
                    if (exercises.Count == 0)
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Нет данных о прогрессе.", replyMarkup: GetBackButton());
                    }
                    else
                    {
                        var response = "Ваш прогресс:\n";
                        foreach (var exercise in exercises)
                        {
                            response += $"{exercise.Date.ToShortDateString()}: {exercise.Name}, {exercise.Weight} кг, {exercise.Reps} повторений\n";
                        }

                        await _botClient.SendTextMessageAsync(chatId, response, replyMarkup: GetBackButton());
                    }
                }
                else if (message.Text == "Напоминание")
                {
                    _userStates[chatId] = "ManagingReminder";
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Добавить напоминание"),
                        new KeyboardButton("Увидеть напоминание"),
                        new KeyboardButton("Назад")
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await _botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: keyboard);
                }
                else if (message.Text == "Добавить напоминание")
                {
                    _userStates[chatId] = "AddingReminder";
                    await _botClient.SendTextMessageAsync(chatId, "Введите напоминание в формате: текст, дата (гггг.мм.дд)", replyMarkup: GetBackButton());
                }
                else if (message.Text == "Увидеть напоминание")
                {
                    var reminders = _database.GetReminders(chatId);
                    if (reminders.Count == 0)
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Нет напоминаний.", replyMarkup: GetBackButton());
                    }
                    else
                    {
                        var response = "Ваши напоминания:\n";
                        foreach (var reminder in reminders)
                        {
                            response += $"{reminder.ReminderDate.ToShortDateString()}: {reminder.Message}\n";
                        }

                        await _botClient.SendTextMessageAsync(chatId, response, replyMarkup: GetBackButton());
                    }
                }
                else if (_userStates.ContainsKey(chatId) && _userStates[chatId] == "AddingExercise")
                {
                    if (message.Text == "Добавить ещё упражнение")
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Введите упражнение в формате: название, вес (кг), количество повторений", replyMarkup: GetBackButton());
                    }
                    else
                    {
                        var exerciseDetails = message.Text.Split(", ");
                        if (exerciseDetails.Length == 3 && int.TryParse(exerciseDetails[1], out int weight) && int.TryParse(exerciseDetails[2], out int reps))
                        {
                            var exercise = new Exercise
                            {
                                UserId = chatId,
                                Name = exerciseDetails[0],
                                Weight = weight,
                                Reps = reps,
                                Date = DateTime.Now
                            };

                            _userExercises[chatId].Add(exercise);
                            _database.AddExercise(exercise); // Сохраняем упражнение в базу данных сразу после добавления

                            var keyboard = new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton("Добавить ещё упражнение"),
                                new KeyboardButton("Нет")
                            })
                            {
                                ResizeKeyboard = true
                            };

                            await _botClient.SendTextMessageAsync(chatId, "Упражнение добавлено! Хотите добавить еще?", replyMarkup: keyboard);
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(chatId, "Неверный формат ввода. Пожалуйста, введите упражнение в правильном формате.", replyMarkup: GetBackButton());
                        }
                    }
                }
                else if (message.Text == "Нет")
                {
                    await _botClient.SendTextMessageAsync(chatId, "Все упражнения сохранены.", replyMarkup: GetMainMenuKeyboard());
                }
                else if (_userStates.ContainsKey(chatId) && _userStates[chatId] == "AddingReminder")
                {
                    var reminderDetails = message.Text.Split(", ");
                    if (reminderDetails.Length == 2 && DateTime.TryParseExact(reminderDetails[1], "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out DateTime reminderDate))
                    {
                        var reminder = new Reminder
                        {
                            UserId = chatId,
                            Message = reminderDetails[0],
                            ReminderDate = reminderDate
                        };

                        _database.AddReminder(reminder);
                        await _botClient.SendTextMessageAsync(chatId, "Напоминание добавлено!", replyMarkup: GetBackButton());
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Неверный формат даты. Пожалуйста, введите дату в формате гггг.мм.дд.", replyMarkup: GetBackButton());
                    }
                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId, "Неверный формат ввода. Пожалуйста, выберите действие из меню.", replyMarkup: GetMainMenuKeyboard());
                }
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task ShowMainMenu(long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Добавить упражнение"),
                new KeyboardButton("Прогресс"),
                new KeyboardButton("Напоминание")
            })
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: keyboard);
            _userStates[chatId] = "MainMenu";
        }

        private ReplyKeyboardMarkup GetBackButton()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Назад")
            })
            {
                ResizeKeyboard = true
            };
        }

        private ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Добавить упражнение"),
                new KeyboardButton("Прогресс"),
                new KeyboardButton("Напоминание")
            })
            {
                ResizeKeyboard = true
            };
        }
    }
}
