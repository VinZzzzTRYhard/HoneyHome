namespace PluginSDK
{
    public interface IPluginSDK : IDisposable
    {
        /// <summary>
        /// returns Plugin Author
        /// </summary>
        string PluginAuthor { get; }
        /// <summary>
        /// returns Plugin Description
        /// </summary>
        string PluginDescription { get; }
        /// <summary>
        /// returns PluginVersion
        /// </summary>
        string PluginVersion { get; }
        /// <summary>
        /// Returns original Plugin Name
        /// </summary>
        string PluginName { get; }
        /// <summary>
        /// Initialize Plugin with parameters
        /// </summary>
        /// <param name="parameters">Parameters for plugin initialization</param>
        /// <returns></returns>
        bool InitializePlugin(string parameters);
        /// <summary>
        /// Returns if plugin was initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Get current Plugin Value
        /// </summary>
        /// <returns>Returns JSON as values</returns>
        string GetCurrentValue();

        /// <summary>
        /// Make Plugin Action (for example switch)
        /// </summary>
        /// <param name="parameters">Parameter which action make</param>
        void MakePluginAction(string parameters);

    }
}
