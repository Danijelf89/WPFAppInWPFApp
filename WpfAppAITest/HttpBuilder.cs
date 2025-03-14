using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using WpfAppAITest.Interfaces;

namespace WpfAppAITest
{
    public class HttpBuilder : IHttpBuilder
    {
        public HttpClient HttpClient { get; set; } = new();
    }
}
