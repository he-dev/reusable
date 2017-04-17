
DECLARE @userTable as nvarchar(50) = 'U'

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Setting1' AND xtype=@userTable)
    CREATE TABLE [dbo].[Setting1] (
		[Name]  NVARCHAR (50)  NOT NULL,
		[Value] NVARCHAR (MAX) NULL,
		PRIMARY KEY CLUSTERED ([Name] ASC)
	);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Setting3' AND xtype=@userTable)
    CREATE TABLE [dbo].[Setting3] (
		[Name]        NVARCHAR (50)  NOT NULL,
		[Value]       NVARCHAR (MAX) NULL,
		[Environment] NVARCHAR (50)  NOT NULL,
		[Config]      NVARCHAR (50)  NOT NULL,
		CONSTRAINT [PK_Setting3] PRIMARY KEY CLUSTERED ([Name] ASC, [Environment] ASC, [Config] ASC)
	);