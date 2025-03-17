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

namespace WpfAppAITest.Services
{
    public class HealthCheckService
    {
        private readonly IHttpBuilder _http;

        public HealthCheckService(IHttpBuilder http)
        {
            _http = http;
        }

        public async Task<bool> CheckIfAlive()
        {
            try
            {
                Log.Information("AiProcessingService - CheckIfAlive: Starting transcription to API.");

                HttpResponseMessage response = await _http.HttpClient.GetAsync("https://localhost:7190/api/healthCheck");
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("AiProcessingService - CheckIfAlive: Not alive.");
                    return true;
                }
                else
                {
                    Log.Information("AiProcessingService - CheckIfAlive: Not alive.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error("AiProcessingService - CheckIfAlive: Not alive with error.");
                return false;
            }

        }
    }
}
