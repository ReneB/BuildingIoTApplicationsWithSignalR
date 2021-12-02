using Server.Configuration;
using Server.Configuration.Test;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.Configure<SpeechOptions>(builder.Configuration.GetSection(SpeechOptions.ConfigKey));
builder.Services.Configure<AirportOptions>(builder.Configuration.GetSection(AirportOptions.ConfigKey));
builder.Services.Configure<ScheduleOptions>(builder.Configuration.GetSection(ScheduleOptions.ConfigKey));

builder.Services.AddSingleton<Server.Hubs.Services.ConnectedClientRegistry>();
builder.Services.AddSingleton<Server.Services.AnnouncementLog>();
builder.Services.AddSingleton<Server.Services.AnnouncementAudioCreator>();

builder.Services.AddTransient<Server.Services.ScheduleFetcher>();

builder.Services.AddHostedService<Server.HostedServices.ScheduleProcessor>();

if (builder.Environment.EnvironmentName == Microsoft.Extensions.Hosting.Environments.Development) {
    builder.Services.Configure<TimeBaseOptions>(builder.Configuration.GetSection(TimeBaseOptions.ConfigKey));
    builder.Services.AddTransient<Server.Services.ITimeService, Server.Services.TestTimeService>();
} else {
    builder.Services.AddTransient<Server.Services.ITimeService, Server.Services.TimeService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapHub<MainHub>("/main");

app.UseAuthorization();

app.MapRazorPages();

app.Run();
