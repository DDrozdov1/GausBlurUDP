using ImageProcessing.Common;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку Razor Pages и Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Регистрация ваших сервисов (например, UdpHelper)
builder.Services.AddSingleton<UdpHelper>(sp => new UdpHelper(
    sp.GetRequiredService<ILogger<UdpHelper>>(),
    12345 // Порт для UDP
));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Настройка маршрутов для Blazor Server
app.MapBlazorHub();
app.MapFallbackToFile("index.html"); // Указываем, что Blazor работает через index.html

app.Run();