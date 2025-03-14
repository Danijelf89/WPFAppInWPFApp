using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppAITest.Interfaces
{
    public interface IHttpBuilder
    {
        HttpClient HttpClient { get; set; }
    }
}
