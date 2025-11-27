using VtmCharacterGenerator.Core.Data; 
using VtmCharacterGenerator.Core.Services;
using VtmCharacterGenerator.Core.Services.Strategies;
using VtmCharacterGenerator.Core.Services.XpStrategies;

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



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(); // This will look for index.html as the default page.

app.UseStaticFiles();  // This enables serving files from the wwwroot folder.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();