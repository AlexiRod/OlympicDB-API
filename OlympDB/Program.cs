using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OlympDB.Database;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<OlympDbContext>();
	//(o => o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=olymp;Trusted_Connection=True;"));
builder.Services.AddControllers()
	.AddNewtonsoftJson(options =>
	{
		options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
		options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "Olympic database API",
		Description = "API for auto-generated database of Olympic games.",
		Contact = new OpenApiContact
		{
			Name = "Rodionov Alexey",
			Url = new Uri("mailto:strose2002@gmail.com")
		},
	});
	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
