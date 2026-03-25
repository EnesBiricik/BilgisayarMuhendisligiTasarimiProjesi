namespace BilgisayarMuhendisligiTasarimi.Services
{
    public interface IRecaptchaValidator
    {
        bool IsRecaptchaValid(string token);
    }
}