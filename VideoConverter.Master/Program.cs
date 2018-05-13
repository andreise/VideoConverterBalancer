using System.Threading.Tasks;

namespace VideoConverter.Master
{
    using Model;

    static class Program
    {
        static string ReadMQServerHostSetting() => "localhost";

        static async Task Main(string[] args) =>
            await new VideoConverterMasterApplication(ReadMQServerHostSetting(), args).RunAsync();
    }
}