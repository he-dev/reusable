USE [TestDb]
GO

/****** Object:  Table [dbo].[Example_SemLog3]    Script Date: 18/02/2018 17:20:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Example_SemLog3](
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
 CONSTRAINT [PK_Example_SemLog3_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Example_SemLog3_Category]    Script Date: 18/02/2018 17:20:53 ******/
CREATE NONCLUSTERED INDEX [IX_Example_SemLog3_Category] ON [dbo].[Example_SemLog3]
(
	[Category] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Example_SemLog3_Environment]    Script Date: 18/02/2018 17:20:53 ******/
CREATE NONCLUSTERED INDEX [IX_Example_SemLog3_Environment] ON [dbo].[Example_SemLog3]
(
	[Environment] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Example_SemLog3_Identifier]    Script Date: 18/02/2018 17:20:53 ******/
CREATE NONCLUSTERED INDEX [IX_Example_SemLog3_Identifier] ON [dbo].[Example_SemLog3]
(
	[Identifier] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Example_SemLog3_Layer]    Script Date: 18/02/2018 17:20:53 ******/
CREATE NONCLUSTERED INDEX [IX_Example_SemLog3_Layer] ON [dbo].[Example_SemLog3]
(
	[Layer] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Example_SemLog3_Logger]    Script Date: 18/02/2018 17:20:53 ******/
CREATE NONCLUSTERED INDEX [IX_Example_SemLog3_Logger] ON [dbo].[Example_SemLog3]
(
	[Logger] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Example_SemLog3_Product]    Script Date: 18/02/2018 17:20:53 ******/
CREATE NONCLUSTERED INDEX [IX_Example_SemLog3_Product] ON [dbo].[Example_SemLog3]
(
	[Product] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IXS_SemLog3_Level]    Script Date: 18/02/2018 17:20:53 ******/
CREATE NONCLUSTERED INDEX [IXS_SemLog3_Level] ON [dbo].[Example_SemLog3]
(
	[Level] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO


