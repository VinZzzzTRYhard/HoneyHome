using PluginSDK;

namespace Voltage
{
    public class Voltage : IPluginSDK
    {
        public Voltage()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private System.Reflection.Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("PluginSDK"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = System.IO.Path.Combine(
                    "..\\", assemblyName);

                return File.Exists(archSpecificPath)
                           ? System.Reflection.Assembly.LoadFrom(archSpecificPath)
                           : null;
            }

            return null;
        }
        public string PluginAuthor => "VinZzzzTRYhard";

        public string PluginDescription => "This is Voltage Device plugin prototype";

        public string PluginVersion => "0.01";

        public string PluginName => "Voltage";

        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        Random _rand = new Random(DateTime.UtcNow.Millisecond);
        public bool InitializePlugin(string parameters)
        {
            _isInitialized = true;
            return _isInitialized;
        }
        public void MakePluginAction(string parameters)
        {
            // Nothing to do
        }
        public string GetCurrentValue()
        {
            if (!_isInitialized) return string.Empty;

            // Return random voltage from +210 -> +230
            return _rand.Next(210, 230).ToString();
        }
        public void Dispose()
        {
            _isInitialized = false;
        }

    }
}
