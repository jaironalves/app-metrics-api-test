using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AppMetricsTest.UnitTests
{
    internal class ModelAsync
    {
        public int Id { get; set; }
    }

    class ModelAsyncAccessor
    {
        private static readonly AsyncLocal<ModelAsyncHolder> modelCurrent = new AsyncLocal<ModelAsyncHolder>();

        public ModelAsync ModelAsyncValue
        {
            get
            {
                return modelCurrent.Value?.Model;
            }
            set
            {
                var holder = modelCurrent.Value;
                if (holder != null)
                {
                    // Clear current trapped in the AsyncLocals, as its done.
                    holder.Model = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    modelCurrent.Value = new ModelAsyncHolder { Model = value };
                }
            }
        }

        private sealed class ModelAsyncHolder
        {
            public ModelAsync Model;
        }
    }
}
