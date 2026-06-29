using Microsoft.OpenApi.Models;
using OCPPaymentSystemAPI.Data;
using OCPPaymentSystemAPI.Helpers;
using OCPPaymentSystemAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "OCP Payment System API",
            Version = "v1"
        });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key diperlukan. Masukkan API Key tanpa kata 'Bearer'.",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        }
    };

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

builder.Services.AddSingleton<Database>();
//RZK
builder.Services.AddScoped<SupplierData>();
builder.Services.AddScoped<SDGWeighData>();
builder.Services.AddScoped<RunningNumberData>();
builder.Services.AddScoped<RunningNumberHelper>();
builder.Services.AddScoped<MemoData>();
builder.Services.AddScoped<WorkflowData>();
builder.Services.AddScoped<CompanyData>();
builder.Services.AddScoped<OCPSupplierData>();
builder.Services.AddHttpClient<LoginData>();

builder.Services.Configure<SMTPSetting>(
    builder.Configuration.GetSection("SMTP"));

builder.Services.AddScoped<EmailHelper>();
builder.Services.AddScoped<EmailData>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();