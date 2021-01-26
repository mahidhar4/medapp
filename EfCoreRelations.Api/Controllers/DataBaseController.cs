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
using Microsoft.AspNetCore.Hosting;

namespace EfCoreRelations.Api.Controllers
{
    [ApiController]
    [Route("api/DataBase")]
    public class DataBaseController : ControllerBase
    {

        private readonly ILogger<DataBaseController> _logger;
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment hostingEnvironment { get; }

        public DataBaseController(ILogger<DataBaseController> logger, IConfiguration configuration, IWebHostEnvironment _hostingEnvironment)
        {
            _logger = logger;
            Configuration = configuration;
            hostingEnvironment = _hostingEnvironment;
        }

        private string GetConnectionString(bool isUrl = false)
        {
            if (isUrl)
            {
                //postgres://username:password@host:port/database
                string url = Configuration.GetConnectionString("DATABASE_URL");

                string connectionStr = $"Host={url.Split("@")[1].Split(":")[0]};Database={url.Split("@")[1].Split("/")[1]};Username={url.Split("://")[1].Split(":")[0]};Password={url.Split("://")[1].Split(":")[1].Split("@")[0]};Port={url.Split("@")[1].Split(":")[1].Split("/")[0]}";

                if (hostingEnvironment.EnvironmentName == "Production")
                {
                    connectionStr = connectionStr + "sslmode=Prefer;Trust Server Certificate=true";
                }

                return connectionStr;
            }
            else
                return Configuration.GetConnectionString("Dev_BasketballDB");
        }

        [HttpGet]
        [Route("TableQueries")]
        public async Task<ActionResult> GetDataQueryAsync([FromQuery][Required] string query)
        {
            try
            {
                using (var connection = new Npgsql.NpgsqlConnection(GetConnectionString()))
                {
                    return Ok(await connection.QueryAsync<IEnumerable<object>>(query));
                }
            }
            catch (System.Exception ex)
            {

                return BadRequest(ex.ToString() + ex.StackTrace.ToString());

            }

        }


        [HttpGet]
        [Route("TableReadData")]
        public async Task<ActionResult> GetDataAsync([FromQuery][Required] string query)
        {
            try
            {
                using (var connection = new Npgsql.NpgsqlConnection(GetConnectionString()))
                {
                    var tables = (from row in await connection.QueryAsync(query) select (IDictionary<string, object>)row).AsList();
                    return new JsonResult(tables);
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.ToString() + ex.StackTrace.ToString());
            }

        }

        [HttpGet]
        [Route("ConnectionInfo")]
        public async Task<ActionResult> GetConnectionInfo()
        {
            string connectionInfo = "novalue";
            try
            {
                connectionInfo = GetConnectionString(true);
                using (var connection = new Npgsql.NpgsqlConnection(connectionInfo))
                {
                    await connection.OpenAsync();
                    await connection.CloseAsync();

                    return Ok(connectionInfo + hostingEnvironment.EnvironmentName);
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(connectionInfo + hostingEnvironment.EnvironmentName + ex.ToString() + ex.StackTrace.ToString());
            }
        }
    }
}
