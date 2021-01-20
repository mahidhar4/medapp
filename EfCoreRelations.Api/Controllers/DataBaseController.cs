using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.ComponentModel.DataAnnotations;

namespace EfCoreRelations.Api.Controllers
{
    [ApiController]
    [Route("api/DataBase")]
    public class DataBaseController : ControllerBase
    {

        private readonly ILogger<DataBaseController> _logger;
        public IConfiguration Configuration { get; }

        public DataBaseController(ILogger<DataBaseController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet]
        [Route("TableData")]
        public async Task<ActionResult> GetDataAsync([FromQuery][Required] string query)
        {
            using (var connection = new Npgsql.NpgsqlConnection(Configuration.GetConnectionString("Dev_BasketballDB")))
            {
                return Ok(await connection.QueryAsync<object>(query));
            }
        }
    }
}
