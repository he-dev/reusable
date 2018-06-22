USE [TestDb]
GO
/****** Object:  Table [dbo].[SemLog]    Script Date: 04/02/2018 16:20:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SemLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Environment] [nvarchar](50) NOT NULL,
	[Product] [nvarchar](50) NOT NULL,
	[Logger] [nvarchar](50) NOT NULL,
	[Scope] [nvarchar](max) NULL,
	[Layer] [nvarchar](50) NOT NULL,
	[Level] [nvarchar](50) NOT NULL,
	[Category] [nvarchar](50) NOT NULL,
	[Identifier] [nvarchar](50) NOT NULL,
	[Snapshot] [nvarchar](max) NOT NULL,
	[Elapsed] [bigint] NULL,
	[Message] [nvarchar](max) NULL,
	[Exception] [nvarchar](max) NULL,
	[CallerMemberName] [nvarchar](200) NULL,
	[CallerLineNumber] [int] NULL,
	[CallerFilePath] [nvarchar](200) NULL,
 CONSTRAINT [PK_SemLog_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SemLog_Category]    Script Date: 04/02/2018 16:20:29 ******/
CREATE NONCLUSTERED INDEX [IX_SemLog_Category] ON [dbo].[SemLog]
(
	[Category] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SemLog_Environment]    Script Date: 04/02/2018 16:20:29 ******/
CREATE NONCLUSTERED INDEX [IX_SemLog_Environment] ON [dbo].[SemLog]
(
	[Environment] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SemLog_Identifier]    Script Date: 04/02/2018 16:20:29 ******/
CREATE NONCLUSTERED INDEX [IX_SemLog_Identifier] ON [dbo].[SemLog]
(
	[Identifier] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SemLog_Layer]    Script Date: 04/02/2018 16:20:29 ******/
CREATE NONCLUSTERED INDEX [IX_SemLog_Layer] ON [dbo].[SemLog]
(
	[Layer] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SemLog_Logger]    Script Date: 04/02/2018 16:20:29 ******/
CREATE NONCLUSTERED INDEX [IX_SemLog_Logger] ON [dbo].[SemLog]
(
	[Logger] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SemLog_Product]    Script Date: 04/02/2018 16:20:29 ******/
CREATE NONCLUSTERED INDEX [IX_SemLog_Product] ON [dbo].[SemLog]
(
	[Product] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXS_SemLog_Level]    Script Date: 04/02/2018 16:20:29 ******/
CREATE NONCLUSTERED INDEX [IXS_SemLog_Level] ON [dbo].[SemLog]
(
	[Level] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

CREATE FULLTEXT CATALOG [SemLog]

/****** Object:  FullTextIndex     Script Date: 04/02/2018 16:20:29 ******/
CREATE FULLTEXT INDEX ON [dbo].[SemLog](
[Exception] LANGUAGE 'English', 
[Message] LANGUAGE 'English', 
[Scope] LANGUAGE 'English', 
[Snapshot] LANGUAGE 'English')
KEY INDEX [PK_SemLog_Id]ON ([SemLogTest], FILEGROUP [PRIMARY])
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)

GO
