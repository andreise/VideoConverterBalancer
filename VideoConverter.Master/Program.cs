namespace VideoConverter.Master
{
    using Model;

    static class Program
    {
        static string ReadHostNameSetting() => "localhost";

        static void Main(string[] args) => new VideoConverterMasterApplication(ReadHostNameSetting(), args).RunAsync().Wait();
    }
}