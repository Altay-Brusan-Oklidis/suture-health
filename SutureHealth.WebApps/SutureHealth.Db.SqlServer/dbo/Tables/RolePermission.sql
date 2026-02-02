CREATE TABLE [dbo].[RolePermission]
(
    [RoleId]       INT NOT NULL,
    [PermissionId] INT NOT NULL,
    CONSTRAINT [PK_RolePermission] PRIMARY KEY CLUSTERED ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermission_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId])
    ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_RolePermission_Permission_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permission]([PermissionId])
    ON DELETE CASCADE ON UPDATE CASCADE
)
