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
        [Route("TableQueries")]
        public async Task<ActionResult> GetDataQueryAsync([FromQuery][Required] string query)
        {
            try
            {
                using (var connection = new Npgsql.NpgsqlConnection(Configuration.GetConnectionString("Dev_BasketballDB")))
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
                using (var connection = new Npgsql.NpgsqlConnection(Configuration.GetConnectionString("Dev_BasketballDB")))
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
    }
}
