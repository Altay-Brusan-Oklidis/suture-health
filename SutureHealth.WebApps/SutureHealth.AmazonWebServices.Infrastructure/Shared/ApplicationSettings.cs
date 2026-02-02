using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using Constructs;


namespace SutureHealth.AmazonWebServices.Infrastructure
{
    public class ApplicationSettings : Stack
    {
        public ApplicationSettings(Construct scope, string? id = null, StackProps? stackProps = null) : base(scope, id, stackProps)
        {
             
        }
    }
}
