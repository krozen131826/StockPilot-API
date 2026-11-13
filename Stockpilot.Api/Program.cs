using Stockpilot.Api;

var builder = WebApplication.CreateBuilder(args);

var app = await builder.ConfigureServices();

app.Run();

public partial class Program { }
