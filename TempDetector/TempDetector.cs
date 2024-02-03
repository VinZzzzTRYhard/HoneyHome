
using PluginSDK;

namespace TempDetector
{
    public class TempDetector : IPluginSDK
    {
        public TempDetector() {
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

        public string PluginDescription => "This is Temperature Device plugin prototype";

        public string PluginVersion => "0.01";

        public string PluginName => "TempDetector";

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

            // Return random temperature from +18 -> +23
            return _rand.Next(18, 23).ToString();
        }
        public void Dispose()
        {
            _isInitialized = false;
        }

    }
}
