/****** Object:  Table [smq].[Message]    Script Date: 2018-03-01 09:57:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [smq].[Message](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TimeRangeId] [int] NOT NULL,
	[Body] [varbinary](max) NOT NULL,
	[Fingerprint] [varbinary](max) NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Message] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [smq].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_TimeRange] FOREIGN KEY([TimeRangeId])
REFERENCES [smq].[TimeRange] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [smq].[Message] CHECK CONSTRAINT [FK_Message_TimeRange]
GO
/****** Object:  Table [smq].[TimeRange]    Script Date: 18/02/2018 17:31:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [smq].[TimeRange](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[QueueId] [int] NOT NULL,
	[StartsOn] [datetime2](7) NOT NULL,
	[EndsOn] [datetime2](7) NOT NULL,
	[CreatedOn]  AS (getutcdate()),
 CONSTRAINT [PK_TimeRange] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Message]    Script Date: 18/02/2018 17:31:05 ******/
CREATE NONCLUSTERED INDEX [IX_Message] ON [smq].[Message]
(
	[BodyHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Queue]    Script Date: 18/02/2018 17:31:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Queue] ON [smq].[Queue]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [smq].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_TimeRange] FOREIGN KEY([TimeRangeId])
REFERENCES [smq].[TimeRange] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [smq].[Message] CHECK CONSTRAINT [FK_Message_TimeRange]
GO
ALTER TABLE [smq].[TimeRange]  WITH CHECK ADD  CONSTRAINT [FK_TimeRange_Queue] FOREIGN KEY([QueueId])
REFERENCES [smq].[Queue] ([Id])
GO
ALTER TABLE [smq].[TimeRange] CHECK CONSTRAINT [FK_TimeRange_Queue]
GO
