using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    public class ErrorResult
    {
        public int Status => (int)Error;
        public string Message { get; set; }
        public HttpStatusCode Error { get; set; }
    }   
}
