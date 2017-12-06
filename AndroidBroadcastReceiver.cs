using System;
using Android.App;
using Android.Content;
using WPC_Android.AppLayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gcm.Client;
using Microsoft.WindowsAzure.MobileServices;
using WPC_Android.View;
using WindowsAzure.Messaging;
using Android.OS;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is only needed for android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

namespace WPC_Android.Code.Applayer
{

    [BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
    Categories = new string[] { "@PACKAGE_NAME@" })]
    public class AccountBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        // Set the Google app ID.
        public static string[] senderIDs = new string[] { "YOUR_SENDERID_PROJECT" };
    }
    // The ServiceAttribute must be applied to the class.
    [Service]
    public class PushHandlerService : GcmServiceBase
    {
        //Set the Notification Hub's attributes
        private const string ListenConnectionString = "YOUR_AZURE_APP_ENPOINT;
        private const string NotificationHubName = "YOUR_AZURE-APP_NAME";
        private NotificationHub _hub;
        public static string RegistrationID { get; private set; }

        public PushHandlerService() : base(AccountBroadcastReceiver.senderIDs) { }

        //Notification Hub registration
        async protected override void OnRegistered(Context context, string registrationId)
        {
            System.Diagnostics.Debug.WriteLine("The device has been registered with GCM.", "Success!");

            // Get the MobileServiceClient from the current activity instance.
            MobileServiceClient client = AzureConnectionFactory.Instance.GetClient();
            var push = client.GetPush();
            var tags = new List<string>() { Data.AccountManager.Instance.Account.Number };

            _hub = new NotificationHub(NotificationHubName, ListenConnectionString, context);
            _hub.Unregister();
            _hub.UnregisterAll(registrationId);

            try
            {
                await Task.Run(() =>
                {
                    //Register with tag
                    var hubRegistration = _hub.Register(registrationId, tags.ToArray());
                });

                System.Diagnostics.Debug.WriteLine(
                    string.Format("Push Installation Id", push.InstallationId.ToString()));
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                      string.Format("Error with Azure push registration: {0}", ex.Message));
            }
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            
            if (intent.Extras.ContainsKey("type"))
            {
                CreateNotification(context, intent);
            }
            
        }

        //Noificiation Hub unregister
        protected override void OnUnRegistered(Context context, string registrationId)
        {
            _hub = new NotificationHub(NotificationHubName, ListenConnectionString, context);
            _hub.UnregisterAll(registrationId);

        }

        protected override void OnError(Context context, string errorId)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("Error occurred in the notification: {0}.", errorId));
        }

        //Create local noification when device receive the push notification
        protected void CreateNotification(Context context, Intent intent)
        {
            string message = string.Empty;
            string tempMessage = intent.Extras.Get("message").ToString();
            string fromUser = intent.Extras.Get("fromUser").ToString();

            var title = "YOUR_NOTIFICATION_TITLE";

            string type = intent.Extras.Get("type").ToString();

            ((WPCApp)Application).LoadAddressBookContacts();
            if (((WPCApp)Application).ContactVM.AddressbookContacts.ContainsKey(fromUser))
            {
                message = ((WPCApp)Application).ContactVM.AddressbookContacts[fromUser] + tempMessage;
            }
            else
            {
                message = fromUser + tempMessage;
            }

            // Create a new intent to show the notification in the UI. 
            
            PendingIntent contentIntent = null;
            int id = 0;

            switch (type)
            {
                case "message":
                    Intent notificationIntent1 = new Intent(this, typeof(FNotification));
                    notificationIntent1.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    contentIntent = PendingIntent.GetActivity(context, id = 1,
                     notificationIntent1, 0);
                    break;
                case "request":
                    Intent notificationIntent2 = new Intent(this, typeof(FPendingContacts));
                    Bundle args = new Bundle();
                    args.PutString("fromUser", fromUser);
                    notificationIntent2.PutExtras(args);
                    notificationIntent2.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    contentIntent = PendingIntent.GetActivity(context, id = 2,
                     notificationIntent2, 0);

                    break;
                case "deny":
                    Intent notificationIntent3 = new Intent(this, typeof(FPendingContacts));
                    notificationIntent3.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    contentIntent = PendingIntent.GetActivity(context, id = 3,
                     notificationIntent3, 0);
                    break;
                case "delete":
                    Intent notificationIntent4 = new Intent(this, typeof(FContacts));
                    notificationIntent4.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    contentIntent = PendingIntent.GetActivity(context, id = 4,
                      notificationIntent4, 0);
                    break;
            }

            // Create a notification manager to send the notification.
            var notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

           

            // Create the notification using the builder.
            var builder = new Notification.Builder(context);
            builder.SetAutoCancel(true);
            builder.SetContentIntent(contentIntent);
            builder.SetContentTitle(title);
            builder.SetContentText(message);
            builder.SetDefaults(NotificationDefaults.Sound);
            builder.SetSmallIcon(Resource.Drawable.Icon);
            var notification = builder.Build();

           
            // Display the notification in the Notifications Area.
            notificationManager.Notify(1, notification);
        }

        
    }
}
