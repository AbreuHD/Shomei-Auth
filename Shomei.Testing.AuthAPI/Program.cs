using Shomei.Infraestructure.Identity;
using Shomei.Testing.AuthAPI.Extensions;
using Shomei.Testing.AuthAPI.ExtraConfig.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(); //Add this line
builder.Services.AddIdentityInfrastructure(builder.Configuration); //Add this line
builder.Services.AddSwaggerExtension(); //Swagger Extension
var app = builder.Build();
// Configure the HTTP request pipeline.

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await services.AddIdentityRolesAsync(Enum.GetNames<Roles>());
} // Add this block

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
