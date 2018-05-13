using System.Threading.Tasks;

namespace VideoConverter.Node
{
    using Model;

    static class Program
    {
        static string ReadMQServerHostSetting() => "localhost";

        static string ReadConverterCommandLineFormatSetting() => "ffmpeg -i {0} {1}.avi";

        static async Task Main(string[] args) =>
            await new VideoConverterNodeApplication(
                ReadMQServerHostSetting(),
                ReadConverterCommandLineFormatSetting(),
                args)
            .RunAsync();
    }
}