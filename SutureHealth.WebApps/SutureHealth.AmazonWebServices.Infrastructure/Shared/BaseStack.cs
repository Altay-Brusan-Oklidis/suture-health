using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace SutureHealth.AmazonWebServices.Infrastructure
{
    public abstract class BaseStack : Stack
    {
        protected IVpc? Vpc { get; private set; }

        public BaseStack(Construct scope, string id, StackProps stackProps)
            : base(scope, id, stackProps)
        {
            Vpc = GetVpc(scope, id, stackProps);
            
        }


        public static IVpc? GetVpc(Construct scope, string id, StackProps stackProps)
        {
            var vpc = Amazon.CDK.AWS.EC2.Vpc.FromLookup(scope, id, new VpcLookupOptions()
            {
                VpcId = "vpc-b29e92d6",
                Tags = new Dictionary<string, string>{
                    { "Organization", "SutureHealth" },
                    { "Role", "Vpc" },
                }
            });

            if (vpc == null)
            {
                throw new NotImplementedException("need on create a VPC here");
            }

            return vpc;
        }
    }
}
