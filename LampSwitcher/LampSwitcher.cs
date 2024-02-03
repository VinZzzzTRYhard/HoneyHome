using PluginSDK;

namespace LampSwitcher
{
    public class LampSwitcher : IPluginSDK
    {

        public LampSwitcher() {
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

        public string PluginDescription => "This is Lamp Switcher plugin prototype";

        public string PluginVersion => "0.01";

        public string PluginName => "LampSwitcher";

        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;



        private bool _currentState = false;
        public bool InitializePlugin(string parameters)
        {
            _isInitialized = true;
            return _isInitialized;
        }

        public void MakePluginAction(string parameters)
        {
            _currentState = !_currentState;
        }
        public string GetCurrentValue()
        {
            if (!IsInitialized) return string.Empty;

            if (_currentState)
                return "On";
            return "Off";
        }

        public void Dispose()
        {
            _isInitialized = false;
        }
    }
}
