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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            if (!isUrl)
                isUrl = hostingEnvironment.EnvironmentName == "Production";

            if (isUrl)
            {
                //postgres://username:password@host:port/database
                string url = Configuration.GetConnectionString("DATABASE_URL");

                if (string.IsNullOrWhiteSpace(url))
                    url = Configuration.GetValue<string>("DATABASE_URL");

                string connectionStr = $"Host={url.Split("@")[1].Split(":")[0]};Database={url.Split("@")[1].Split("/")[1]};Username={url.Split("://")[1].Split(":")[0]};Password={url.Split("://")[1].Split(":")[1].Split("@")[0]};Port={url.Split("@")[1].Split(":")[1].Split("/")[0]}";

                if (hostingEnvironment.EnvironmentName == "Production")
                {
                    connectionStr = connectionStr + ";sslmode=Prefer;Trust Server Certificate=true";
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
            dynamic response = null;
            try
            {
                using (var connection = new Npgsql.NpgsqlConnection(GetConnectionString()))
                {
                    // var dataset = await connection.QueryAsync(query);
                    var dataset2 = await connection.ExecuteReaderAsync(query);
                    List<string> keys = new List<string>();
                    List<object[]> data = new List<object[]>();


                    while (await dataset2.ReadAsync())
                    {
                        List<object> objects = new List<object>();

                        for (int i = 0; i < dataset2.VisibleFieldCount; i++)
                        {
                            var column = dataset2.GetName(0);
                            if (!keys.Contains(column)) keys.Add(column);
                            objects.Add(dataset2.GetValue(i));
                        }
                        data.Add(objects.ToArray());
                    }

                    response = new
                    {
                        keys,
                        data
                    };

                    // var tables = (from row in dataset select (IDictionary<string, object>)row).AsList();
                    // var tablesData = (from row in dataset select (DapperRow)row).AsList();


                    // response = tables;
                    // if (tables != null && tables.Count > 0)
                    // {
                    //     // var dataJson = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(tables[0]));

                    //     var keys = tables[0].Keys; //dataJson.Properties().Select(p => p.Name).ToList();

                    //     response = new
                    //     {
                    //         keys,
                    //         // data = tablesData
                    //     };

                    // }

                    return new JsonResult(response);
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
