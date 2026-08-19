using Serilog;
using WorkerSafetyDashboard.Services;


Log.Logger = new LoggerConfiguration()
               .CreateLogger();

try 
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>

         lc.WriteTo.File(ctx.HostingEnvironment.ContentRootPath + ctx.Configuration.GetValue<string>("logFilePath"),
         rollingInterval: RollingInterval.Day,
         rollOnFileSizeLimit: true,
         fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
         retainedFileCountLimit: 31, // Keep 31 days
         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
         .ReadFrom.Configuration(ctx.Configuration));
    // Add services to the container.
    builder.Services.AddSingleton<FortyGuardService>();
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

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

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

