using Amazon.CDK;
using Constructs;

namespace SutureHealth.AmazonWebServices.Infrastructure
{
    public class StackProperties : StackProps
    {
        public StackProperties()
        { }

        public string Deployment { get; internal set; }
    }
}
