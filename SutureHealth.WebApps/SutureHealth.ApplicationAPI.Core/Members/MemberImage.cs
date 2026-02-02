using System;

namespace SutureHealth.Application;

public class MemberImage
{
    public Guid MemberImageId { get; set; }
    public int MemberId { get; set; }
    public bool IsPrimary { get; set; }
    public MemberImageSizeType? SizeType { get; set; }
    public DateTime UploadDate { get; set; }
    public bool Active { get; set; }
}

public class MemberBase64Image
{
    public int MemberId { get; set; }
    public string Image { get; set; }
}

public enum MemberImageSizeType
{
    Original = 0,
    Cropped = 1,
    Small = 2,
}