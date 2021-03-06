using System.Threading.Tasks;

namespace FluentMediator.Pipelines.PipelineAsync
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPipelineAsync : INamedPipeline, ITypedPipeline
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getService"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task PublishAsync(GetService getService, object request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getService"></param>
        /// <param name="request"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        Task<TResult> SendAsync<TResult>(GetService getService, object request);
    }
}