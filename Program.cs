using FluentValidation;
using RbcTestTask.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрируем FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); 

// Регистрируем сервис обработки
builder.Services.AddScoped<ProcessingService>();

var app = builder.Build();

// Настраиваем конвейер HTTP-запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();