using Bot.Services;
using Bot.Services.MessageHandlers;
using Telegram.Bot.Polling;
using Telegram.Bot;
using Google.GenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<BotBackgroundService>();
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(builder.Configuration["TelegramBotAPIKey"]!));
builder.Services.AddSingleton<ITelegramBotFacade, TelegramBotFacade>();
builder.Services.AddSingleton<IUpdateHandler, UpdateHandlerService>();
builder.Services.AddSingleton<IAiResponseService, AiResponseService>();
builder.Services.AddSingleton<IMediaGroupAggregator, MediaGroupAggregator>();
builder.Services.AddSingleton<ITextMessageHandler, TextMessageHandler>();
builder.Services.AddSingleton<IPhotoMessageHandler, PhotoMessageHandler>();

builder.Services.AddSingleton<Client>(_ =>
{
    var apiKey = builder.Configuration["GoogleAIApiKey"];
    return new Client(apiKey: apiKey);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
