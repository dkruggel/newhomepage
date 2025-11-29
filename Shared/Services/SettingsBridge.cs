using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace newhomepage.Shared.Services
{
    public static class SettingsBridge
    {
        // Event invoked when settings change (JSON string payload)
        public static event Func<string, Task>? SettingsChanged;

        [JSInvokable("OnAppSettingsChangedFromJs")]
        public static Task OnAppSettingsChangedFromJs(string json)
        {
            var handler = SettingsChanged;
            if (handler != null)
            {
                return handler.Invoke(json);
            }
            return Task.CompletedTask;
        }
    }
}
