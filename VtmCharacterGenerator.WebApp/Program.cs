using VtmCharacterGenerator.Core.Data; 
using VtmCharacterGenerator.Core.Services; 

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