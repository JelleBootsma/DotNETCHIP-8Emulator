using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorChip8Emulator
{
    /// <summary>
    /// This class contains all JavaScript interop required for 
    /// </summary>
    public class JsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public JsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/BlazorChip8Emulator/jsInterop.js").AsTask());
        }

        public async ValueTask ClaimFocus(ElementReference element)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("SetFocus", element);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}