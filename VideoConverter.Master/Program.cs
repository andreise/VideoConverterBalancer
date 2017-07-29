namespace VideoConverter.Master
{
    using Model;

    static class Program
    {
        static string ReadMQServerHostSetting() => "localhost";

        static void Main(string[] args) => new VideoConverterMasterApplication(ReadMQServerHostSetting(), args).RunAsync().Wait();
    }
}