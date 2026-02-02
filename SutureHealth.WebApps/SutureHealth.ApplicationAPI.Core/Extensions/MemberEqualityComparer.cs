using System.Collections.Generic;

namespace SutureHealth.Application.Extensions;

public class MemberIdEqualityComparer : IEqualityComparer<Member>
{
    public bool Equals(Member x, Member y)
    {
        return (x?.MemberId == y?.MemberId);
    }

    public int GetHashCode(Member obj)
    {
        return obj?.MemberId.GetHashCode() ?? 0;
    }
}
