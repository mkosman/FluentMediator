using System;

namespace FluentMediator.Pipelines.Pipeline
{
    internal sealed class PipelineBuilder<TRequest> : IPipelineBuilder<TRequest>
    {
        private readonly IMethodCollection<Method<Action<object, object>>> _methods;
        private IDirect? _direct;
        private string? _name;

        public PipelineBuilder(string? name)
        {
            _methods = new MethodCollection<Method<Action<object, object>>>();
            _name = name;
        }

        public IPipelineBuilder<TRequest> Call<THandler>(Action<THandler, TRequest> action)
        {
            Action<object, object> typedHandler = (h, r) => action((THandler) h, (TRequest) r);
            var method = new Method<Action<object, object>>(typeof(THandler), typedHandler);
            _methods.Add(method);
            return this;
        }

        public IPipeline Return<TResult, THandler>(Func<THandler, TRequest, TResult> func)
        {
            _direct = new Direct<TRequest, TResult, THandler>(func);
            return Build();
        }

        public IPipeline Build()
        {
            return new Pipeline(_methods, _direct, typeof(TRequest), _name);
        }
    }
}