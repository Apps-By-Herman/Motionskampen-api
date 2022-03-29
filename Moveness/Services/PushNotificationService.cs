using Microsoft.Extensions.Configuration;
using Moveness.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Moveness.Services
{
    #pragma warning disable IDE1006 // Naming Styles
    class Notification
    {
        public string title { get; set; }
        public string body { get; set; }
    }

    class Message
    {
        public IEnumerable<string> registration_ids { get; set; }
        public Notification notification { get; set; }
    }
    
    public interface IPushNotificationService
    {
        Task NewPrivateChallenge(ApplicationUser user);
        Task NewTeamChallenge(Team challengedTeam, Team challengingTeam, ApplicationUser user);
        Task AddedToTeam(string teamName, ApplicationUser user);
        Task RemovedFromTeam(string teamName, ApplicationUser user);
    }

    public class PushNotificationService : IPushNotificationService
    {
        const string FireBasePushNotificationsURL = "https://fcm.googleapis.com/fcm/send";
        private readonly string ApiKey;
        
        public PushNotificationService(IConfiguration configuration)
        {
            ApiKey = configuration["FirebaseApiKey"];
        }

        public async Task NewPrivateChallenge(ApplicationUser user)
        {
            string title = user.PreferredLanguage == "sv" ? "Ny utmaning!" : "New challenge!";
            string message = user.PreferredLanguage == "sv"
                ? $"Ny utmaning från {user.UserName}. Öppna för att svara." 
                : $"New challenge from {user.UserName}. Open to respond.";

            await SendPushNotification(title, message, user);
        }

        public async Task NewTeamChallenge(Team challengedTeam, Team challengingTeam, ApplicationUser user)
        {
            string title = user.PreferredLanguage == "sv" ? "Ny utmaning!" : "New challenge!";
            string message = user.PreferredLanguage == "sv"
                ? $"Ditt team {challengedTeam.Name} har blivit utmanad av {challengedTeam.Name}. Öppna för att svara."
                : $"Your team {challengedTeam.Name} has been challenged by {challengedTeam.Name}. Open to respond.";

            await SendPushNotification(title, message, user);
        }

        public async Task AddedToTeam(string teamName, ApplicationUser user)
        {
            string title = user.PreferredLanguage == "sv" ? "Tillagd till nytt team" : "Added to new team";
            string message = user.PreferredLanguage == "sv" ? $"Du har blivit tillagd till teamet {teamName}." : $"You have been added to the team {teamName}.";
            await SendPushNotification(title, message, user);
        }

        public async Task RemovedFromTeam(string teamName, ApplicationUser user)
        {
            string title = user.PreferredLanguage == "sv" ? "Borttagen från team" : "Removed from team";
            string message = user.PreferredLanguage == "sv" ? $"Du har blivit borttagen från teamet {teamName}." : $"You have been removed from team {teamName}.";
            await SendPushNotification(title, message, user);
        }

        private async Task SendPushNotification(string title, string message, ApplicationUser user)
        {
            if (user == null && user.DeviceToken == null)
                return;

            string token = user.DeviceToken;
            var results = new List<HttpResponseMessage>();

            var pushNotification = new Message
            {
                registration_ids = new List<string>() { token },
                notification = new Notification
                {
                    title = title,
                    body = message,
                }
            };

            var jsonObject = JObject.FromObject(pushNotification);
            string jsonMsg = JsonConvert.SerializeObject(jsonObject);
            results.Add(await SendJsonMsg(jsonMsg));
        }

        private async Task<HttpResponseMessage> SendJsonMsg(string jsonMsg)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, FireBasePushNotificationsURL);
            request.Headers.TryAddWithoutValidation("Authorization", "key =" + ApiKey);
            request.Content = new StringContent(jsonMsg, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            return await client.SendAsync(request);
        }
    }
}
