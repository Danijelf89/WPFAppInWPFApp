using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using WpfAppAITest.Interfaces;
using static System.Net.WebRequestMethods;

namespace WpfAppAITest.Services
{
    public class HealthCheckService
    {
        private readonly IHttpBuilder _http;

        public HealthCheckService(IHttpBuilder http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<bool> CheckIfAlive()
        {
            try
            {
                Log.Information("AiProcessingService - CheckIfAlive: Starting transcription to API.");
                
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));

                HttpResponseMessage response = await _http.HttpClient.GetAsync("https://localhost:7190/api/healthCheck", cts.Token);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("AiProcessingService - CheckIfAlive: Not alive.");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error("AiProcessingService - CheckIfAlive: Not alive with error.");
                return false;
            }

        }
    }
}
