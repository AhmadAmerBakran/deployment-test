namespace MQTTClient;

public static class MQTTConfig
{
    public static string Server => "mqtt.flespi.io";
    public static int Port => 8883;
    public static string ClientId => "FullStackIoTAppClient";
    public static string Username => Environment.GetEnvironmentVariable("flespi_client");
    public static string Password => "";
    
}