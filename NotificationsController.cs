// Copyright (c) Aidvanced Srl. All rights reserved.
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using System.Web.Http;
using System.Web.Http.Tracing;
using System;
using System.Threading.Tasks;


namespace wpc_appService.Controllers
{
    [MobileAppController]
    public class NotificationsController : ApiController
    {
        public async Task<string> PostNotification(string type, string message, string deviceTag, string fromUser)
        {

            HttpConfiguration config = this.Configuration;
            MobileAppSettingsDictionary settings =
                this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();

            // Create a new Notification Hub client.
            NotificationHubClient hub =Notifications.Instance.Hub;

            var notif = "{ \"data\" : {\"type\":\"" + type + "\", \"message\":\"" + message + "\", \"fromUser\":\"" + fromUser + "\", \"sound\" : \"default\"}}";

            try
            {

                // Send the push notification and log the results.
                var result = await hub.SendGcmNativeNotificationAsync(notif, deviceTag);

                // Write the success result to the logs.
                config.Services.GetTraceWriter().Info(result.State.ToString());
            }
            catch (Exception ex)
            {
                // Write the failure result to the logs.
                config.Services.GetTraceWriter()
                    .Error(ex.Message, null, "Push.SendAsync Error");
            }
            return deviceTag;
        }
    }
}
