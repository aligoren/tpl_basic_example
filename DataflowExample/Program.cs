using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks.Dataflow;

public class AuthService
{
    private static string Token;

    public static string GetToken()
    {
        string token = string.Empty;
        if (Token is null) token = Guid.NewGuid().ToString();
        return token;
    }
}

internal class Program
{
    private static async Task Main(string[] args)
    {
        var dataflowBlockOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 2 // Max concurrent requests
        };

        if (!Directory.Exists("results")) Directory.CreateDirectory("results");

        var piBlock = new TransformBlock<PnrRequestModel, (string token, PnrRequestModel data)>(async requestData =>
        {
            var jwtToken = AuthService.GetToken();
            var data = $"piResponse: {requestData.Pnr} and token: {jwtToken}";
            File.WriteAllText($"results/{requestData.Pnr}_{jwtToken}_piBlock.txt", data);
            Thread.Sleep(2000);
            return (token: jwtToken, requestData);
        }, dataflowBlockOptions);

        var providerBlock = new ActionBlock<(string Token, PnrRequestModel data)>(async response =>
        {
            var jwtToken = AuthService.GetToken();
            var data = $"ticket for: {response.Token} and token: {response.Token}";
            File.WriteAllText($"results/{response.data.Pnr}_{response.Token}_providerBlock.txt", data);
            Thread.Sleep(2000);
        }, dataflowBlockOptions);


        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        piBlock.LinkTo(providerBlock, linkOptions);

        var pnrs = new List<PnrRequestModel> 
        {
            new PnrRequestModel { Pnr = "ABCD", Surname ="SURABCD" },
            new PnrRequestModel { Pnr = "CCDSA", Surname ="SURCCDSA" },
            new PnrRequestModel { Pnr = "TSAFSA", Surname ="SURTSAFSA" },
            new PnrRequestModel { Pnr = "KAKAKAKA", Surname ="SURKAKAKAKA" },
            new PnrRequestModel { Pnr = "CCCCE", Surname ="SURCCCCE" },
            new PnrRequestModel { Pnr = "SSSEEE", Surname ="SURSSSEEE" },
            new PnrRequestModel { Pnr = "KSAJFJHS", Surname ="SURKSAJFJHS" },
        };

        pnrs.ForEach(pnr => piBlock.SendAsync(pnr));
        piBlock.Complete();

        await providerBlock.Completion;
    }
}

public class PnrRequestModel
{
    public string Pnr { get; set; }
    public string Surname { get; set; }
}