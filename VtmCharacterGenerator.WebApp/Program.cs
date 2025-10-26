using VtmCharacterGenerator.Core.Data; 
using VtmCharacterGenerator.Core.Services; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<GameDataProvider>(sp =>
{   
    string webRootPath = builder.Environment.ContentRootPath;
    string dataPath = Path.Combine(webRootPath, "..", "GameData");
    return new GameDataProvider(dataPath);
});

// "AddScoped" means a new instance will be created for each web request.
builder.Services.AddScoped<CharacterGeneratorService>();



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