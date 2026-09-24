using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Services;
using VtmCharacterGenerator.Core.Services.Strategies;
using VtmCharacterGenerator.Core.Services.XpStrategies;
using VtmCharacterGenerator.WebApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameDataProvider>(sp =>
{
    // Most reliable way to find the solution root, since i use this project in both console and web app formats

    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    DirectoryInfo dirInfo = new DirectoryInfo(currentDirectory);
    while (dirInfo != null && !dirInfo.GetFiles("*.sln").Any())
    {
        dirInfo = dirInfo.Parent;
    }

    string solutionRoot = dirInfo.FullName;
    string dataPath = Path.Combine(solutionRoot, "GameData");

    return new GameDataProvider(dataPath);
});


// "AddScoped" means a new instance will be created for each web request.
builder.Services.AddScoped<CharacterGeneratorService>();
builder.Services.AddScoped<AffinityProcessorService>();
builder.Services.AddScoped<PersonaService>();
builder.Services.AddScoped<AttributeService>();
builder.Services.AddScoped<AbilityDistributionService>();
builder.Services.AddScoped<BackgroundDistributionService>();
builder.Services.AddScoped<VirtueDistributionService>();
builder.Services.AddScoped<DisciplineDistributionService>();
builder.Services.AddScoped<CoreStatsService>();
builder.Services.AddScoped<ITraitCostStrategy, FreebiePointCostStrategy>();
builder.Services.AddScoped<TraitManagerService>();
builder.Services.AddScoped<FreebieSpendingService>();
builder.Services.AddScoped<LifeCycleService>();
builder.Services.AddScoped<XpSpendingService>();
builder.Services.AddScoped<ITraitCostStrategy, XpPointCostStrategy>();
builder.Services.AddScoped<IXpStrategy, XpAttributeStrategy>();
builder.Services.AddScoped<IXpStrategy, XpAbilityStrategy>();
builder.Services.AddScoped<IXpStrategy, XpWillpowerStrategy>();
builder.Services.AddScoped<IXpStrategy, XpVirtueStrategy>();
builder.Services.AddScoped<IXpStrategy, XpHumanityStrategy>();
builder.Services.AddScoped<IXpStrategy, XpDisciplineStrategy>();
builder.Services.AddScoped<NameGeneratorService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var retryAfter = TimeSpan.FromSeconds(60);
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var leaseRetryAfter))
        {
            retryAfter = leaseRetryAfter;
        }

        var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
        context.HttpContext.Response.Headers["Retry-After"] =
            retryAfterSeconds.ToString(CultureInfo.InvariantCulture);

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                error = "Too many character generation requests. Please wait before trying again.",
                retryAfterSeconds
            },
            cancellationToken);
    };

    // Both generation endpoints share an IP bucket so clients cannot bypass the limit by switching routes.
    options.AddPolicy(RateLimitPolicyNames.CharacterGeneration, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = TimeSpan.FromSeconds(60)
            }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(); // This will look for index.html as the default page.

app.UseStaticFiles();  // This enables serving files from the wwwroot folder.

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
