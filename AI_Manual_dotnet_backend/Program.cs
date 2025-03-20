
using Whisper.net.Ggml;
using Whisper.net;
using AI_Manual_dotnet_backend.Services;
using System.Text;

namespace AI_Manual_dotnet_backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "whisper-models", "ggml-base.bin");
            if (!File.Exists(modelPath))
            {
                Directory.CreateDirectory("whisper-models");
                using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
                await using var fileWriter = File.Create(modelPath);
                await modelStream.CopyToAsync(fileWriter);
            }

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("allowAny", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Register Whisper Factory as a Singleton
            var whisperFactory = WhisperFactory.FromPath(modelPath);
            builder.Services.AddSingleton(whisperFactory);

            builder.Services.AddSingleton<TranscriptionService>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();


        }
    }
}
