CREATE
OR ALTER TRIGGER [Chat].Create_channel_message ON [Chat].[Channels]
AFTER
INSERT
    AS
        SET NOCOUNT ON;

        DECLARE @channel_id uniqueidentifier = (
            SELECT
                id
            FROM
                inserted
        );

        DECLARE @sql NVARCHAR(MAX) = N'
            CREATE TABLE [Chat].[' + CAST(@channel_id AS NVARCHAR(36)) + N '] (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [ParentId] UNIQUEIDENTIFIER NULL,
                [Content] NVARCHAR(MAX) NOT NULL,   
                [CreatedAt] DATETIMEOFFSET NOT NULL,
                [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                [UpdatedAt] DATETIMEOFFSET NULL,
                [UpdatedBy] UNIQUEIDENTIFIER NULL,
                [DeletedAt] DATETIMEOFFSET NULL,
                [DeletedBy] UNIQUEIDENTIFIER NULL,w
            );
            
            ALTER TABLE [Chat].[' + CAST(@channel_id AS NVARCHAR(36)) + N ']
                ADD CONSTRAINT [FK_' + CAST(@channel_id AS NVARCHAR(36)) + N '_ParentId]
                FOREIGN KEY ([ParentId]) REFERENCES [Chat].[' + CAST(@channel_id AS NVARCHAR(36)) + N ']([Id]);
            ';

        EXEC sp_executesql @sql;
GO