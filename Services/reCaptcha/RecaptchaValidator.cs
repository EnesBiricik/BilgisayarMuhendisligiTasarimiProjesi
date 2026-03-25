using Newtonsoft.Json;

namespace BilgisayarMuhendisligiTasarimi.Services
{
    public class RecaptchaValidator : IRecaptchaValidator
    {
        private const string GoogleRecaptchaAddress = "https://www.google.com/recaptcha/api/siteverify";

        public readonly IConfiguration Configuration;

        public RecaptchaValidator(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public bool IsRecaptchaValid(string token)
        {
            using var client = new HttpClient();
            var response = client
                .GetStringAsync(
                    $@"{GoogleRecaptchaAddress}?secret={Configuration["Google:RecaptchaV2SecretKey"]}&response={token}")
                .Result;
            var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(response);

            if (!recaptchaResponse.Success)
            {
                return false;
            }

            return true;
        }
    }
}