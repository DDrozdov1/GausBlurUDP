using ImageProcessing.Common;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� Razor Pages � Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ����������� ����� �������� (��������, UdpHelper)
builder.Services.AddSingleton<UdpHelper>(sp => new UdpHelper(
    sp.GetRequiredService<ILogger<UdpHelper>>(),
    12345 // ���� ��� UDP
));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// ��������� ��������� ��� Blazor Server
app.MapBlazorHub();
app.MapFallbackToFile("index.html"); // ���������, ��� Blazor �������� ����� index.html

app.Run();