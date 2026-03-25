using BilgisayarMuhendisligiTasarimi.Data.Context;
using BilgisayarMuhendisligiTasarimi.Services;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DBContext>();

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Error/403";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<IRecaptchaValidator, RecaptchaValidator>();
builder.Services.AddScoped<ILoggerService, LoggerService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

var app = builder.Build();

//app.UseExceptionHandler(errorApp =>
//{
//    errorApp.Run(async context =>
//    {
//        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
//        var exception = exceptionHandlerPathFeature?.Error;
//        var logger = context.RequestServices.GetRequiredService<ILoggerService>();

//        if (exception != null)
//        {
//            var routeData = context.GetRouteData();
//            var controller = routeData?.Values["controller"]?.ToString() ?? "Unknown";
//            var action = routeData?.Values["action"]?.ToString() ?? "Unknown";
            
//            logger.LogError(
//                exception,
//                controller,
//                action,
//                $"URL: {context.Request.Path}, Method: {context.Request.Method}"
//            );
//        }

//        context.Response.Redirect("/Error/500");
//    });
//});

//app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "panel",
    pattern: "panel",
    defaults: new { controller = "Panel", action = "Index" });

app.MapDefaultControllerRoute();

app.Run();
