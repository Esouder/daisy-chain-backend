using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Reflection;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;


namespace Daisy.Chain
{
    // Basically just the default Azure http trigger function
    public class Response{
        public string? status {get; set;}
        public List<string[]>? solutions {get; set;}
    }

    public class solve
    {
        private readonly ILogger _logger;

        public solve(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<solve>();
        }

        [Function("solve")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);

            var jsonResponse = new Response();

            Solver solver = new Solver();
            var rawAnagrams = req.Query["anagrams"];
            if(rawAnagrams == null){
                jsonResponse.status = "Invalid: Empty";
                response.WriteString(JsonSerializer.Serialize(jsonResponse));
                return response;
            }
            var anagrams = rawAnagrams.ToString().Split(",");

            if(anagrams.Length == 0 || anagrams[0] == ""){
                jsonResponse.status = "Invalid: Empty";
            } else {
                int numLetters = anagrams[0].Length;

                for (int i = 0; i < anagrams.Length; i++){
                    if (anagrams[i].Length != numLetters){
                        jsonResponse.status = "Invalid: Different Lengths";
                        break;
                    }
                }
                var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, "../../.."));
                string dictionaryPath = System.IO.Path.Combine(rootDirectory, "dictionary.txt");
                List<string[]>? solutions = solver.solve(anagrams, dictionaryPath);
                if (solutions == null){
                    jsonResponse.status = "Error: No Solutions";
                } else {
                    jsonResponse.status = "OK";
                    jsonResponse.solutions = solutions;
                }

            }

            response.WriteString(JsonSerializer.Serialize(jsonResponse));
            
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            return response;
        }
    }
}
