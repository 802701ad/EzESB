CREATE TABLE [dbo].[t_consumer] 
(
	[consumer_id] varchar(100) NOT NULL DEFAULT (newid()),
	[consumer_name] varchar(100) NOT NULL,
	[message_type] varchar(100) NOT NULL,
	[dest_url] varchar(2000) NOT NULL,
	PRIMARY KEY ([consumer_id]),	
);

CREATE TABLE [dbo].[t_message] 
(
	[message_id] varchar(100) NOT NULL,
	[message_type] varchar(100) NOT NULL,
	[sent_at] varchar(100) NOT NULL,
	PRIMARY KEY ([message_id])
);

CREATE TABLE [dbo].[t_message_delivery_log] 
(
	[record_id] varchar(100) NOT NULL,
	[message_id] varchar(100) NOT NULL,
	[consumer_id] varchar(100) NOT NULL,
	[status_code] varchar(100) NULL,
	[response_content] text NULL,
	[next_retry] varchar(100) NULL,
	[last_delivery_attempt] varchar(100) NOT NULL,
	[retry_count] int NULL,
	PRIMARY KEY ([message_id], [consumer_id])
);

CREATE TABLE [dbo].[t_message_values] 
(
	[message_id] varchar(100) NOT NULL,
	[name] varchar(750) NOT NULL,
	[value] text NOT NULL
);