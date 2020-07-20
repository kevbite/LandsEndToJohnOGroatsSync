using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LandsEndToJohnOGroatsSync
{
    public class LandsEnd3FireBaseAppClient
    {
        private HttpClient _client;
        private static readonly Random Random = new Random();

        public LandsEnd3FireBaseAppClient()
        {   
            _client = new HttpClient();
        }

        public async Task SubmitData(ILandsEnd3FireBaseAppAthleteData data, DateTime dateTime, double miles)
        {
            var url =
                $"https://script.google.com/macros/s/AKfycbxkmeISIIZFONdYYkZM7S0JzVuZMfRbaNsCjDJjvKF3pCD1jqY/exec?callback=jQuery33109242062575738661_1595280391802&primary[0][]={data.Name}&primary[0][]={data.Pin}&primary[0][]={data.Bib}&primary[0][]={dateTime:dd'/'MM'/'yyyy}&primary[0][]={miles}&primary[0][]=&action=submitMileage&_={Random.Next()}";

            var responseMessage = await _client.GetAsync(url);

            responseMessage.EnsureSuccessStatusCode();

            var text = await responseMessage.Content.ReadAsStringAsync();

            if (text.Contains("Error"))
            {
                throw new Exception("Error Submitting Data: \n" + text);
            }
        }

        //https://script.google.com/macros/s/AKfycbxkmeISIIZFONdYYkZM7S0JzVuZMfRbaNsCjDJjvKF3pCD1jqY/exec?callback=jQuery33109242062575738661_1595280391802&primary%5B0%5D%5B%5D=Name&primary%5B0%5D%5B%5D=1&primary%5B0%5D%5B%5D=2&primary%5B0%5D%5B%5D=01%2F02%2F1985&primary%5B0%5D%5B%5D=3&primary%5B0%5D%5B%5D=&action=submitMileage&_=1595280391803

    }
}