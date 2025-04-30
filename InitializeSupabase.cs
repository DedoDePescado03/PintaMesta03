using Supabase;
using System.Diagnostics;
using System.Threading.Tasks;

public static class SupabaseClientService
{
    public static Supabase.Client SupabaseClient { get; private set; }

    public static async Task InitializeAsync()
    {

        try
        {
            var url = "https://sqohokjrvzkpyemexfuq.supabase.co";
            var public_key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InNxb2hva2pydnprcHllbWV4ZnVxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQ1MDUzOTcsImV4cCI6MjA2MDA4MTM5N30.pZZ0VuNOfRSP2ZDy9cW2IPdIrQCzSfKlaWV677cZ0a4";

            SupabaseClient = new Supabase.Client(url, public_key, new SupabaseOptions
            {
                AutoConnectRealtime = true
            });

            await SupabaseClient.InitializeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

    }

}