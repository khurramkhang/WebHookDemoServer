using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebHooksSender
{
    class Program
    {
        private static IWebHookManager _whManager;
        private static IWebHookStore _whStore;

        static void Main(string[] args)
        {
            _whStore = new MemoryWebHookStore();
            _whManager = new WebHookManager(_whStore, new TraceLogger());

            //Alloy site URL
            string receiverUrl = "http://localhost:51481";

            //Subscribe alloy site in Memory of server
            var wh = SubscribeNewUser(receiverUrl);

            // Send Notification to all subscribers
            SendWebhookAsync("alloy-plan", "Alloy Plan").Wait();

            //verify the webhook
            var verify = _whManager.VerifyWebHookAsync(wh);

            Console.ReadLine();
        }

        private static WebHook SubscribeNewUser(string host)
        {
            var webhook = new WebHook();
            //Alloy site will require this key in web.config to make the link
            string secretKey = "12345678901234567890123456789012";
            string webHookUrl = "/api/webhooks/incoming/custom";

            //action name, Client will register to.
            webhook.Filters.Add("entryupdating");

            //A test paramameter
            webhook.Properties.Add("StaticParamA", 10);
            webhook.Secret = secretKey;
            webhook.WebHookUri = host + webHookUrl;
            
            //refister alloy site as user 1 in memory
            _whStore.InsertWebHookAsync("user1", webhook);

            return webhook;
        }

        /// <summary>
        /// Send Notification to register clients
        /// </summary>
        /// <param name="product">product code</param>
        /// <param name="productName"> New Product Name</param>
        /// <returns></returns>
        private static async Task SendWebhookAsync(string product, string productName)
        {
            var notifications = new List<NotificationDictionary> { new NotificationDictionary("entryupdating", new { id = product, name = productName }) };
            var x = await _whManager.NotifyAsync("user1", notifications);
        }
    }
}