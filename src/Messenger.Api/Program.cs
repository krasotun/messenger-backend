var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

// WebApplicationFactory в e2e-тестах требует доступного типа точки входа,
// а при top-level statements он генерируется internal.
public partial class Program;
