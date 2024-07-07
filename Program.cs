using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Eq;

public class Program
{
    public static Form1? MainForm { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().RunAsync();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        MainForm = new Form1();
        
        Application.Run(MainForm);
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
}