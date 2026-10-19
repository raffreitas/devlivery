using Amazon.Runtime;
using Amazon.S3;

using Devlivery.BackupJob.Configuration;
using Devlivery.BackupJob.Observability;
using Devlivery.BackupJob.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.SingleLine = true;
});

builder.AddBackupObservabilityFeature();

builder.Services.AddOptions<BackupOptions>()
    .Bind(builder.Configuration.GetSection(BackupOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.PostConfigure<BackupOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.ApplicationName))
    {
        options.ApplicationName = "devlivery";
    }

    if (string.IsNullOrWhiteSpace(options.EnvironmentName))
    {
        options.EnvironmentName = builder.Environment.EnvironmentName;
    }
});

builder.Services.AddSingleton<IValidateOptions<BackupOptions>, BackupOptionsValidator>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IBackupObjectKeyFactory, BackupObjectKeyFactory>();
builder.Services.AddSingleton<IFileChecksumProvider, FileChecksumProvider>();
builder.Services.AddSingleton<IPostgresConnectionInfoParser, PostgresConnectionInfoParser>();
builder.Services.AddSingleton<IPostgresDumpExporter, PostgresDumpExporter>();
builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BackupOptions>>().Value;

    var config = new AmazonS3Config
    {
        ServiceURL = options.R2Endpoint, ForcePathStyle = options.ForcePathStyle, AuthenticationRegion = "auto",
    };

    return new AmazonS3Client(
        new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey),
        config
    );
});
builder.Services.AddSingleton<IBackupStorageClient, R2BackupStorageClient>();
builder.Services.AddSingleton<IBackupRetentionService, BackupRetentionService>();
builder.Services.AddSingleton<BackupJobRunner>();

using var app = builder.Build();

var runner = app.Services.GetRequiredService<BackupJobRunner>();

return await runner.RunAsync(CancellationToken.None);