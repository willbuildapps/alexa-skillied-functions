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
            }

            return new OkObjectResult(skillResponse);
        }

        private static SkillResponse GetAleatorySlangResponse()
        {
            var slangs = new List<string>
            {
                "E aee rapá: um de dizer ”oi, tudo bem?”",
                "Abraça: significa desistir de algo. Algo na linha do “já era”, não tem mais jeito.",
                "Ô, véi: muito usado para chamar a atenção de outra pessoa. O mesmo que cara, mano, irmão…",
                "BÉRA: Uma das palavras mais importantes do universo: significa cerveja!",
                "LARGAR OS BETS: O termo é utilizado quando alguém desiste de alguma coisa.",
                "FRIACA: Quando está muito frio se diz friaca.",
                "JAPONA: Japona é um casaco grande usado no frio. Uma jaqueta grande com zíper na frente com, ou sem capuz",
                "BERMA: O mesmo que Bermuda.",
                "Mó comédia: a pessoa que está sem credibilidade.",
                "Cacete armado: lugar ruim demais"
            };

            var rnd = new Random();
            var slang = slangs.ElementAt(rnd.Next(slangs.Count));
            return BuildTellResponse(slang, false);
        }

        private static SkillResponse BuildRentcarsResponse()
        {
            var text = "Ok, mas para alugar um carro você precisa dizer primeiro datas de inicio e fim!";
            return BuildTellResponse(text, false);
        }

        private static SkillResponse BuildGoodbyeResponse() 
        {
            var text = "Até mais mano, da um salve para os trutas=! É NÓIS!!!";
            return BuildTellResponse(text, true);
        }
        
        private static SkillResponse BuildHelpResponse()
        {
            var help = "Na Moral! Diz para nóis o que você quer, para a gente te dar uma força!";
            return BuildTellResponse(help, false);
        }

        private static SkillResponse BuildTellResponse(string message, bool shouldEndSession)
        {
            var responseBuilder = ResponseBuilder.Tell(message);
            responseBuilder.Response.ShouldEndSession = shouldEndSession;
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
