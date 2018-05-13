using System.Threading.Tasks;

namespace VideoConverter.Master
{
    using Model;

    static class Program
    {
        static string ReadMQServerHostSetting() => "localhost";

        static async Task MainAsync(string[] args) =>
            await new VideoConverterMasterApplication(ReadMQServerHostSetting(), args).RunAsync();

        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    }
}