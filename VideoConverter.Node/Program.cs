namespace VideoConverter.Node
{
    using Model;

    static class Program
    {
        static string ReadMQServerHostSetting() => "localhost";

        static string ReadConverterCommandLineFormatSetting() => "ffmpeg -i {0} {1}.avi";

        static void Main(string[] args) => new VideoConverterNodeApplication(
            ReadMQServerHostSetting(),
            ReadConverterCommandLineFormatSetting(),
            args)
            .RunAsync().Wait();
    }
}