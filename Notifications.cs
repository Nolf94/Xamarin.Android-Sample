using Microsoft.Azure.NotificationHubs;
public class Notifications
{
    public static Notifications Instance = new Notifications();

    public NotificationHubClient Hub { get; set; }

    private Notifications()
    {
        Hub = NotificationHubClient.CreateClientFromConnectionString("YOUR_APP_BACKPOINT");
    }
}
