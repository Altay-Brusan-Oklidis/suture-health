CREATE TABLE [dbo].[MemberMemberGroup]
(
    [MemberId] INT NOT NULL,
    [MemberGroupId] INT NOT NULL,
    CONSTRAINT [PK_MemberMemberGroup] PRIMARY KEY CLUSTERED ([MemberId], [MemberGroupId]),
    CONSTRAINT [CHK_MemberMemberGroup_MemberId] CHECK ([dbo].[MemberIdExists] (MemberId) = 1), -- TODO: should be a foreign key to the Users
    CONSTRAINT [FK_MemberMemberGroup_MemberGroup_MemberGroupId] FOREIGN KEY ([MemberGroupId]) REFERENCES [MemberGroup]([MemberGroupId]) ON DELETE CASCADE,
)
