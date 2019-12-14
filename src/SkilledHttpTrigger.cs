using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Response;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using System.Collections.Generic;
using System.Linq;

namespace AlexaFunctionDemo
{
    public static class SkilledHttpTrigger
    {
        [FunctionName("skilled")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var skillRequest = await ParseSkillRequestFromJson(req);
           
            if (skillRequest == null)
            {
                return new BadRequestResult();
            }

            var requestType = skillRequest.GetRequestType();
            
            SkillResponse skillResponse = null;

            if (requestType == typeof(LaunchRequest))
            {
                skillResponse = BuildHelpResponse();
            }
            else if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;
                switch (intentRequest.Intent.Name)
                {
                    case "salve":
                        skillResponse = GetAleatorySlangResponse();
                        break;
                    case "alugarcarro":
                        skillResponse = BuildRentcarsResponse();
                        break;
                    case "AMAZON.StopIntent":
                    case "AMAZON.CancelIntent":
                        skillResponse = BuildGoodbyeResponse();
                        break;
                    case "AMAZON.HelpIntent":
                        skillResponse = BuildHelpResponse();
                        break;
                    default:
                        skillResponse = ResponseBuilder.Empty();
                        skillResponse.Response.ShouldEndSession = false;
                        break;
                }
            }
            else if (requestType == typeof(SessionEndedRequest))
            {
                skillResponse = BuildGoodbyeResponse();
                skillResponse.Response.ShouldEndSession = true;
            }

            return new OkObjectResult(skillResponse);
        }

        private static SkillResponse GetAleatorySlangResponse()
        {
            var slangs = new List<string>
            {
                "E aee rapá: um jeito bem paulistano de dizer ”oi, tudo bem?”",
                "Abraça: em Sampa significa desistir de algo. Algo na linha do “já era”, não tem mais jeito.",
                "Ô, véi: muito usado para chamar a atenção de outra pessoa. O mesmo que cara, mano, irmão…"
            };

            var rnd = new Random();
            var slang = slangs.ElementAt(rnd.Next(slangs.Count));

            var responseBuilder = ResponseBuilder.Tell(slang);
            responseBuilder.Response.ShouldEndSession = false;
            return responseBuilder;
        }

        private static SkillResponse BuildRentcarsResponse()
        {
            var text = "Para alugar um carro você precisa dizer a data de inicio e fim!";
            var responseBuilder = ResponseBuilder.Tell(text);
            responseBuilder.Response.ShouldEndSession = false;
            return responseBuilder;
        }

        private static SkillResponse BuildGoodbyeResponse() =>
            ResponseBuilder.Tell("Até mais mano, da um salve para os trutas, é nóis!");
        
        private static SkillResponse BuildHelpResponse()
        {
            var help = "Presta atenção! Você precisa dizer o que quer, para que eu te ajude!";
            var responseBuilder = ResponseBuilder.Tell(help);
            responseBuilder.Response.ShouldEndSession = false;
            return responseBuilder;
        }

        private static async Task<SkillRequest> ParseSkillRequestFromJson(HttpRequest req) 
        {
            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
            return skillRequest;
        }
        
    }
}
