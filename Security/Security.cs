using PluginSDK;

namespace Security
{
    public class Security : IPluginSDK
    {

        public Security()
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

        public string PluginDescription => "This is Security plugin prototype";

        public string PluginVersion => "0.01";

        public string PluginName => "Security";

        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        public bool InitializePlugin(string parameters)
        {
            _isInitialized = true;
            return _isInitialized;
        }

        public void MakePluginAction(string parameters)
        {
        }

        Random _rand = new Random(DateTime.UtcNow.Millisecond);

        public string GetCurrentValue()
        {
            if (!IsInitialized) return string.Empty;

            // Lock/Unlock
            var rand = _rand.Next(18, 180);
            if (rand / 17 < 15)
                return "Locked";
            return "Unlocked";
        }

        public void Dispose()
        {
            _isInitialized = false;
        }
    }
}
