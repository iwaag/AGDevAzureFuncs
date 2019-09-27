using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AGBLang.StdUtil;
using AGBLang;
using AGDev.StdUtil;

namespace TestFunction {
	public static class ProcessNLang {
		[FunctionName("ProcessNLang")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
			ILogger log) {
			#region get input
			log.LogInformation("C# HTTP trigger function processed a request.");
			var form = await req.ReadFormAsync();
			var files = form.Files;
			var ewordsFile = files["ewords"];
			MemoryStream ms = new MemoryStream();
			ewordsFile.OpenReadStream().CopyTo(ms);
			var content = ms.ToArray();
			#endregion
			string nLang = System.Text.Encoding.UTF8.GetString(content);
			NaturalLanguageProcessor processor = new ExampleNLangProcessor("BasicEnglishDictionary.json");
			var collector = new EasyAsyncCollector<GrammarBlock>();
			processor.PerformSyntacticProcess(nLang, collector);
			var jsonStr = GrammarBlockUtils.ToJson(collector.collected);
			var json = System.Text.Encoding.UTF8.GetBytes(jsonStr);
			return json!=null
				? (ActionResult)new FileContentResult(json, "text/html")
				: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
		}
	}
}
