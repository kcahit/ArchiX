using ArchiX.Library.Web;
using ArchiX.Library.Web.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Register shared web defaults
builder.Services.AddArchiXWebDefaults();
builder.Services.AddApplicationMappings();

var app = builder.Build();
app.MapGet("/", () => "ArchiX WebHost - OK");
app.Run();

public partial class Program { }
