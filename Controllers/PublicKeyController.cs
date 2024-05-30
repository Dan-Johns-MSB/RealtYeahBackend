using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using RealtYeahBackend.Models;
using RealtYeahBackend.Services;
using Microsoft.AspNetCore.Authorization;

namespace RealtYeahBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class PublicKeyController : ControllerBase
    {
        private readonly CspParameters csp;

        public PublicKeyController(CspParameters csp)
        {
            this.csp = csp;
        }

        [HttpGet]
        public IActionResult GetPubKey()
        {
            byte[] tmp;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096, csp))
            {
                tmp = rsa.ExportRSAPublicKey();
            }
            return Ok(new { pubKey = tmp});
        }
    }
}
