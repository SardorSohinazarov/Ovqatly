using Bot.Services;
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
builder.Services.AddSingleton(new TelegramBotClient(builder.Configuration["TelegramBotAPIKey"]));
builder.Services.AddSingleton<IUpdateHandler, UpdateHandlerService>();

builder.Services.AddSingleton<Client>(sp =>
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
