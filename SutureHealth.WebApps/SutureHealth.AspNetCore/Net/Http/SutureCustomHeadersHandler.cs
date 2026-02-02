using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SutureHealth.Net.Http
{
    public class SutureCustomHeadersHandler : DelegatingHandler
    {
        public const string SUTURE_UNIQUE_REQUEST_ID = "x-suture-unique-request-id";

        protected SutureCustomHeadersHandlerOptions options;

        public SutureCustomHeadersHandler(SutureCustomHeadersHandlerOptions options)
        {
            this.options = options ?? throw new ArgumentNullException("options");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.options.OnGetUniqueRequestId != null && !request.Headers.Contains(SUTURE_UNIQUE_REQUEST_ID))
            {
                string value = this.options.OnGetUniqueRequestId();

                if (value != null)
                {
                    request.Headers.TryAddWithoutValidation(SUTURE_UNIQUE_REQUEST_ID, this.options.OnGetUniqueRequestId());
                }
            }

            this.options.OnHandlerConfigured?.Invoke();

            return base.SendAsync(request, cancellationToken);
        }
    }

    public class SutureCustomHeadersHandlerOptions
    {
        public Func<string> OnGetUniqueRequestId { get; set; }
        public Action OnHandlerConfigured { get; set; }
    }
}
